using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using CodeWalker.Cli.Helpers;
using CodeWalker.GameFiles;

namespace CodeWalker.Cli.Handlers;

internal sealed record ExtractOptions
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
    public required string? OutputPath { get; init; }
    public required bool DryRun { get; init; }
    public required bool NoOverwrite { get; init; }
    public required bool Progress { get; init; }
}

internal static class ExtractHandler
{
    public static Command CreateCommand(CancellationToken cancellationToken = default)
    {
        Option<FileInfo> rpfOpt = CliOptions.Rpf();
        Option<DirectoryInfo> exeOpt = CliOptions.Exe();
        Option<bool> gen9Opt = CliOptions.Gen9();
        Option<string[]> filterOpt = CliOptions.Filter();
        Option<bool> recursiveOpt = CliOptions.Recursive();
        Option<bool> verboseOpt = CliOptions.Verbose();
        Option<bool> jsonOpt = CliOptions.Json();
        Option<bool> siOpt = CliOptions.Si();
        Option<int> threadsOpt = CliOptions.Threads();
        Option<DirectoryInfo> outputOption = CliOptions.OutputDir();
        Option<bool> dryRunOption = CliOptions.DryRun();
        Option<bool> noOverwriteOption = CliOptions.NoOverwrite();
        Option<bool> progressOption = CliOptions.Progress();

        Command command = new("extract", "Extract files from an RPF archive")
        {
            rpfOpt,
            exeOpt,
            gen9Opt,
            filterOpt,
            recursiveOpt,
            verboseOpt,
            jsonOpt,
            siOpt,
            threadsOpt,
            outputOption,
            dryRunOption,
            noOverwriteOption,
            progressOption,
        };
        command.Aliases.Add("x");

        command.SetAction(parseResult =>
        {
            ExtractOptions options = new()
            {
                RpfPath = parseResult.GetValue(rpfOpt)?.FullName ?? "",
                ExePath = parseResult.GetRequiredValue(exeOpt).FullName,
                Gen9 = parseResult.GetValue(gen9Opt),
                Filters = Filter.Normalize(parseResult.GetValue(filterOpt)),
                Verbose = parseResult.GetValue(verboseOpt),
                Json = parseResult.GetValue(jsonOpt),
                Recursive = parseResult.GetValue(recursiveOpt),
                Threads = parseResult.GetValue(threadsOpt),
                SizeFormat = parseResult.GetValue(siOpt) ? SizeFormat.SI : SizeFormat.IEC,
                OutputPath = parseResult.GetValue(outputOption)?.FullName,
                DryRun = parseResult.GetValue(dryRunOption),
                NoOverwrite = parseResult.GetValue(noOverwriteOption),
                Progress = parseResult.GetValue(progressOption),
            };
            return Execute(options, cancellationToken);
        });

        return command;
    }

