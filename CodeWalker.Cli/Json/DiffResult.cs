using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using CodeWalker.Cli.Helpers;

namespace CodeWalker.Cli.Json;

[ExcludeFromCodeCoverage]
internal sealed record DiffResult : BaseResult
{
    [JsonPropertyName("leftRpf")]
    public required string LeftRpf { get; init; }

    [JsonPropertyName("rightRpf")]
    public required string RightRpf { get; init; }

    [JsonPropertyName("added")]
    public required IReadOnlyList<DiffEntry> Added { get; init; }

    [JsonPropertyName("removed")]
    public required IReadOnlyList<DiffEntry> Removed { get; init; }

    [JsonPropertyName("modified")]
    public required IReadOnlyList<DiffEntry> Modified { get; init; }

    [JsonPropertyName("unchanged")]
    public required IReadOnlyList<DiffEntry> Unchanged { get; init; }

    [JsonPropertyName("summary")]
    public required DiffSummary Summary { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record DiffEntry
{
    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Size { get; init; }

    [JsonPropertyName("sizeFormatted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SizeFormatted { get; init; }

    [JsonPropertyName("leftSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? LeftSize { get; init; }

    [JsonPropertyName("leftSizeFormatted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LeftSizeFormatted { get; init; }

    [JsonPropertyName("rightSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? RightSize { get; init; }

    [JsonPropertyName("rightSizeFormatted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RightSizeFormatted { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record DiffSummary
{
    [JsonPropertyName("addedCount")]
    public required int AddedCount { get; init; }

    [JsonPropertyName("removedCount")]
    public required int RemovedCount { get; init; }

    [JsonPropertyName("modifiedCount")]
    public required int ModifiedCount { get; init; }

    [JsonPropertyName("unchangedCount")]
    public required int UnchangedCount { get; init; }
}
