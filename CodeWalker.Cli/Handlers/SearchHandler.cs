using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Text.Json;
using System.Threading;

using CodeWalker.Cli.Helpers;
using CodeWalker.GameFiles;

namespace CodeWalker.Cli.Handlers;

internal sealed record SearchOptions
{
    public required string RpfPath { get; init; }
    public required string ExePath { get; init; }
    public required bool Gen9 { get; init; }
    public required string[] Filters { get; init; }
    public required bool Verbose { get; init; }
    public required bool Json { get; init; }
    public required bool Recursive { get; init; }
    public required SizeFormat SizeFormat { get; init; }
    public required string Pattern { get; init; }
    public string? DirPath { get; init; }
}

internal static class SearchHandler
{
    public static Command CreateCommand(CancellationToken cancellationToken = default)
    {
        Option<FileInfo> rpfOpt = CliOptions.Rpf();
        rpfOpt.Required = false;
        rpfOpt.Description = "Path to the RPF file; use --dir to search a folder instead";
        Option<DirectoryInfo> exeOpt = CliOptions.Exe();
        Option<bool> gen9Opt = CliOptions.Gen9();
        Option<string[]> filterOpt = CliOptions.Filter();
        Option<bool> recursiveOpt = CliOptions.Recursive();
        Option<bool> verboseOpt = CliOptions.Verbose();
        Option<bool> jsonOpt = CliOptions.Json();
        Option<bool> siOpt = CliOptions.Si();

        Option<DirectoryInfo> dirOpt = new("--dir", "-D")
        {
            Description = "Directory to search; every .rpf below it is searched",
        };

        Argument<string> patternArg = new("pattern")
        {
            Description = "Substring to search for in file paths",
        };

        Command command = new("search", "Search for files by name or path in one or more RPF archives")
        {
            patternArg,
            rpfOpt,
            exeOpt,
            gen9Opt,
            filterOpt,
            recursiveOpt,
            verboseOpt,
            jsonOpt,
            siOpt,
            dirOpt,
        };
        command.Aliases.Add("s");

        command.Validators.Add(result =>
        {
            bool hasRpf = result.GetValue(rpfOpt) != null;
            bool hasDir = result.GetValue(dirOpt) != null;
            if (hasRpf == hasDir)
                result.AddError("Specify exactly one of --rpf or --dir.");
        });

        command.SetAction(parseResult =>
        {
            SearchOptions options = new()
            {
                RpfPath = parseResult.GetValue(rpfOpt)?.FullName ?? "",
                ExePath = parseResult.GetRequiredValue(exeOpt).FullName,
                Gen9 = parseResult.GetValue(gen9Opt),
                Filters = Filter.Normalize(parseResult.GetValue(filterOpt)),
                Verbose = parseResult.GetValue(verboseOpt),
                Json = parseResult.GetValue(jsonOpt),
                Recursive = parseResult.GetValue(recursiveOpt),
                SizeFormat = parseResult.GetValue(siOpt) ? SizeFormat.SI : SizeFormat.IEC,
                Pattern = parseResult.GetRequiredValue(patternArg),
                DirPath = parseResult.GetValue(dirOpt)?.FullName,
            };
            return Execute(options, cancellationToken);
        });

        return command;
    }

    public static int Execute(SearchOptions options, CancellationToken cancellationToken = default)
    {
        if (options.DirPath != null)
            return ExecuteDirectory(options, cancellationToken);

        string? initError = RpfHelper.ValidateAndLoadKeys(
            options.RpfPath,
            options.ExePath,
            options.Gen9,
            options.Json
        );
        if (initError != null)
        {
            return Output.ReportError(
                initError,
                options.Json,
                ErrorResult([], options)
            );
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
                Console.Error.WriteLine();

            Json.SearchResult result = CollectSearch(rpf, scanErrors, options, cancellationToken: cancellationToken);

            if (options.Json)
                PrintJsonSearch(result);
            else
                PrintSearch(result, options);

            return scanErrors.Count > 0 ? 1 : 0;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return Output.ReportError(
                ex.Message,
                options.Json,
                ErrorResult([.. scanErrors], options),
                options.Verbose ? ex.StackTrace : null
            );
        }
    }

    internal static int ExecuteDirectory(SearchOptions options, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(options.DirPath))
        {
            return Output.ReportError(
                $"Directory not found: {options.DirPath}",
                options.Json,
                ErrorResult([], options)
            );
        }

        string[] rpfPaths = Directory.GetFiles(options.DirPath!, "*.rpf", SearchOption.AllDirectories);
        Array.Sort(rpfPaths, StringComparer.OrdinalIgnoreCase);

        if (rpfPaths.Length == 0)
        {
            return Output.ReportError(
                $"No .rpf files found in: {options.DirPath}",
                options.Json,
                ErrorResult([], options)
            );
        }

        string? initError = RpfHelper.ValidateExeAndLoadKeys(
            options.ExePath,
            options.Gen9,
            options.Json
        );
        if (initError != null)
        {
            return Output.ReportError(
                initError,
                options.Json,
                ErrorResult([], options)
            );
        }

        List<string> allScanErrors = [];
        List<Json.SearchMatch> allMatches = [];
        List<string> rpfFiles = [];

