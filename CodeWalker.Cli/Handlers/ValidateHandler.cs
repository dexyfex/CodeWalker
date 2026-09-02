using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using CodeWalker.Cli.Helpers;
using CodeWalker.GameFiles;

namespace CodeWalker.Cli.Handlers;

internal sealed record ValidateOptions
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
    public required bool Progress { get; init; }
}

internal static class ValidateHandler
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
        Option<bool> progressOpt = CliOptions.Progress();

        Command command = new("validate", "Validate game file integrity by parsing RPF contents")
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
            progressOpt,
        };
        command.Aliases.Add("val");

        command.SetAction(parseResult =>
        {
            ValidateOptions options = new()
            {
                RpfPath = parseResult.GetRequiredValue(rpfOpt).FullName,
                ExePath = parseResult.GetRequiredValue(exeOpt).FullName,
                Gen9 = parseResult.GetValue(gen9Opt),
                Filters = Filter.Normalize(parseResult.GetValue(filterOpt)),
                Verbose = parseResult.GetValue(verboseOpt),
                Json = parseResult.GetValue(jsonOpt),
                Recursive = parseResult.GetValue(recursiveOpt),
                Threads = parseResult.GetValue(threadsOpt),
                SizeFormat = parseResult.GetValue(siOpt) ? SizeFormat.SI : SizeFormat.IEC,
                Progress = parseResult.GetValue(progressOpt),
            };
            return Execute(options, cancellationToken);
        });

        return command;
    }

    public static int Execute(ValidateOptions options, CancellationToken cancellationToken = default)
    {
        Json.ValidateResult ErrorResult(string[] errorMessages) =>
            new()
            {
                Success = false,
                RpfFile = options.RpfPath,
                TotalFiles = 0,
                Valid = 0,
                Warnings = 0,
                Errors = 0,
                Skipped = 0,
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

            if (!options.Json)
            {
                Console.Error.WriteLine();
            }

            List<(RpfFile rpf, RpfFileEntry entry)> entries = RpfHelper.CollectFiles(
                rpf,
                options.Filters,
                options.Recursive
            );

            Json.ValidateFileEntry?[] results = new Json.ValidateFileEntry?[entries.Count];
            object consoleLock = new();

            using (ProgressBar progress = new(entries.Count, options.Progress && !options.Json))
            {
                _ = Parallel.For(
                    0,
                    entries.Count,
                    new ParallelOptions { MaxDegreeOfParallelism = options.Threads, CancellationToken = cancellationToken },
                    i =>
                    {
                        (_, RpfFileEntry fileEntry) = entries[i];
                        string ext = Path.GetExtension(fileEntry.Name).ToLowerInvariant();

                        try
                        {
                            (string status, string? message) = ValidateFile(
                                fileEntry,
                                ext
                            );

                            results[i] = new Json.ValidateFileEntry
                            {
                                Path = fileEntry.Path,
                                Name = fileEntry.Name,
                                Status = status,
                                Message = message,
                            };

                            if (
                                !options.Json
                                && !options.Progress
                                && (status == "warning" || status == "error")
                            )
                            {
                                lock (consoleLock)
                                {
                                    Console.Error.WriteLine(
                                        $"[{status.ToUpperInvariant()}] {fileEntry.Path}: {message}"
                                    );
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            results[i] = new Json.ValidateFileEntry
                            {
                                Path = fileEntry.Path,
                                Name = fileEntry.Name,
                                Status = "error",
                                Message = ex.Message,
                            };

                            if (!options.Json && !options.Progress)
                            {
                                lock (consoleLock)
                                {
                                    Console.Error.WriteLine(
                                        $"[ERROR] {fileEntry.Path}: {ex.Message}"
                                    );
                                }
                            }
                        }

                        progress.Increment(fileEntry.Path);
                    }
                );
            }

            List<Json.ValidateFileEntry> nonNull = results.OfType<Json.ValidateFileEntry>().ToList();
            int valid = nonNull.Count(e => e.Status == "valid");
            int warnings = nonNull.Count(e => e.Status == "warning");
            int errors = nonNull.Count(e => e.Status == "error");
            int skipped = nonNull.Count(e => e.Status == "skipped");

            // In verbose mode or JSON, include all; otherwise only warnings/errors
            List<Json.ValidateFileEntry> files = (options.Json || options.Verbose)
                ? nonNull
                : nonNull.Where(e => e.Status is "warning" or "error").ToList();

            Json.ValidateResult result = new()
            {
                Success = errors == 0 && scanErrors.Count == 0,
                RpfFile = options.RpfPath,
                TotalFiles = entries.Count,
                Valid = valid,
                Warnings = warnings,
                Errors = errors,
                Skipped = skipped,
                Files = files,
                ErrorMessages = [.. scanErrors],
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
                Console.Error.WriteLine(
                    $"Validation complete: {valid} valid, {warnings} warnings, {errors} errors, {skipped} skipped"
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

    private static (string status, string? message) ValidateFile(
        RpfFileEntry fileEntry,
        string ext
    )
    {
        switch (ext)
        {
            case ".ytd":
                {
                    YtdFile file = RpfFile.GetFile<YtdFile>(fileEntry);
                    if (file == null)
                        return ("error", "Failed to load YTD file");
                    if (file.TextureDict?.Textures?.data_items == null || file.TextureDict.Textures.data_items.Length == 0)
                        return ("warning", "Texture dictionary is empty");
                    return ("valid", null);
                }
            case ".ydr":
                {
                    YdrFile file = RpfFile.GetFile<YdrFile>(fileEntry);
                    if (file == null)
                        return ("error", "Failed to load YDR file");
                    if (file.Drawable == null)
                        return ("error", "Drawable is null");
                    return ("valid", null);
                }
            case ".ydd":
                {
                    YddFile file = RpfFile.GetFile<YddFile>(fileEntry);
                    if (file == null)
                        return ("error", "Failed to load YDD file");
                    if (file.DrawableDict == null)
                        return ("error", "DrawableDict is null");
                    return ("valid", null);
                }
            case ".yft":
                {
                    YftFile file = RpfFile.GetFile<YftFile>(fileEntry);
                    if (file == null)
                        return ("error", "Failed to load YFT file");
                    if (file.Fragment == null)
                        return ("error", "Fragment is null");
                    return ("valid", null);
                }
            case ".ymap":
                {
                    YmapFile file = RpfFile.GetFile<YmapFile>(fileEntry);
                    if (file == null)
                        return ("error", "Failed to load YMAP file");
                    if (file.AllEntities == null || file.AllEntities.Length == 0)
                        return ("warning", "No entities found");
                    return ("valid", null);
                }
            case ".ytyp":
                {
                    YtypFile file = RpfFile.GetFile<YtypFile>(fileEntry);
                    if (file == null)
                        return ("error", "Failed to load YTYP file");
                    if (file.AllArchetypes == null || file.AllArchetypes.Length == 0)
                        return ("warning", "No archetypes found");
                    return ("valid", null);
                }
            case ".ybn":
                {
                    YbnFile file = RpfFile.GetFile<YbnFile>(fileEntry);
                    if (file == null)
                        return ("error", "Failed to load YBN file");
                    if (file.Bounds == null)
                        return ("error", "Bounds is null");
                    return ("valid", null);
                }
            case ".awc":
                {
                    AwcFile file = RpfFile.GetFile<AwcFile>(fileEntry);
                    if (file == null)
                        return ("error", "Failed to load AWC file");
                    if (file.Streams == null || file.Streams.Length == 0)
                        return ("warning", "No audio streams found");
                    return ("valid", null);
                }
            case ".gxt2":
                {
                    Gxt2File file = RpfFile.GetFile<Gxt2File>(fileEntry);
                    if (file == null)
                        return ("error", "Failed to load GXT2 file");
                    if (file.TextEntries == null || file.TextEntries.Length == 0)
                        return ("warning", "No text entries found");
                    return ("valid", null);
                }
            default:
                return ("skipped", null);
        }
    }
}
