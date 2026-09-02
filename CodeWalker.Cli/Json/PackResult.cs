using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using CodeWalker.Cli.Helpers;

namespace CodeWalker.Cli.Json;

[ExcludeFromCodeCoverage]
internal sealed record PackResult : BaseResult
{
    [JsonPropertyName("inputDir")]
    public required string InputDir { get; init; }

    [JsonPropertyName("outputFile")]
    public required string OutputFile { get; init; }

    [JsonPropertyName("totalFiles")]
    public required int TotalFiles { get; init; }

    [JsonPropertyName("totalDirs")]
    public required int TotalDirs { get; init; }

    [JsonPropertyName("totalSize")]
    public required long TotalSize { get; init; }

    [JsonPropertyName("totalSizeFormatted")]
    public required string TotalSizeFormatted { get; init; }

    [JsonPropertyName("errors")]
    public required int Errors { get; init; }
}
