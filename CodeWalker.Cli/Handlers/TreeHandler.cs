using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

using CodeWalker.Cli.Helpers;
using CodeWalker.GameFiles;

namespace CodeWalker.Cli.Handlers;

[ExcludeFromCodeCoverage]
internal sealed record TreeOptions
{
    public required string RpfPath { get; init; }
    public required string ExePath { get; init; }
    public required bool Gen9 { get; init; }
    public required string[] Filters { get; init; }
    public required bool Verbose { get; init; }
    public required bool Json { get; init; }
    public required bool Recursive { get; init; }
    public required SizeFormat SizeFormat { get; init; }
    public required int Depth { get; init; }
}

internal readonly record struct ChildItem(string Name, bool IsDir, RpfEntry Entry, RpfFile? ChildRpf, RpfFileEntry? ArchiveEntry = null);

internal static class TreeHandler
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
        Option<int> depthOption = new("--depth", "-d")
        {
            Description = "Maximum depth to display (default: unlimited)",
            DefaultValueFactory = _ => -1
        };

        depthOption.Validators.Add(result =>
        {
            if (result.GetValue(depthOption) < -1)
                result.AddError("--depth must be -1 (unlimited) or a non-negative integer.");
        });

        Command command = new("tree", "Display a visual tree of the RPF directory structure")
        {
            rpfOpt,
            exeOpt,
            gen9Opt,
            filterOpt,
            recursiveOpt,
            verboseOpt,
            jsonOpt,
            siOpt,
            depthOption,
        };
        command.Aliases.Add("t");

        command.SetAction(parseResult =>
        {
            TreeOptions options = new()
            {
                RpfPath = parseResult.GetValue(rpfOpt)?.FullName ?? "",
                ExePath = parseResult.GetRequiredValue(exeOpt).FullName,
                Gen9 = parseResult.GetValue(gen9Opt),
                Filters = Filter.Normalize(parseResult.GetValue(filterOpt)),
                Verbose = parseResult.GetValue(verboseOpt),
                Json = parseResult.GetValue(jsonOpt),
                Recursive = parseResult.GetValue(recursiveOpt),
                SizeFormat = parseResult.GetValue(siOpt) ? SizeFormat.SI : SizeFormat.IEC,
                Depth = parseResult.GetValue(depthOption),
            };
            return Execute(options, cancellationToken);
        });

        return command;
    }

    public static int Execute(TreeOptions options, CancellationToken cancellationToken = default)
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

            int totalFiles = 0;
            int totalDirs = 0;

            Json.TreeNode rootNode = BuildTreeNode(
                rpf.Root,
                rpf,
                options,
                0,
                ref totalFiles,
                ref totalDirs,
                cancellationToken
            ) with
            { Name = Path.GetFileName(options.RpfPath) + "/" };

            if (options.Json)
                PrintJsonTree(rootNode, totalFiles, totalDirs, scanErrors, options);
            else
                PrintTree(rootNode, totalFiles, totalDirs, options, cancellationToken);

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

    internal static Json.TreeResult ErrorResult(string[] errorMessages, TreeOptions options) =>
        new()
        {
            Success = false,
            RpfFile = options.RpfPath,
            TotalFiles = 0,
            TotalDirs = 0,
            Root = null,
            ErrorMessages = errorMessages
        };

    internal static List<ChildItem> CollectChildren(RpfDirectoryEntry dir, RpfFile rpf, TreeOptions options)
    {
        List<ChildItem> items = [];
        HashSet<string> expandedRpfs = new(StringComparer.Ordinal);

        if (dir.Directories != null)
        {
            foreach (RpfDirectoryEntry subDir in dir.Directories)
                items.Add(new ChildItem(subDir.Name, true, subDir, null));
        }

        // Add nested RPFs as expandable directories if recursive
        if (options.Recursive && dir.Files != null && rpf.Children != null)
        {
            foreach (RpfFileEntry fileEntry in dir.Files)
            {
                if (!fileEntry.NameLower.EndsWith(".rpf", StringComparison.Ordinal))
                    continue;

                RpfFile? child = rpf.Children
                    .FirstOrDefault(c => c.Name == fileEntry.Name && c.Root != null);

                if (child != null)
                {
                    items.Add(new ChildItem(fileEntry.Name, true, child.Root, child, fileEntry));
                    _ = expandedRpfs.Add(fileEntry.Name);
                }
            }
        }

        if (dir.Files == null)
            return items;

        // Add files (matching filters, skip RPFs already expanded as directories)
        items.AddRange(dir.Files
            .Where(fe =>
                !expandedRpfs.Contains(fe.Name)
                && Filter.Matches(fe.Path, options.Filters)
            )
            .Select(fe => new ChildItem(fe.Name, false, fe, null)));

        return items;
    }

    internal static Json.TreeNode BuildTreeNode(
        RpfDirectoryEntry dir,
        RpfFile rpf,
        TreeOptions options,
        int depth,
        ref int totalFiles,
        ref int totalDirs,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        List<Json.TreeNode> children = [];

        if (options.Depth < 0 || depth < options.Depth)
        {
            foreach (ChildItem item in CollectChildren(dir, rpf, options))
            {
                if (item.IsDir)
                {
                    if (item.Entry is not RpfDirectoryEntry subDir)
                        continue;

                    Json.TreeNode dirNode = BuildTreeNode(
                        subDir,
                        item.ChildRpf ?? rpf,
                        options,
                        depth + 1,
                        ref totalFiles,
                        ref totalDirs,
                        cancellationToken
                    );

                    // Prune empty directories when filters are active
                    if (options.Filters.Length > 0
                        && (dirNode.Children == null || dirNode.Children.Count == 0))
                    {
                        continue;
                    }

                    totalDirs++;
                    if (item.ArchiveEntry != null)
                    {
                        long archiveSize = item.ArchiveEntry.GetFileSize();
                        children.Add(dirNode with
                        {
                            Name = item.Name,
                            Size = archiveSize,
                            SizeFormatted = options.SizeFormat.ToFormattedString(archiveSize),
                            FileType = RpfHelper.GetFileType(item.ArchiveEntry)
                        });
                    }
                    else
                    {
                        children.Add(dirNode);
                    }
                }
                else
                {
                    totalFiles++;
                    long? size = null;
                    string? sizeFormatted = null;
                    string? fileType = null;
                    int? version = null;

                    if (item.Entry is RpfFileEntry fileEntry)
                    {
                        size = fileEntry.GetFileSize();
                        sizeFormatted = options.SizeFormat.ToFormattedString(size.Value);
                        fileType = RpfHelper.GetFileType(fileEntry);
                        if (fileEntry is RpfResourceFileEntry rfe)
                            version = rfe.Version;
                    }

                    children.Add(
                        new Json.TreeNode
                        {
                            Name = item.Name,
                            Path = item.Entry.Path,
                            Type = "file",
                            Size = size,
                            SizeFormatted = sizeFormatted,
                            FileType = fileType,
                            Version = version
                        }
                    );
                }
            }
        }

        return new Json.TreeNode
        {
            Name = dir.Name ?? Path.GetFileName(rpf.FilePath),
            Path = dir.Path ?? rpf.Path,
            Type = "dir",
            Children = children
        };
    }

    internal static void PrintTree(
        Json.TreeNode root,
        int totalFiles,
        int totalDirs,
        TreeOptions options,
        CancellationToken cancellationToken)
    {
        Console.WriteLine(root.Name);
        PrintTreeChildren(root, "", options, cancellationToken);
        Console.Error.WriteLine();
        Console.Error.WriteLine($"{totalDirs} directories, {totalFiles} files");
    }

    internal static void PrintTreeChildren(
        Json.TreeNode node,
        string prefix,
        TreeOptions options,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (node.Children == null)
            return;

        for (int i = 0; i < node.Children.Count; i++)
        {
            bool isLast = i == node.Children.Count - 1;
            string connector = isLast ? "\u2514\u2500\u2500 " : "\u251c\u2500\u2500 ";
            string childPrefix = prefix + (isLast ? "    " : "\u2502   ");

            Json.TreeNode child = node.Children[i];

            if (child.Type == "dir")
            {
                if (options.Verbose && child.SizeFormatted != null)
                    Console.WriteLine($"{prefix}{connector}{child.Name}/  <{child.SizeFormatted}, {child.FileType}>");
                else
                    Console.WriteLine($"{prefix}{connector}{child.Name}/");
                PrintTreeChildren(child, childPrefix, options, cancellationToken);
            }
            else if (options.Verbose && child.SizeFormatted != null)
            {
                string versionStr = child.Version != null ? $" v{child.Version}" : "";
                Console.WriteLine(
                    $"{prefix}{connector}{child.Name}  ({child.SizeFormatted}, {child.FileType}{versionStr})"
                );
            }
            else
            {
                Console.WriteLine($"{prefix}{connector}{child.Name}");
            }
        }
    }

    internal static void PrintJsonTree(
        Json.TreeNode root,
        int totalFiles,
        int totalDirs,
        List<string> scanErrors,
        TreeOptions options)
    {
        Json.TreeResult result = new()
        {
            Success = scanErrors.Count == 0,
            RpfFile = options.RpfPath,
            TotalFiles = totalFiles,
            TotalDirs = totalDirs,
            Root = root,
            ErrorMessages = [.. scanErrors]
        };

        Console.WriteLine(
            JsonSerializer.Serialize(result, Output.JsonSerializerOptions)
        );
    }
}
