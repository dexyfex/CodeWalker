using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using CodeWalker.Cli.Helpers;
using CodeWalker.GameFiles;

namespace CodeWalker.Cli.Handlers;

internal sealed record DiffOptions
{
    public required string LeftPath { get; init; }
    public required string RightPath { get; init; }
    public required string LeftExePath { get; init; }
    public required string RightExePath { get; init; }
    public required bool LeftGen9 { get; init; }
    public required bool RightGen9 { get; init; }
    public required bool Recursive { get; init; }
    public required bool Progress { get; init; }
    public required bool Verbose { get; init; }
    public required bool Json { get; init; }
    public required SizeFormat SizeFormat { get; init; }
    public required int Threads { get; init; }
}

internal static class DiffHandler
{
    public static Command CreateCommand(CancellationToken cancellationToken = default)
    {
        Option<bool> verboseOpt = CliOptions.Verbose();
        Option<bool> jsonOpt = CliOptions.Json();
        Option<bool> siOpt = CliOptions.Si();
        Option<int> threadsOpt = CliOptions.Threads();

        Option<FileInfo> leftOption = new("--left", "-l")
        {
            Description = "First RPF archive to compare",
            Required = true,
        };

        Option<FileInfo> rightOption = new("--right", "-r")
        {
            Description = "Second RPF archive to compare",
            Required = true,
        };

        Option<DirectoryInfo> leftExeOption = new("--left-exe", "-le")
        {
            Description = "Path to the GTA V installation for the left archive",
            Required = true,
        };

        Option<DirectoryInfo> rightExeOption = new("--right-exe", "-re")
        {
            Description = "Path to the GTA V installation for the right archive",
            Required = true,
        };

        Option<bool> leftGen9Option = new("--left-gen9", "-lg")
        {
            Description = "Use GTA V Enhanced (Gen9) mode for the left archive",
        };

        Option<bool> rightGen9Option = new("--right-gen9", "-rg")
        {
            Description = "Use GTA V Enhanced (Gen9) mode for the right archive",
        };

        Option<bool> recursiveOption = new("--recursive", "-R")
        {
            Description = "Include nested RPFs in comparison",
        };

        Option<bool> progressOption = CliOptions.Progress();

        Command command = new("diff", "Compare two RPF archives")
        {
            leftOption,
            rightOption,
            leftExeOption,
            rightExeOption,
            leftGen9Option,
            rightGen9Option,
            recursiveOption,

            progressOption,
            verboseOpt,
            jsonOpt,
            siOpt,
            threadsOpt
        };

        command.Aliases.Add("d");

        command.SetAction(parseResult =>
        {
            DiffOptions options = new()
            {
                LeftPath = parseResult.GetRequiredValue(leftOption).FullName,
                RightPath = parseResult.GetRequiredValue(rightOption).FullName,
                LeftExePath = parseResult.GetRequiredValue(leftExeOption).FullName,
                RightExePath = parseResult.GetRequiredValue(rightExeOption).FullName,
                LeftGen9 = parseResult.GetValue(leftGen9Option),
                RightGen9 = parseResult.GetValue(rightGen9Option),
                Recursive = parseResult.GetValue(recursiveOption),
                Progress = parseResult.GetValue(progressOption),
                Verbose = parseResult.GetValue(verboseOpt),
                Json = parseResult.GetValue(jsonOpt),
                SizeFormat = parseResult.GetValue(siOpt) ? SizeFormat.SI : SizeFormat.IEC,
                Threads = parseResult.GetValue(threadsOpt),
            };
            return Execute(options, cancellationToken);
        });

        return command;
    }

