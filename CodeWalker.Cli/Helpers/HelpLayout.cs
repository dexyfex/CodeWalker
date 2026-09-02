using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;

namespace CodeWalker.Cli.Helpers;

/// <summary>
/// Appends the parts of the command reference that generated option tables cannot carry:
/// what each output stream is for, the shape of <c>--json</c> output, and the exit codes.
/// </summary>
/// <remarks>
/// System.CommandLine keeps its help layout API internal, so the help option's action is
/// wrapped rather than reconfigured: the stock help is written first, then these sections.
/// </remarks>
internal sealed class HelpLayout : SynchronousCommandLineAction
{
    private const string ExportFields =
        "rpfFile, outputDir, format, totalFiles, exported, skipped, errors, dryRun, "
        + "files[{path, name, outputPath, outputFiles, status}]";

    /// <summary>
    /// Fields each command writes under <c>--json</c>, keyed by command name, on top of the
    /// <c>success</c> and <c>errorMessages</c> that every result carries.
    /// </summary>
    private static readonly Dictionary<string, string> JsonFields = new(StringComparer.Ordinal)
    {
        ["extract"] = "rpfFile, outputDir, totalFiles, extracted, skipped, errors, dryRun, "
            + "files[{path, name, size, sizeFormatted, type, extension, status}]",
        ["list"] = "rpfFile, totalFiles, totalSize, totalSizeFormatted, nestedRpfCount, "
            + "files[{path, name, size, sizeFormatted, type, extension}]",
        ["hash"] = "hashes[{input, hash, hashSigned, hashHex, encoding}]",
        ["tree"] = "rpfFile, totalFiles, totalDirs, "
            + "root{name, path, type, size, sizeFormatted, fileType, version, children[]}",
        ["gen9"] = "inputFolder, outputFolder, totalFiles, converted, skipped, copied, errors, "
            + "files[{path, status, message}]",
        ["pack"] = "inputDir, outputFile, totalFiles, totalDirs, totalSize, "
            + "totalSizeFormatted, errors",
        ["diff"] = "leftRpf, rightRpf, added[], removed[], modified[], unchanged[], "
            + "summary{addedCount, removedCount, modifiedCount, unchangedCount}",
        ["stat"] = "rpfFile, totalFiles, totalSize, totalSizeFormatted, resourceCount, "
            + "binaryCount, compressedSize, uncompressedSize, compressionRatio, "
            + "extensions[{extension, count, totalSize, avgSize, minSize, maxSize}]",
        ["search"] = "rpfFile, rpfFiles, pattern, matchCount, "
            + "matches[{archive, path, name, size, type, extension}]",
        ["validate"] = "rpfFile, totalFiles, valid, warnings, errors, skipped, "
            + "files[{path, name, status, message}]",
        ["inspect"] = "rpfFile, path, name, size, sizeFormatted, type, extension, nameHash, "
            + "shortNameHash, resourceVersion, systemSize, graphicsSize, uncompressedSize, "
            + "encryptionType, details (shape depends on the file type)",
        ["xml"] = ExportFields,
        ["textures"] = ExportFields,
        ["audio"] = ExportFields,
        ["text"] = ExportFields,
    };

    private readonly HelpAction inner = new();

    /// <summary>
    /// Replaces the help action on <paramref name="root"/>. The option is recursive, so every
    /// subcommand's help goes through it too.
    /// </summary>
    public static void Install(Command root)
    {
        foreach (HelpOption help in root.Options.OfType<HelpOption>())
            help.Action = new HelpLayout();
    }

    public override int Invoke(ParseResult parseResult)
    {
        int result = this.inner.Invoke(parseResult);

        TextWriter output = parseResult.InvocationConfiguration.Output;
        Command command = parseResult.CommandResult.Command;
        // MaxWidth is unbounded when stdout is not a terminal; keep prose readable anyway.
        int width = Math.Min(100, Math.Max(40, this.inner.MaxWidth));

        // Only worth stating once, on the root command.
        if (!command.Parents.Any())
            WriteStreams(output);

        WriteJsonFields(output, command.Name, width);
        WriteExitCodes(output);

        return result;
    }

    private static void WriteStreams(TextWriter output)
    {
        output.WriteLine("Output:");
        output.WriteLine("  stdout  Data: file listings, trees, hashes, JSON.");
        output.WriteLine("  stderr  Progress, status and error messages.");
        output.WriteLine();
        output.WriteLine("  Redirecting stdout captures the data alone, so a run stays pipeable");
        output.WriteLine("  while still reporting what it is doing.");
        output.WriteLine();
    }

    private static void WriteJsonFields(TextWriter output, string commandName, int width)
    {
        if (!JsonFields.TryGetValue(commandName, out string? fields))
            return;

        output.WriteLine("JSON output (--json):");
        output.WriteLine("  A single object on stdout. Always present:");
        output.WriteLine("    success        Whether the command completed without errors.");
        output.WriteLine("    errorMessages  Every error encountered, as an array.");
        output.WriteLine();
        output.WriteLine("  Alongside those:");
        foreach (string line in Wrap(fields, width - 4))
            output.WriteLine("    " + line);
        output.WriteLine();
    }

    private static void WriteExitCodes(TextWriter output)
    {
        output.WriteLine("Exit codes:");
        output.WriteLine("  0    Success.");
        output.WriteLine("  1    One or more errors, or invalid arguments.");
        output.WriteLine("  130  Cancelled with Ctrl+C.");
    }

    internal static List<string> Wrap(string text, int width)
    {
        List<string> lines = [];
        string current = "";
        foreach (string word in text.Split(' '))
        {
            if (current.Length == 0)
                current = word;
            else if (current.Length + 1 + word.Length <= width)
                current += " " + word;
            else
            {
                lines.Add(current);
                current = word;
            }
        }
        if (current.Length > 0)
            lines.Add(current);
        return lines;
    }
}
