using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Text.Json;
using System.Threading;

using CodeWalker.Cli.Helpers;
using CodeWalker.GameFiles;

namespace CodeWalker.Cli.Handlers;

internal sealed record ListOptions
{
    public required string RpfPath { get; init; }
    public required string ExePath { get; init; }
    public required bool Gen9 { get; init; }
    public required string[] Filters { get; init; }
    public required bool Verbose { get; init; }
    public required bool Json { get; init; }
    public required bool Recursive { get; init; }
    public required SizeFormat SizeFormat { get; init; }
}

internal static class ListHandler
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

        Command command = new("list", "List contents of an RPF archive")
        {
            rpfOpt,
            exeOpt,
            gen9Opt,
            filterOpt,
            recursiveOpt,
            verboseOpt,
            jsonOpt,
            siOpt,
        };
        command.Aliases.Add("l");

        command.SetAction(parseResult =>
        {
            ListOptions options = new()
            {
                RpfPath = parseResult.GetValue(rpfOpt)?.FullName ?? "",
                ExePath = parseResult.GetRequiredValue(exeOpt).FullName,
                Gen9 = parseResult.GetValue(gen9Opt),
                Filters = Filter.Normalize(parseResult.GetValue(filterOpt)),
                Verbose = parseResult.GetValue(verboseOpt),
                Json = parseResult.GetValue(jsonOpt),
                Recursive = parseResult.GetValue(recursiveOpt),
                SizeFormat = parseResult.GetValue(siOpt) ? SizeFormat.SI : SizeFormat.IEC,
            };
            return Execute(options, cancellationToken);
        });

        return command;
    }

    public static int Execute(ListOptions options, CancellationToken cancellationToken = default)
    {
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

            List<(RpfFile rpf, RpfFileEntry entry)> entries = RpfHelper.CollectFiles(
                rpf,
                options.Filters,
                options.Recursive
            );

            Json.ListResult result = CollectList(entries, rpf, scanErrors, options, cancellationToken);

            if (options.Json)
                PrintJsonList(result);
            else
                PrintList(result, options, cancellationToken);

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

    internal static Json.ListResult ErrorResult(string[] errorMessages, ListOptions options) =>
        new()
        {
            Success = false,
            RpfFile = options.RpfPath,
            TotalFiles = 0,
            TotalSize = 0,
            TotalSizeFormatted = "0 B",
            NestedRpfCount = 0,
            Files = [],
            ErrorMessages = errorMessages,
        };

    internal static Json.ListResult CollectList(
        List<(RpfFile rpf, RpfFileEntry entry)> entries,
        RpfFile rpf,
        List<string> scanErrors,
        ListOptions options,
        CancellationToken cancellationToken = default)
    {
        long totalSize = 0;
        List<Json.FileEntry> files = [];

        foreach ((RpfFile _, RpfFileEntry fileEntry) in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            long size = fileEntry.GetFileSize();
            totalSize += size;
            string ext = Path.GetExtension(fileEntry.Name).ToLowerInvariant();

            files.Add(
                new Json.FileEntry
                {
                    Path = fileEntry.Path,
                    Name = fileEntry.Name,
                    Size = size,
                    SizeFormatted = options.SizeFormat.ToFormattedString(size),
                    Type = RpfHelper.GetFileType(fileEntry),
                    Extension = ext,
                }
            );
        }

        return new Json.ListResult
        {
            Success = scanErrors.Count == 0,
            RpfFile = options.RpfPath,
            TotalFiles = entries.Count,
            TotalSize = totalSize,
            TotalSizeFormatted = options.SizeFormat.ToFormattedString(totalSize),
            NestedRpfCount = rpf.GrandTotalRpfCount,
            Files = [.. files],
            ErrorMessages = [.. scanErrors],
        };
    }

    internal static void PrintJsonList(Json.ListResult result) =>
        Console.WriteLine(JsonSerializer.Serialize(result, Output.JsonSerializerOptions));

    internal static void PrintList(
        Json.ListResult result,
        ListOptions options,
        CancellationToken cancellationToken = default)
    {
        foreach (Json.FileEntry file in result.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (options.Verbose)
            {
                string sizeStr = file.SizeFormatted.PadLeft(12);
                Console.WriteLine($"{sizeStr}  {file.Path}");
            }
            else
            {
                Console.WriteLine(file.Path);
            }
        }

        Console.Error.WriteLine();
        Console.Error.WriteLine(
            $"Total: {result.TotalFiles} files, {result.TotalSizeFormatted}"
        );
    }
}
