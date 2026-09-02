using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using CodeWalker.GameFiles;

namespace CodeWalker.Cli.Helpers;

/// <summary>
/// Delegate for processing a single file entry during export.
/// Returns an <see cref="Json.ExportFileEntry"/> on success (status = "exported", "unsupported", "skipped"),
/// or a tuple with a null entry and error string on failure.
/// </summary>
/// <param name="fileEntry">The RPF file entry to process.</param>
/// <param name="data">The raw file data extracted from the RPF.</param>
/// <param name="fileOutputDir">The output directory for this file (includes relative path).</param>
/// <param name="noOverwrite">When true, skip files that already exist at the output path.</param>
internal delegate (Json.ExportFileEntry? entry, string? error) ExportFileProcessor(
    RpfFileEntry fileEntry,
    byte[] data,
    string fileOutputDir,
    bool noOverwrite
);

internal sealed record ExportOptions
{
    public required string RpfPath { get; init; }
    public required string ExePath { get; init; }
    public required bool Gen9 { get; init; }
    public required string[] Filters { get; init; }
    public required bool Verbose { get; init; }
    public required bool Json { get; init; }
    public required bool Recursive { get; init; }
    public required int Threads { get; init; }
    public required SizeFormat SizeFormat { get; init; }
    public required string OutputPath { get; init; }
    public required bool DryRun { get; init; }
    public required bool NoOverwrite { get; init; }
    public required bool Progress { get; init; }
}

internal static class ExportPipeline
{
    internal readonly record struct ExportAggregation
    {
        public required int Exported { get; init; }
        public required int Skipped { get; init; }
        public required int Errors { get; init; }
        public required IReadOnlyList<Json.ExportFileEntry> Files { get; init; }
        public required IReadOnlyList<string> ErrorMessages { get; init; }
    }

    internal static (Json.ExportFileEntry? entry, string? error) ProcessSingleFile(
        RpfFileEntry fileEntry,
        byte[]? data,
        string outputDir,
        bool dryRun,
        bool noOverwrite,
        ExportFileProcessor processor
    )
    {
        // RPF entry paths are separated with backslashes. Path.GetDirectoryName only
        // recognises the platform separator, so they must be translated first or the
        // whole path reads as a bare file name and every export lands in the root.
        string relativePath =
            Path.GetDirectoryName(fileEntry.Path.Replace('\\', Path.DirectorySeparatorChar))
            ?? "";

        string fileOutputDir = Path.Combine(outputDir, relativePath);

        if (dryRun)
        {
            return (
                new Json.ExportFileEntry
                {
                    Path = fileEntry.Path,
                    Name = fileEntry.Name,
                    OutputFiles = 0,
                    Status = "dry_run",
                },
                null
            );
        }

        if (data == null)
            return (null, $"Failed to extract: {fileEntry.Path}");

        (Json.ExportFileEntry? entry, string? error) = processor(
            fileEntry,
            data,
            fileOutputDir,
            noOverwrite
        );

        if (error != null)
            return (entry, error);

        if (entry != null)
            return (entry, null);

        return (null, $"No result for: {fileEntry.Path}");
    }

    internal static ExportAggregation AggregateResults(
        (Json.ExportFileEntry? jsonEntry, string? errorMessage)[] results,
        IReadOnlyList<string> scanErrors
    )
    {
        int exported = 0;
        int skipped = 0;
        int errors = 0;
        List<Json.ExportFileEntry> files = [];
        List<string> errorMessages = [.. scanErrors];

        foreach ((Json.ExportFileEntry? jsonEntry, string? errorMessage) in results)
        {
            if (errorMessage == null && jsonEntry?.Status is "exported" or "dry_run")
                exported++;

            if (errorMessage == null && jsonEntry?.Status is "unsupported" or "skipped")
                skipped++;

            if (jsonEntry != null)
                files.Add(jsonEntry);

            if (errorMessage != null)
            {
                errors++;
                errorMessages.Add(errorMessage);
            }
            else if (jsonEntry?.Status == "error")
            {
                errors++;
                errorMessages.Add($"Error processing: {jsonEntry.Path}");
            }
        }

        return new ExportAggregation
        {
            Exported = exported,
            Skipped = skipped,
            Errors = errors,
            Files = files,
            ErrorMessages = errorMessages,
        };
    }

