using System.CommandLine;
using System.Threading;

namespace CodeWalker.Cli.Handlers;

internal static class ExportHandler
{
    public static Command CreateCommand(CancellationToken cancellationToken = default)
    {
        Command command = new(
            "export",
            "Export game files to external formats (XML, DDS, WAV, text)"
        )
        {
            ExportXmlHandler.CreateCommand(cancellationToken),
            ExportTexturesHandler.CreateCommand(cancellationToken),
            ExportAudioHandler.CreateCommand(cancellationToken),
            ExportTextHandler.CreateCommand(cancellationToken),
        };
        command.Aliases.Add("e");

        return command;
    }
}
