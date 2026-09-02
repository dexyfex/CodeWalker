using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using CodeWalker.Cli.Helpers;

namespace CodeWalker.Cli.Json;

[ExcludeFromCodeCoverage]
internal sealed record SearchMatch
{
    [JsonPropertyName("archive")]
    public required string Archive { get; init; }

    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("size")]
    public required long Size { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("extension")]
    public required string Extension { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record SearchResult : BaseResult
{
    [JsonPropertyName("rpfFile")]
    public required string RpfFile { get; init; }

    [JsonPropertyName("rpfFiles")]
    public required IReadOnlyList<string> RpfFiles { get; init; }

    [JsonPropertyName("pattern")]
    public required string Pattern { get; init; }

    [JsonPropertyName("matchCount")]
    public required int MatchCount { get; init; }

    [JsonPropertyName("matches")]
    public required IReadOnlyList<SearchMatch> Matches { get; init; }
}