    public static int Execute(DiffOptions options, CancellationToken cancellationToken = default)
    {
        string? leftError = RpfHelper.ValidateInputs(
            options.LeftPath,
            options.LeftExePath,
            options.LeftGen9
        );
        if (leftError != null)
        {
            return Output.ReportError(
                leftError,
                options.Json,
                ErrorResult([], options)
            );
        }

        string? rightError = RpfHelper.ValidateInputs(
            options.RightPath,
            options.RightExePath,
            options.RightGen9
        );
        if (rightError != null)
        {
            return Output.ReportError(
                rightError,
                options.Json,
                ErrorResult([], options)
            );
        }

        List<string> errorMessages = [];
        try
        {
            // Encryption keys are process-wide, so each archive is opened and read while its
            // own installation's keys are loaded. Metadata comes first for both sides; only the
            // entries that could still turn out identical are extracted and hashed.
            if (!options.Json)
                Console.Error.WriteLine("Loading left encryption keys...");
            RpfHelper.LoadKeys(options.LeftExePath, options.LeftGen9);
            RpfFile leftRpf = RpfHelper.OpenRpf(
                options.LeftPath,
                options.Verbose,
                options.Json,
                errorMessages
            );
            List<(RpfFile rpf, RpfFileEntry entry)> leftFiles =
                RpfHelper.CollectFiles(leftRpf, null, options.Recursive);
            Dictionary<string, SideEntry> left = BuildMetadata(leftFiles, leftRpf.Root.Path);

            if (!options.Json)
                Console.Error.WriteLine("Loading right encryption keys...");
            RpfHelper.LoadKeys(options.RightExePath, options.RightGen9);
            RpfFile rightRpf = RpfHelper.OpenRpf(
                options.RightPath,
                options.Verbose,
                options.Json,
                errorMessages
            );
            List<(RpfFile rpf, RpfFileEntry entry)> rightFiles =
                RpfHelper.CollectFiles(rightRpf, null, options.Recursive);
            Dictionary<string, SideEntry> right = BuildMetadata(rightFiles, rightRpf.Root.Path);

            HashSet<string> candidates = FindHashCandidates(left, right);

            if (candidates.Count > 0)
            {
                HashEntries(rightFiles, right, candidates, rightRpf.Root.Path, "right", options, errorMessages, cancellationToken);

                RpfHelper.LoadKeys(options.LeftExePath, options.LeftGen9);
                HashEntries(leftFiles, left, candidates, leftRpf.Root.Path, "left", options, errorMessages, cancellationToken);
            }

            Json.DiffResult result = CompareSides(left, right, errorMessages, options);

            if (options.Json)
                PrintJsonDiff(result);
            else
                PrintDiff(result, options);

            return errorMessages.Count > 0 ? 1 : 0;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return Output.ReportError(
                ex.Message,
                options.Json,
                ErrorResult([.. errorMessages], options),
                options.Verbose ? ex.StackTrace : null
            );
        }
    }

    internal static Json.DiffResult ErrorResult(string[] errorMessages, DiffOptions options) =>
        new()
        {
            Success = false,
            LeftRpf = options.LeftPath,
            RightRpf = options.RightPath,
            Added = [],
            Removed = [],
            Modified = [],
            Unchanged = [],
            Summary = new Json.DiffSummary
            {
                AddedCount = 0,
                RemovedCount = 0,
                ModifiedCount = 0,
                UnchangedCount = 0,
            },
            ErrorMessages = errorMessages,
        };

    /// <summary>
    /// One archive entry as seen from a single side, with its content hash filled in
    /// only for entries that need a byte-level comparison.
    /// </summary>
    internal sealed record SideEntry
    {
        public required string Name { get; init; }
        public required long Size { get; init; }
        public required string Type { get; init; }
        public string? Hash { get; init; }
    }