        foreach (string rpfPath in rpfPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<string> scanErrors = [];
            try
            {
                RpfFile rpf = RpfHelper.OpenRpf(
                    rpfPath,
                    options.Verbose,
                    options.Json,
                    scanErrors
                );

                Json.SearchResult partialResult = CollectSearch(rpf, scanErrors, options, archive: rpfPath, cancellationToken: cancellationToken);
                rpfFiles.Add(rpfPath);

                allMatches.AddRange(partialResult.Matches);
                allScanErrors.AddRange(scanErrors);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                allScanErrors.Add($"{rpfPath}: {ex.Message}");
            }
        }

        if (!options.Json)
            Console.Error.WriteLine();

        Json.SearchResult result = new()
        {
            Success = allScanErrors.Count == 0,
            RpfFile = options.DirPath!,
            RpfFiles = rpfFiles,
            Pattern = options.Pattern,
            MatchCount = allMatches.Count,
            Matches = allMatches,
            ErrorMessages = [.. allScanErrors],
        };

        if (options.Json)
            PrintJsonSearch(result);
        else
            PrintSearch(result, options);

        return allScanErrors.Count > 0 ? 1 : 0;
    }

    internal static Json.SearchResult ErrorResult(string[] errorMessages, SearchOptions options) =>
        new()
        {
            Success = false,
            RpfFile = options.RpfPath,
            RpfFiles = [],
            Pattern = options.Pattern,
            MatchCount = 0,
            Matches = [],
            ErrorMessages = errorMessages,
        };

    internal static Json.SearchResult CollectSearch(
        RpfFile rpf,
        List<string> scanErrors,
        SearchOptions options,
        string? archive = null,
        CancellationToken cancellationToken = default)
    {
        string archivePath = archive ?? options.RpfPath;
        string normalizedPattern = options.Pattern.Replace('\\', '/');

        List<RpfEntry> allEntries = [];
        CollectAllEntries(rpf, options.Recursive, allEntries);

        List<Json.SearchMatch> matches = [];

        foreach (RpfEntry entry in allEntries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Filter.Matches(entry.Path ?? "", options.Filters))
                continue;

            if (entry.Path?.Replace('\\', '/').Contains(normalizedPattern, StringComparison.OrdinalIgnoreCase) != true)
                continue;

            long size = 0;
            string type = "directory";
            string ext = "";

            if (entry is RpfFileEntry fileEntry)
            {
                size = fileEntry.GetFileSize();
                type = RpfHelper.GetFileType(fileEntry);
                ext = Path.GetExtension(fileEntry.Name).ToLowerInvariant();
            }

            matches.Add(new Json.SearchMatch
            {
                Archive = archivePath,
                Path = entry.Path ?? entry.Name ?? "",
                Name = entry.Name ?? "",
                Size = size,
                Type = type,
                Extension = ext,
            });
        }

        return new Json.SearchResult
        {
            Success = scanErrors.Count == 0,
            RpfFile = archivePath,
            RpfFiles = [archivePath],
            Pattern = options.Pattern,
            MatchCount = matches.Count,
            Matches = matches,
            ErrorMessages = [.. scanErrors],
        };
    }

    internal static void PrintJsonSearch(Json.SearchResult result) =>
        Console.WriteLine(JsonSerializer.Serialize(result, Output.JsonSerializerOptions));

    internal static void PrintSearch(Json.SearchResult result, SearchOptions options)
    {
        bool multiArchive = result.RpfFiles.Count > 1;
        string? lastArchive = null;

        foreach (Json.SearchMatch match in result.Matches)
        {
            if (multiArchive && match.Archive != lastArchive)
            {
                if (lastArchive != null)
                    Console.Error.WriteLine();
                Console.Error.WriteLine($"== {RelativePath(result.RpfFile, match.Archive)} ==");
                lastArchive = match.Archive;
            }

            if (options.Verbose)
            {
                string sizeStr = options
                    .SizeFormat.ToFormattedString(match.Size)
                    .PadLeft(12);
                Console.WriteLine($"{sizeStr}  {match.Path}");
            }
            else
            {
                Console.WriteLine(match.Path);
            }
        }

        string matchWord = result.MatchCount == 1 ? "match" : "matches";
        Console.Error.WriteLine();
        Console.Error.WriteLine(
            multiArchive
                ? $"Found {result.MatchCount} {matchWord} across {result.RpfFiles.Count} archive(s) for '{result.Pattern}'"
                : $"Found {result.MatchCount} {matchWord} for '{result.Pattern}'"
        );
    }

    internal static string RelativePath(string basePath, string fullPath)
    {
        string normalizedBase = basePath.Replace('\\', '/').TrimEnd('/') + "/";
        string normalizedFull = fullPath.Replace('\\', '/');

        return normalizedFull.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase)
            ? normalizedFull[normalizedBase.Length..]
            : Path.GetFileName(fullPath);
    }

    internal static void CollectAllEntries(RpfFile rpf, bool recursive, List<RpfEntry> entries)
    {
        if (rpf.AllEntries != null)
            entries.AddRange(rpf.AllEntries);

        if (!recursive || rpf.Children == null)
            return;

        foreach (RpfFile child in rpf.Children)
            CollectAllEntries(child, recursive, entries);
    }
}
