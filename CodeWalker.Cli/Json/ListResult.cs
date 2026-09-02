using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using CodeWalker.Cli.Helpers;

namespace CodeWalker.Cli.Json;

[ExcludeFromCodeCoverage]
internal sealed record ListResult : BaseResult
{
    [JsonPropertyName("rpfFile")]
    public required string RpfFile { get; init; }

    [JsonPropertyName("totalFiles")]
    public required int TotalFiles { get; init; }

    [JsonPropertyName("totalSize")]
    public required long TotalSize { get; init; }

    [JsonPropertyName("totalSizeFormatted")]
    public required string TotalSizeFormatted { get; init; }

    [JsonPropertyName("nestedRpfCount")]
    public required long NestedRpfCount { get; init; }

    [JsonPropertyName("files")]
    public required IReadOnlyList<FileEntry> Files { get; init; }
}
