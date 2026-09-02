using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using CodeWalker.Cli.Helpers;

namespace CodeWalker.Cli.Json;

[ExcludeFromCodeCoverage]
internal sealed record HashResult : BaseResult
{
    [JsonPropertyName("hashes")]
    public required IReadOnlyList<HashEntry> Hashes { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record HashEntry
{
    [JsonPropertyName("input")]
    public required string Input { get; init; }

    [JsonPropertyName("hash")]
    public required uint Hash { get; init; }

    [JsonPropertyName("hashSigned")]
    public required int HashSigned { get; init; }

    [JsonPropertyName("hashHex")]
    public required string HashHex { get; init; }

    [JsonPropertyName("encoding")]
    public required string Encoding { get; init; }
}
