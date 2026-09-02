using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using CodeWalker.Cli.Helpers;

namespace CodeWalker.Cli.Json;

[ExcludeFromCodeCoverage]
internal sealed record Gen9Result : BaseResult
{
    [JsonPropertyName("inputFolder")]
    public required string InputFolder { get; init; }

    [JsonPropertyName("outputFolder")]
    public required string OutputFolder { get; init; }

    [JsonPropertyName("totalFiles")]
    public required int TotalFiles { get; init; }

    [JsonPropertyName("converted")]
    public required int Converted { get; init; }

    [JsonPropertyName("skipped")]
    public required int Skipped { get; init; }

    [JsonPropertyName("copied")]
    public required int Copied { get; init; }

    [JsonPropertyName("errors")]
    public required int Errors { get; init; }

    [JsonPropertyName("files")]
    public required IReadOnlyList<Gen9FileEntry> Files { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record Gen9FileEntry
{
    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; init; }
}
