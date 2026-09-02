using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using CodeWalker.Cli.Helpers;

namespace CodeWalker.Cli.Json;

[ExcludeFromCodeCoverage]
internal sealed record ExportFileEntry
{
    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("outputPath")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OutputPath { get; init; }

    [JsonPropertyName("outputFiles")]
    public required int OutputFiles { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record ExportResult : BaseResult
{
    [JsonPropertyName("rpfFile")]
    public required string RpfFile { get; init; }

    [JsonPropertyName("outputDir")]
    public required string OutputDir { get; init; }

    [JsonPropertyName("format")]
    public required string Format { get; init; }

    [JsonPropertyName("totalFiles")]
    public required int TotalFiles { get; init; }

    [JsonPropertyName("exported")]
    public required int Exported { get; init; }

    [JsonPropertyName("skipped")]
    public required int Skipped { get; init; }

    [JsonPropertyName("errors")]
    public required int Errors { get; init; }

    [JsonPropertyName("dryRun")]
    public required bool DryRun { get; init; }

    [JsonPropertyName("files")]
    public required IReadOnlyList<ExportFileEntry> Files { get; init; }
}
