using System.CommandLine;
using System.IO;
using System.Threading;

using CodeWalker.Cli.Helpers;
using CodeWalker.GameFiles;

namespace CodeWalker.Cli.Handlers;

internal static class ExportAudioHandler
{
    private static readonly string[] DefaultFilters = ["*.awc"];

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

        Command command = new("audio", "Export .awc audio containers to WAV/MIDI files")
        {
            rpfOpt, exeOpt, gen9Opt, filterOpt, recursiveOpt,
            verboseOpt, jsonOpt, siOpt, threadsOpt,
            outputOpt, dryRunOpt, noOverwriteOpt, progressOpt,
        };
        command.Aliases.Add("a");
        command.Aliases.Add("awc");

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
            return ExportPipeline.Execute(options, "wav", "Audio", ProcessFile, cancellationToken);
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
        AwcFile awc = RpfFile.GetFile<AwcFile>(fileEntry, data);
        if (awc?.Streams == null || awc.Streams.Length == 0)
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
        int streamCount = 0;
        foreach (AwcStream stream in awc.Streams)
        {
            // Hash 0 indicates a metadata-only stream with no playable audio data
            if (stream.Hash == 0)
                continue;

            string streamName = stream.Name;

            if (stream.MidiChunk?.Data != null)
            {
                string midiPath = Path.Combine(fileOutputDir, streamName + ".midi");
                if (noOverwrite && File.Exists(midiPath))
                    continue;
                if (!dirCreated)
                {
                    _ = Directory.CreateDirectory(fileOutputDir);
                    dirCreated = true;
                }
                File.WriteAllBytes(midiPath, stream.MidiChunk.Data);
                streamCount++;
            }
            else
            {
                byte[] wav = stream.GetWavFile();
                string wavPath = Path.Combine(fileOutputDir, streamName + ".wav");
                if (noOverwrite && File.Exists(wavPath))
                    continue;
                if (!dirCreated)
                {
                    _ = Directory.CreateDirectory(fileOutputDir);
                    dirCreated = true;
                }
                File.WriteAllBytes(wavPath, wav);
                streamCount++;
            }
        }

        return (
            new Json.ExportFileEntry
            {
                Path = fileEntry.Path,
                Name = fileEntry.Name,
                OutputPath = streamCount > 0 ? fileOutputDir : null,
                OutputFiles = streamCount,
                Status = streamCount > 0 ? "exported" : "skipped",
            },
            null
        );
    }
}
