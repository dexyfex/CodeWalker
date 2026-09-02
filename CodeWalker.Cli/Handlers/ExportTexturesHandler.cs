using System.CommandLine;
using System.IO;
using System.Threading;

using CodeWalker.Cli.Helpers;
using CodeWalker.GameFiles;
using CodeWalker.Utils;

namespace CodeWalker.Cli.Handlers;

internal static class ExportTexturesHandler
{
    private static readonly string[] DefaultFilters = ["*.ytd"];

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

        Command command = new("textures", "Export .ytd texture dictionaries to DDS files")
        {
            rpfOpt, exeOpt, gen9Opt, filterOpt, recursiveOpt,
            verboseOpt, jsonOpt, siOpt, threadsOpt,
            outputOpt, dryRunOpt, noOverwriteOpt, progressOpt,
        };
        command.Aliases.Add("t");
        command.Aliases.Add("ytd");

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
            return ExportPipeline.Execute(options, "dds", "Texture", ProcessFile, cancellationToken);
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
        YtdFile ytd = RpfFile.GetFile<YtdFile>(fileEntry, data);
        if (
            ytd?.TextureDict?.Textures?.data_items == null
            || ytd.TextureDict.Textures.data_items.Length == 0
        )
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

        bool dirCreated = false;
        int texCount = 0;
        foreach (Texture tex in ytd.TextureDict.Textures.data_items)
        {
            string texName = (tex.Name ?? "unknown") + ".dds";
            string outputPath = Path.Combine(fileOutputDir, texName);

            if (noOverwrite && File.Exists(outputPath))
                continue;

            if (!dirCreated)
            {
                _ = Directory.CreateDirectory(fileOutputDir);
                dirCreated = true;
            }

            byte[] dds = DDSIO.GetDDSFile(tex);
            File.WriteAllBytes(outputPath, dds);
            texCount++;
        }

        return (
            new Json.ExportFileEntry
            {
                Path = fileEntry.Path,
                Name = fileEntry.Name,
                OutputPath = texCount > 0 ? fileOutputDir : null,
                OutputFiles = texCount,
                Status = texCount > 0 ? "exported" : "skipped",
            },
            null
        );
    }
}