    public static int Execute(ExtractOptions options, CancellationToken cancellationToken = default)
    {
        Json.ExtractResult ErrorResult(string[] errorMessages) =>
            new()
            {
                Success = false,
                RpfFile = options.RpfPath,
                OutputDir = options.OutputPath ?? Directory.GetCurrentDirectory(),
                TotalFiles = 0,
                Extracted = 0,
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
        {
            return Output.ReportError(initError, options.Json, ErrorResult([]));
        }

        List<string> scanErrors = [];
        try
        {
            RpfFile rpf = RpfHelper.OpenRpf(
                options.RpfPath,
                options.Verbose,
                options.Json,
                scanErrors
            );

            if (!options.Json && options.DryRun)
            {
                Console.Error.WriteLine("Dry run mode - no files will be extracted");
            }

            string outputDir = options.OutputPath ?? Directory.GetCurrentDirectory();

            if (!options.DryRun && !Directory.Exists(outputDir))
            {
                _ = Directory.CreateDirectory(outputDir);
            }

            List<(RpfFile rpf, RpfFileEntry entry)> filesToExtract = RpfHelper.CollectFiles(
                rpf,
                options.Filters,
                options.Recursive
            );

            int skipped = 0;

            // Process files in parallel, storing results by index to preserve order
            (Json.FileEntry? jsonEntry, string? errorMessage)[] results =
                new (Json.FileEntry?, string?)[filesToExtract.Count];

            object consoleLock = new();

            using (
                ProgressBar progress = new(
                    filesToExtract.Count,
                    options.Progress && !options.Json
                )
            )
            {
                _ = Parallel.For(
                    0,
                    filesToExtract.Count,
                    new ParallelOptions { MaxDegreeOfParallelism = options.Threads, CancellationToken = cancellationToken },
                    i =>
                    {
                        (RpfFile sourceRpf, RpfFileEntry fileEntry) = filesToExtract[i];
                        try
                        {
                            string relativePath = fileEntry.Path;
                            string outputPath = Path.Combine(
                                outputDir,
                                relativePath.Replace("\\", Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                            );
                            string? fileDir = Path.GetDirectoryName(outputPath);

                            long size = fileEntry.GetFileSize();
                            string ext = Path.GetExtension(fileEntry.Name).ToLowerInvariant();

                            Json.FileEntry jsonEntry = new()
                            {
                                Path = fileEntry.Path,
                                Name = fileEntry.Name,
                                Size = size,
                                SizeFormatted = options.SizeFormat.ToFormattedString(size),
                                Type = RpfHelper.GetFileType(fileEntry),
                                Extension = ext,
                            };

                            if (options.DryRun)
                            {
                                if (options.Verbose && !options.Json)
                                {
                                    lock (consoleLock)
                                    {
                                        Console.WriteLine($"Would extract: {fileEntry.Path}");
                                    }
                                }
                                results[i] = (jsonEntry with { Status = "dry_run" }, null);
                            }
                            else if (options.NoOverwrite && File.Exists(outputPath))
                            {
                                _ = Interlocked.Increment(ref skipped);
                                if (options.Verbose && !options.Json && !options.Progress)
                                {
                                    lock (consoleLock)
                                    {
                                        Console.Error.WriteLine(
                                            $"Skipping (exists): {fileEntry.Path}"
                                        );
                                    }
                                }
                                results[i] = (jsonEntry with { Status = "skipped" }, null);
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(fileDir))
                                {
                                    _ = Directory.CreateDirectory(fileDir);
                                }

                                if (options.Verbose && !options.Json && !options.Progress)
                                {
                                    lock (consoleLock)
                                    {
                                        Console.Error.WriteLine($"Extracting: {fileEntry.Path}");
                                    }
                                }

                                byte[]? data = sourceRpf.ExtractFile(fileEntry);
                                if (data != null)
                                {
                                    File.WriteAllBytes(outputPath, data);
                                    results[i] = (
                                        jsonEntry with { Status = "extracted" },
                                        null
                                    );
                                }
                                else
                                {
                                    if (options.Verbose && !options.Json)
                                    {
                                        lock (consoleLock)
                                        {
                                            Console.Error.WriteLine(
                                                $"Warning: Failed to extract {fileEntry.Path}"
                                            );
                                        }
                                    }
                                    results[i] = (
                                        null,
                                        $"Failed to extract: {fileEntry.Path}"
                                    );
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
                                        $"Error extracting {fileEntry.Path}: {ex.Message}"
                                    );
                                }
                            }
                            results[i] = (
                                null,
                                $"Error extracting {fileEntry.Path}: {ex.Message}"
                            );
                            progress.Increment();
                        }
                    }
                );
            }

            int extracted = 0;
            int errors = 0;
            List<Json.FileEntry> files = [];
            List<string> errorMessages = [.. scanErrors];

            foreach ((Json.FileEntry? jsonEntry, string? errorMessage) in results)
            {
                if (errorMessage == null && jsonEntry?.Status is "extracted" or "dry_run")
                    extracted++;

                if (jsonEntry != null)
                    files.Add(jsonEntry);

                if (errorMessage != null)
                {
                    errors++;
                    errorMessages.Add(errorMessage);
                }
            }

            Json.ExtractResult result = new()
            {
                Success = errors == 0 && scanErrors.Count == 0,
                RpfFile = options.RpfPath,
                OutputDir = options.OutputPath ?? Directory.GetCurrentDirectory(),
                TotalFiles = filesToExtract.Count,
                Extracted = extracted,
                Skipped = skipped,
                Errors = errors,
                DryRun = options.DryRun,
                Files = [.. files],
                ErrorMessages = [.. errorMessages],
            };

            if (options.Json)
            {
                Console.WriteLine(
                    JsonSerializer.Serialize(result, Output.JsonSerializerOptions)
                );
            }
            else
            {
                Console.Error.WriteLine();
                string action = options.DryRun ? "would be extracted" : "extracted";
                Console.Error.WriteLine(
                    $"Extraction complete: {extracted} files {action}, {skipped} skipped, {errors} errors"
                );
            }

            return (errors > 0 || scanErrors.Count > 0) ? 1 : 0;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return Output.ReportError(
                ex.Message,
                options.Json,
                ErrorResult([.. scanErrors]),
                options.Verbose ? ex.StackTrace : null
            );
        }
    }
}
