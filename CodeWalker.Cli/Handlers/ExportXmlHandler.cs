using System.CommandLine;
using System.IO;
using System.Text;
using System.Threading;

using CodeWalker.Cli.Helpers;
using CodeWalker.GameFiles;

namespace CodeWalker.Cli.Handlers;

internal static class ExportXmlHandler
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
        Option<DirectoryInfo> outputOpt = CliOptions.OutputDir();
        Option<bool> dryRunOpt = CliOptions.DryRun();
        Option<bool> noOverwriteOpt = CliOptions.NoOverwrite();
        Option<bool> progressOpt = CliOptions.Progress();

        Command command = new("xml", "Export binary game files to XML")
        {
            rpfOpt, exeOpt, gen9Opt, filterOpt, recursiveOpt,
            verboseOpt, jsonOpt, siOpt, threadsOpt,
            outputOpt, dryRunOpt, noOverwriteOpt, progressOpt,
        };
        command.Aliases.Add("x");

        command.SetAction(parseResult =>
        {
            ExportOptions options = new()
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
                OutputPath = parseResult.GetValue(outputOpt)?.FullName ?? Directory.GetCurrentDirectory(),
                DryRun = parseResult.GetValue(dryRunOpt),
                NoOverwrite = parseResult.GetValue(noOverwriteOpt),
                Progress = parseResult.GetValue(progressOpt),
            };
            return ExportPipeline.Execute(options, "xml", "XML", ProcessFile, cancellationToken);
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
        string xml = MetaXml.GetXml(fileEntry, data, out string filename, fileOutputDir);

        if (string.IsNullOrEmpty(xml))
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

        if (!string.IsNullOrEmpty(fileOutputDir) && !Directory.Exists(fileOutputDir))
        {
            _ = Directory.CreateDirectory(fileOutputDir);
        }

        string outputPath = Path.Combine(fileOutputDir, filename);

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

        File.WriteAllText(outputPath, xml, Encoding.UTF8);

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
