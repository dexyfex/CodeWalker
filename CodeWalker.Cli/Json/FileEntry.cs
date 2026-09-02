using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CodeWalker.Cli.Json;

[ExcludeFromCodeCoverage]
internal sealed record FileEntry
{
    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("size")]
    public required long Size { get; init; }

    [JsonPropertyName("sizeFormatted")]
    public required string SizeFormatted { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("extension")]
    public required string Extension { get; init; }

    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Status { get; init; }
}
