using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using CodeWalker.Cli.Helpers;

namespace CodeWalker.Cli.Json;

[ExcludeFromCodeCoverage]
internal sealed record ExtractResult : BaseResult
{
    [JsonPropertyName("rpfFile")]
    public required string RpfFile { get; init; }

    [JsonPropertyName("outputDir")]
    public required string OutputDir { get; init; }

    [JsonPropertyName("totalFiles")]
    public required int TotalFiles { get; init; }

    [JsonPropertyName("extracted")]
    public required int Extracted { get; init; }

    [JsonPropertyName("skipped")]
    public required int Skipped { get; init; }

    [JsonPropertyName("errors")]
    public required int Errors { get; init; }

    [JsonPropertyName("dryRun")]
    public required bool DryRun { get; init; }

    [JsonPropertyName("files")]
    public required IReadOnlyList<FileEntry> Files { get; init; }
}
