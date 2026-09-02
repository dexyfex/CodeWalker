using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using CodeWalker.Cli.Helpers;

namespace CodeWalker.Cli.Json;

[ExcludeFromCodeCoverage]
internal sealed record ValidateFileEntry
{
    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record ValidateResult : BaseResult
{
    [JsonPropertyName("rpfFile")]
    public required string RpfFile { get; init; }

    [JsonPropertyName("totalFiles")]
    public required int TotalFiles { get; init; }

    [JsonPropertyName("valid")]
    public required int Valid { get; init; }

    [JsonPropertyName("warnings")]
    public required int Warnings { get; init; }

    [JsonPropertyName("errors")]
    public required int Errors { get; init; }

    [JsonPropertyName("skipped")]
    public required int Skipped { get; init; }

    [JsonPropertyName("files")]
    public required IReadOnlyList<ValidateFileEntry> Files { get; init; }
}
