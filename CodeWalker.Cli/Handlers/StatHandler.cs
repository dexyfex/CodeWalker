using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

using CodeWalker.Cli.Helpers;
using CodeWalker.GameFiles;

namespace CodeWalker.Cli.Handlers;

internal sealed record StatOptions
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

internal static class StatHandler
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

        Command command = new("stat", "Show aggregate statistics for RPF archive contents")
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
        command.Aliases.Add("S");

        command.SetAction(parseResult =>
        {
            StatOptions options = new()
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

    /// <summary>
    /// Validates the archive, collects statistics for the matching entries and prints them.
    /// </summary>
    public static int Execute(StatOptions options, CancellationToken cancellationToken = default)
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

            Json.StatResult result = CollectStats(entries, scanErrors, options, cancellationToken);

            if (options.Json)
                PrintJsonStats(result);
            else
                PrintStats(result, options, cancellationToken);

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

    /// <summary>
    /// A failed result carrying the given messages, with every statistic zeroed.
    /// </summary>
    internal static Json.StatResult ErrorResult(string[] errorMessages, StatOptions options) =>
        new()
        {
            Success = false,
            RpfFile = options.RpfPath,
            TotalFiles = 0,
            TotalSize = 0,
            TotalSizeFormatted = "0 B",
            ResourceCount = 0,
            BinaryCount = 0,
            CompressedSize = 0,
            CompressedSizeFormatted = "0 B",
            UncompressedSize = 0,
            UncompressedSizeFormatted = "0 B",
            CompressionRatio = 0,
            Extensions = [],
            ErrorMessages = errorMessages
        };

    /// <summary>
    /// Totals, compression figures and per-extension breakdown for the given entries.
    /// </summary>
    internal static Json.StatResult CollectStats(
        List<(RpfFile rpf, RpfFileEntry entry)> entries,
        List<string> scanErrors,
        StatOptions options,
        CancellationToken cancellationToken = default)
    {
        int resourceCount = 0;
        int binaryCount = 0;
        long totalSize = 0;
        long compressedSize = 0;
        long uncompressedSize = 0;

        Dictionary<string, (int count, long total, long min, long max)> extStats = [];

        foreach ((RpfFile _, RpfFileEntry fileEntry) in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            long size = fileEntry.GetFileSize();
            totalSize += size;

            string ext = Path.GetExtension(fileEntry.Name).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext))
                ext = "(none)";

            if (extStats.TryGetValue(ext, out (int count, long total, long min, long max) stat))
            {
                extStats[ext] = (
                    stat.count + 1,
                    stat.total + size,
                    Math.Min(stat.min, size),
                    Math.Max(stat.max, size)
                );
            }
            else
            {
                extStats[ext] = (
                    1,
                    size,
                    size,
                    size
                );
            }

            switch (fileEntry)
            {
                case RpfResourceFileEntry rfe:
                    resourceCount++;
                    compressedSize += size;
                    uncompressedSize += rfe.SystemSize + rfe.GraphicsSize;
                    break;
                case RpfBinaryFileEntry bfe:
                    binaryCount++;
                    compressedSize += size;
                    uncompressedSize += bfe.FileUncompressedSize;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown file entry type: {fileEntry.GetType().FullName}");
            }
        }

        double compressionRatio =
            uncompressedSize > 0 ? (double)compressedSize / uncompressedSize : 0;

        List<Json.ExtensionStat> extensionStats = [
            .. extStats
                .OrderByDescending(kv => kv.Value.total)
                .Select(kv => new Json.ExtensionStat
                {
                    Extension = kv.Key,
                    Count = kv.Value.count,
                    TotalSize = kv.Value.total,
                    TotalSizeFormatted = options.SizeFormat.ToFormattedString(kv.Value.total),
                    AvgSize = kv.Value.count > 0 ? kv.Value.total / kv.Value.count : 0,
                    AvgSizeFormatted =
                        options.SizeFormat.ToFormattedString(kv.Value.count > 0
                            ? kv.Value.total / kv.Value.count
                            : 0),
                    MinSize = kv.Value.min,
                    MinSizeFormatted = options.SizeFormat.ToFormattedString(kv.Value.min),
                    MaxSize = kv.Value.max,
                    MaxSizeFormatted = options.SizeFormat.ToFormattedString(kv.Value.max)
                })
        ];

        return new Json.StatResult
        {
            Success = scanErrors.Count == 0,
            RpfFile = options.RpfPath,
            TotalFiles = entries.Count,
            TotalSize = totalSize,
            TotalSizeFormatted = options.SizeFormat.ToFormattedString(totalSize),
            ResourceCount = resourceCount,
            BinaryCount = binaryCount,
            CompressedSize = compressedSize,
            CompressedSizeFormatted = options.SizeFormat.ToFormattedString(compressedSize),
            UncompressedSize = uncompressedSize,
            UncompressedSizeFormatted = options.SizeFormat.ToFormattedString(uncompressedSize),
            CompressionRatio = Math.Round(compressionRatio, 4),
            Extensions = extensionStats,
            ErrorMessages = [.. scanErrors]
        };
    }

    /// <summary>
    /// Prints the statistics as JSON.
    /// </summary>
    internal static void PrintJsonStats(Json.StatResult result) =>
        Console.WriteLine(JsonSerializer.Serialize(result, Output.JsonSerializerOptions));

    /// <summary>
    /// Prints the statistics as an aligned table.
    /// </summary>
    internal static void PrintStats(Json.StatResult result, StatOptions options, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Collect all rows for dynamic column sizing
        string[] headers = ["Extension", "Count", "Total", "Avg", "Min", "Max"];
        string[][] rows = [
            .. result.Extensions
                .Select(ext =>
                    (string[])[
                        ext.Extension,
                        ext.Count.ToString(CultureInfo.InvariantCulture),
                        options.SizeFormat.ToFormattedString(ext.TotalSize),
                        options.SizeFormat.ToFormattedString(ext.AvgSize),
                        options.SizeFormat.ToFormattedString(ext.MinSize),
                        options.SizeFormat.ToFormattedString(ext.MaxSize)
                    ]
                )
        ];

        int[] widths = new int[headers.Length];
        for (int i = 0; i < headers.Length; i++)
            widths[i] = headers[i].Length;

        foreach (string[] row in rows)
            for (int i = 0; i < row.Length; i++)
                widths[i] = Math.Max(widths[i], row[i].Length);

        Console.Write($"+{new string('-', widths[0] + 2)}");
        for (int i = 1; i < widths.Length; i++)
            Console.Write($"+{new string('-', widths[i] + 2)}");
        Console.WriteLine("+");

        // First column left-aligned, the rest right-aligned
        Console.Write($"| {headers[0].PadRight(widths[0])} ");
        for (int i = 1; i < headers.Length; i++)
            Console.Write($"| {headers[i].PadLeft(widths[i])} ");
        Console.WriteLine("|");

        Console.Write($"+{new string('-', widths[0] + 2)}");
        for (int i = 1; i < widths.Length; i++)
            Console.Write($"+{new string('-', widths[i] + 2)}");
        Console.WriteLine("+");

        foreach (string[] row in rows)
        {
            Console.Write($"| {row[0].PadRight(widths[0])} ");
            for (int i = 1; i < row.Length; i++)
                Console.Write($"| {row[i].PadLeft(widths[i])} ");
            Console.WriteLine("|");
        }

        Console.Write($"+{new string('-', widths[0] + 2)}");
        for (int i = 1; i < widths.Length; i++)
            Console.Write($"+{new string('-', widths[i] + 2)}");
        Console.WriteLine("+");

        Console.Error.WriteLine();
        Console.Error.WriteLine(
            $"Total: {result.TotalFiles} files, {options.SizeFormat.ToFormattedString(result.TotalSize)}"
        );
        Console.Error.WriteLine($"Types: {result.ResourceCount} resource, {result.BinaryCount} binary");

        if (result.UncompressedSize > 0)
        {
            string compressedStr = options.SizeFormat.ToFormattedString(result.CompressedSize);
            string uncompressedStr = options.SizeFormat.ToFormattedString(result.UncompressedSize);
            Console.Error.WriteLine(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Compression: {0} / {1} ({2:P1} of original)",
                    compressedStr,
                    uncompressedStr,
                    result.CompressionRatio
                )
            );
        }
    }
}
