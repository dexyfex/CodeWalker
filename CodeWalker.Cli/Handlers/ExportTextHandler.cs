using System.CommandLine;
using System.IO;
using System.Text;
using System.Threading;

using CodeWalker.Cli.Helpers;
using CodeWalker.GameFiles;

namespace CodeWalker.Cli.Handlers;

internal static class ExportTextHandler
{
    private static readonly string[] DefaultFilters = ["*.gxt2"];

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
        Option<DirectoryInfo> outputOpt = CliOptions.OutputDir();
        Option<bool> dryRunOpt = CliOptions.DryRun();
        Option<bool> noOverwriteOpt = CliOptions.NoOverwrite();
        Option<bool> progressOpt = CliOptions.Progress();

        Command command = new("text", "Export .gxt2 localization files to plain text")
        {
            rpfOpt, exeOpt, gen9Opt, filterOpt, recursiveOpt,
            verboseOpt, jsonOpt, siOpt, threadsOpt,
            outputOpt, dryRunOpt, noOverwriteOpt, progressOpt,
        };
        command.Aliases.Add("g");
        command.Aliases.Add("gxt2");

        command.SetAction(parseResult =>
        {
            string[] filters = Filter.Normalize(parseResult.GetValue(filterOpt));
            ExportOptions options = new()
            {
                RpfPath = parseResult.GetValue(rpfOpt)?.FullName ?? "",
                ExePath = parseResult.GetRequiredValue(exeOpt).FullName,
                Gen9 = parseResult.GetValue(gen9Opt),
                Filters = filters.Length == 0 ? DefaultFilters : filters,
                Verbose = parseResult.GetValue(verboseOpt),
                Json = parseResult.GetValue(jsonOpt),
                Recursive = parseResult.GetValue(recursiveOpt),
                Threads = parseResult.GetValue(threadsOpt),
                SizeFormat = parseResult.GetValue(siOpt) ? SizeFormat.SI : SizeFormat.IEC,
                OutputPath = parseResult.GetValue(outputOpt)?.FullName ?? Directory.GetCurrentDirectory(),
                DryRun = parseResult.GetValue(dryRunOpt),
                NoOverwrite = parseResult.GetValue(noOverwriteOpt),
                Progress = parseResult.GetValue(progressOpt),
            };
            return ExportPipeline.Execute(options, "txt", "Text", ProcessFile, cancellationToken);
        });

        return command;
    }

    private static (Json.ExportFileEntry entry, string? _) ProcessFile(
        RpfFileEntry fileEntry,
        byte[] data,
        string fileOutputDir,
        bool noOverwrite
    )
    {
        Gxt2File gxt = RpfFile.GetFile<Gxt2File>(fileEntry, data);
        if (gxt == null)
        {
            return (
                new Json.ExportFileEntry
                {
                    Path = fileEntry.Path,
                    Name = fileEntry.Name,
                    OutputFiles = 0,
                    Status = "unsupported",
                },
                null
            );
        }

        string text = gxt.ToText();

        if (string.IsNullOrEmpty(text))
        {
            return (
                new Json.ExportFileEntry
                {
                    Path = fileEntry.Path,
                    Name = fileEntry.Name,
                    OutputFiles = 0,
                    Status = "unsupported",
                },
                null
            );
        }

        string outputFileName = Path.GetFileNameWithoutExtension(fileEntry.Name) + ".txt";
        string outputPath = Path.Combine(fileOutputDir, outputFileName);

        if (noOverwrite && File.Exists(outputPath))
        {
            return (
                new Json.ExportFileEntry
                {
                    Path = fileEntry.Path,
                    Name = fileEntry.Name,
                    OutputFiles = 0,
                    Status = "skipped",
                },
                null
            );
        }

        if (!Directory.Exists(fileOutputDir))
        {
            _ = Directory.CreateDirectory(fileOutputDir);
        }

        File.WriteAllText(outputPath, text, Encoding.UTF8);

        return (
            new Json.ExportFileEntry
            {
                Path = fileEntry.Path,
                Name = fileEntry.Name,
                OutputPath = outputPath,
                OutputFiles = 1,
                Status = "exported",
            },
            null
        );
    }
}