    /// <summary>
    /// Strips the containing archive's own name from an entry path, so two archives compare
    /// by their contents rather than by what the files on disk happen to be called.
    /// </summary>
    internal static string RelativeKey(string entryPath, string rootPath)
    {
        if (rootPath.Length == 0 || !entryPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
            return entryPath;

        string rest = entryPath[rootPath.Length..];
        return rest.StartsWith('\\') ? rest[1..] : rest;
    }

    internal static Dictionary<string, SideEntry> BuildMetadata(
        List<(RpfFile rpf, RpfFileEntry entry)> files,
        string rootPath
    ) =>
        files.ToDictionary(
            f => RelativeKey(f.entry.Path, rootPath),
            f => new SideEntry
            {
                Name = f.entry.Name,
                Size = f.entry.GetFileSize(),
                Type = RpfHelper.GetFileType(f.entry),
            }
        );

    /// <summary>
    /// Paths present on both sides with matching size and type. Anything else is already
    /// decided by its metadata, so its content never has to be read.
    /// </summary>
    internal static HashSet<string> FindHashCandidates(
        IReadOnlyDictionary<string, SideEntry> left,
        IReadOnlyDictionary<string, SideEntry> right
    )
    {
        HashSet<string> candidates = [];
        foreach (KeyValuePair<string, SideEntry> kvp in left)
        {
            if (right.TryGetValue(kvp.Key, out SideEntry? other)
                && other.Size == kvp.Value.Size
                && other.Type == kvp.Value.Type)
            {
                _ = candidates.Add(kvp.Key);
            }
        }
        return candidates;
    }

    private static void HashEntries(
        List<(RpfFile rpf, RpfFileEntry entry)> files,
        Dictionary<string, SideEntry> side,
        HashSet<string> candidates,
        string rootPath,
        string label,
        DiffOptions options,
        List<string> errorMessages,
        CancellationToken cancellationToken
    )
    {
        (string key, RpfFile rpf, RpfFileEntry entry)[] targets =
        [
            .. files
                .Select(f => (key: RelativeKey(f.entry.Path, rootPath), f.rpf, f.entry))
                .Where(f => candidates.Contains(f.key)),
        ];

        string?[] hashes = new string?[targets.Length];
        string?[] failures = new string?[targets.Length];

        using (ProgressBar progress = new(targets.Length, options.Progress && !options.Json))
        {
            _ = Parallel.For(
                0,
                targets.Length,
                new ParallelOptions { MaxDegreeOfParallelism = options.Threads, CancellationToken = cancellationToken },
                i =>
                {
                    (_, RpfFile rpf, RpfFileEntry entry) = targets[i];
                    byte[]? data = rpf.ExtractFile(entry);
                    if (data == null)
                        failures[i] = $"Failed to extract {label} entry: {entry.Path}";
                    else
                        hashes[i] = ComputeHash(data);

                    progress.Increment(entry.Path);
                }
            );
        }

        for (int i = 0; i < targets.Length; i++)
        {
            if (failures[i] != null)
            {
                errorMessages.Add(failures[i]!);
                continue;
            }
            string key = targets[i].key;
            side[key] = side[key] with { Hash = hashes[i] };
        }
    }

    private static string ComputeHash(byte[] data)
    {
#if NET5_0_OR_GREATER
        return Convert.ToHexString(SHA256.HashData(data));
#else
        using SHA256 sha = SHA256.Create();
        return BitConverter.ToString(sha.ComputeHash(data)).Replace("-", string.Empty);
#endif
    }

    internal static Json.DiffResult CompareSides(
        IReadOnlyDictionary<string, SideEntry> left,
        IReadOnlyDictionary<string, SideEntry> right,
        List<string> errorMessages,
        DiffOptions options
    )
    {
        SizeFormat sizeFormat = options.SizeFormat;

        Json.DiffEntry Single(string path, SideEntry entry) =>
            new()
            {
                Path = path,
                Name = entry.Name,
                Type = entry.Type,
                Size = entry.Size,
                SizeFormatted = sizeFormat.ToFormattedString(entry.Size),
            };

        List<Json.DiffEntry> added = [];
        List<Json.DiffEntry> removed = [];
        List<Json.DiffEntry> modified = [];
        List<Json.DiffEntry> unchanged = [];

        foreach (KeyValuePair<string, SideEntry> kvp in left)
        {
            if (!right.TryGetValue(kvp.Key, out SideEntry? other))
            {
                removed.Add(Single(kvp.Key, kvp.Value));
                continue;
            }

            // A missing hash means the entry was never a candidate, or extraction failed.
            // Either way it cannot be proven identical.
            bool same = kvp.Value.Hash != null
                && other.Hash != null
                && string.Equals(kvp.Value.Hash, other.Hash, StringComparison.Ordinal);

            if (same)
            {
                unchanged.Add(Single(kvp.Key, kvp.Value));
            }
            else
            {
                modified.Add(
                    new Json.DiffEntry
                    {
                        Path = kvp.Key,
                        Name = kvp.Value.Name,
                        Type = kvp.Value.Type,
                        LeftSize = kvp.Value.Size,
                        LeftSizeFormatted = sizeFormat.ToFormattedString(kvp.Value.Size),
                        RightSize = other.Size,
                        RightSizeFormatted = sizeFormat.ToFormattedString(other.Size),
                    }
                );
            }
        }

        added.AddRange(
            right.Where(kvp => !left.ContainsKey(kvp.Key)).Select(kvp => Single(kvp.Key, kvp.Value))
        );

        added.Sort((a, b) => string.CompareOrdinal(a.Path, b.Path));
        removed.Sort((a, b) => string.CompareOrdinal(a.Path, b.Path));
        modified.Sort((a, b) => string.CompareOrdinal(a.Path, b.Path));
        unchanged.Sort((a, b) => string.CompareOrdinal(a.Path, b.Path));

        return new Json.DiffResult
        {
            Success = errorMessages.Count == 0,
            LeftRpf = options.LeftPath,
            RightRpf = options.RightPath,
            Added = [.. added],
            Removed = [.. removed],
            Modified = [.. modified],
            Unchanged = [.. unchanged],
            Summary = new Json.DiffSummary
            {
                AddedCount = added.Count,
                RemovedCount = removed.Count,
                ModifiedCount = modified.Count,
                UnchangedCount = unchanged.Count,
            },
            ErrorMessages = [.. errorMessages],
        };
    }

    internal static void PrintJsonDiff(Json.DiffResult result) =>
        Console.WriteLine(JsonSerializer.Serialize(result, Output.JsonSerializerOptions));

    internal static void PrintDiff(Json.DiffResult result, DiffOptions options)
    {
        if (result.Added.Count > 0)
        {
            Console.WriteLine($"Added ({result.Added.Count}):");
            foreach (Json.DiffEntry entry in result.Added)
            {
                Console.WriteLine($"  + {entry.Path}");
            }
            Console.WriteLine();
        }

        if (result.Removed.Count > 0)
        {
            Console.WriteLine($"Removed ({result.Removed.Count}):");
            foreach (Json.DiffEntry entry in result.Removed)
            {
                Console.WriteLine($"  - {entry.Path}");
            }
            Console.WriteLine();
        }

        if (result.Modified.Count > 0)
        {
            Console.WriteLine($"Modified ({result.Modified.Count}):");
            foreach (Json.DiffEntry entry in result.Modified)
            {
                Console.WriteLine(
                    $"  ~ {entry.Path}  ({entry.LeftSizeFormatted} -> {entry.RightSizeFormatted})"
                );
            }
            Console.WriteLine();
        }

        if (options.Verbose && result.Unchanged.Count > 0)
        {
            Console.WriteLine($"Unchanged ({result.Unchanged.Count}):");
            foreach (Json.DiffEntry entry in result.Unchanged)
            {
                Console.WriteLine($"  = {entry.Path}");
            }
            Console.WriteLine();
        }

        Console.Error.WriteLine(
            $"Summary: {result.Summary.AddedCount} added, {result.Summary.RemovedCount} removed, {result.Summary.ModifiedCount} modified, {result.Summary.UnchangedCount} unchanged"
        );
    }
}