    public static int Execute(
        ExportOptions options,
        string format,
        string summaryLabel,
        ExportFileProcessor processor,
        CancellationToken cancellationToken = default
    )
    {
        Json.ExportResult ErrorResult(string[] errorMessages) =>
            new()
            {
                Success = false,
                RpfFile = options.RpfPath,
                OutputDir = options.OutputPath,
                Format = format,
                TotalFiles = 0,
                Exported = 0,
                Skipped = 0,
                Errors = 0,
                DryRun = options.DryRun,
                Files = [],
                ErrorMessages = errorMessages,
            };

        string? initError = RpfHelper.ValidateAndLoadKeys(
            options.RpfPath,
            options.ExePath,
            options.Gen9,
            options.Json
        );
        if (initError != null)
            return Output.ReportError(initError, options.Json, ErrorResult([]));

        try
        {
            List<string> scanErrors = [];
            RpfFile rpf = RpfHelper.OpenRpf(
                options.RpfPath,
                options.Verbose,
                options.Json,
                scanErrors
            );

            if (!options.Json && options.DryRun)
                Console.Error.WriteLine("Dry run mode - no files will be exported");

            string outputDir = options.OutputPath;

            if (!options.DryRun && !Directory.Exists(outputDir))
                _ = Directory.CreateDirectory(outputDir);

            List<(RpfFile rpf, RpfFileEntry entry)> filesToExport = RpfHelper.CollectFiles(
                rpf,
                options.Filters,
                options.Recursive
            );

            (Json.ExportFileEntry? jsonEntry, string? errorMessage)[] results =
                new (Json.ExportFileEntry?, string?)[filesToExport.Count];

            object consoleLock = new();

            using (
                ProgressBar progress = new(
                    filesToExport.Count,
                    options is { Progress: true, Json: false }
                )
            )
            {
                _ = Parallel.For(
                    0,
                    filesToExport.Count,
                    new ParallelOptions { MaxDegreeOfParallelism = options.Threads, CancellationToken = cancellationToken },
                    i =>
                    {
                        (RpfFile sourceRpf, RpfFileEntry fileEntry) = filesToExport[i];
                        try
                        {
                            byte[]? data = options.DryRun
                                ? null
                                : sourceRpf.ExtractFile(fileEntry);

                            (Json.ExportFileEntry? entry, string? error) result = ProcessSingleFile(
                                fileEntry,
                                data,
                                outputDir,
                                options.DryRun,
                                options.NoOverwrite,
                                processor
                            );

                            results[i] = result;

                            if (
                                result.entry != null
                                && options is { Verbose: true, Json: false, Progress: false }
                            )
                            {
                                if (options.DryRun)
                                {
                                    lock (consoleLock)
                                    {
                                        Console.WriteLine(
                                            $"Would export: {fileEntry.Path}"
                                        );
                                    }
                                }
                                else if (result.entry.Status == "exported")
                                {
                                    lock (consoleLock)
                                    {
                                        Console.Error.WriteLine(
                                            $"Exported: {fileEntry.Path} -> {result.entry.OutputFiles} file(s)"
                                        );
                                    }
                                }
                            }

                            progress.Increment(fileEntry.Path);
                        }
                        catch (Exception ex)
                        {
                            if (!options.Json)
                            {
                                lock (consoleLock)
                                {
                                    Console.Error.WriteLine(
                                        $"Error exporting {fileEntry.Path}: {ex.Message}"
                                    );
                                }
                            }
                            results[i] = (
                                null,
                                $"Error exporting {fileEntry.Path}: {ex.Message}"
                            );
                            progress.Increment();
                        }
                    }
                );
            }

            ExportAggregation agg = AggregateResults(results, scanErrors);

            Json.ExportResult jsonResult = new()
            {
                Success = agg.ErrorMessages.Count == 0,
                RpfFile = options.RpfPath,
                OutputDir = options.OutputPath,
                Format = format,
                TotalFiles = filesToExport.Count,
                Exported = agg.Exported,
                Skipped = agg.Skipped,
                Errors = agg.Errors,
                DryRun = options.DryRun,
                Files = agg.Files,
                ErrorMessages = agg.ErrorMessages,
            };

            if (options.Json)
            {
                Console.WriteLine(
                    JsonSerializer.Serialize(jsonResult, Output.JsonSerializerOptions)
                );
            }
            else
            {
                Console.Error.WriteLine();
                string action = options.DryRun ? "would be exported" : "exported";
                Console.Error.WriteLine(
                    $"{summaryLabel} export complete: {agg.Exported} files {action}, {agg.Skipped} skipped, {agg.Errors} errors"
                );
            }

            return agg.ErrorMessages.Count > 0 ? 1 : 0;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return Output.ReportError(
                ex.Message,
                options.Json,
                ErrorResult([]),
                options.Verbose ? ex.StackTrace : null
            );
        }
    }
}
