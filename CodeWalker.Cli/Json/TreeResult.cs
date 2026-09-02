using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using CodeWalker.Cli.Helpers;

namespace CodeWalker.Cli.Json;

[ExcludeFromCodeCoverage]
internal sealed record TreeResult : BaseResult
{
    [JsonPropertyName("rpfFile")]
    public required string RpfFile { get; init; }

    [JsonPropertyName("totalFiles")]
    public required int TotalFiles { get; init; }

    [JsonPropertyName("totalDirs")]
    public required int TotalDirs { get; init; }

    [JsonPropertyName("root")]
    public required TreeNode? Root { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record TreeNode
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Size { get; init; }

    [JsonPropertyName("sizeFormatted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SizeFormatted { get; init; }

    [JsonPropertyName("fileType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FileType { get; init; }

    [JsonPropertyName("version")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Version { get; init; }

    [JsonPropertyName("children")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<TreeNode>? Children { get; init; }
}
