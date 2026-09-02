using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using CodeWalker.Cli.Helpers;

namespace CodeWalker.Cli.Json;

[ExcludeFromCodeCoverage]
internal sealed record InspectResult : BaseResult
{
    [JsonPropertyName("rpfFile")]
    public required string RpfFile { get; init; }

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

    [JsonPropertyName("nameHash")]
    public required uint NameHash { get; init; }

    [JsonPropertyName("shortNameHash")]
    public required uint ShortNameHash { get; init; }

    [JsonPropertyName("resourceVersion")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ResourceVersion { get; init; }

    [JsonPropertyName("systemSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? SystemSize { get; init; }

    [JsonPropertyName("graphicsSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? GraphicsSize { get; init; }

    [JsonPropertyName("uncompressedSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? UncompressedSize { get; init; }

    [JsonPropertyName("encryptionType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public uint? EncryptionType { get; init; }

    [JsonPropertyName("details")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InspectDetailBase? Details { get; init; }
}

[JsonPolymorphic]
[JsonDerivedType(typeof(YtdDetails))]
[JsonDerivedType(typeof(YdrDetails))]
[JsonDerivedType(typeof(YddDetails))]
[JsonDerivedType(typeof(YftDetails))]
[JsonDerivedType(typeof(YmapDetails))]
[JsonDerivedType(typeof(YtypDetails))]
[JsonDerivedType(typeof(YbnDetails))]
[JsonDerivedType(typeof(AwcDetails))]
[JsonDerivedType(typeof(Gxt2Details))]
[ExcludeFromCodeCoverage]
internal abstract record InspectDetailBase;

[ExcludeFromCodeCoverage]
internal sealed record TextureInfo
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("width")]
    public required ushort Width { get; init; }

    [JsonPropertyName("height")]
    public required ushort Height { get; init; }

    [JsonPropertyName("format")]
    public required string Format { get; init; }

    [JsonPropertyName("mipLevels")]
    public required byte MipLevels { get; init; }

    [JsonPropertyName("stride")]
    public required ushort Stride { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record YtdDetails : InspectDetailBase
{
    [JsonPropertyName("textureCount")]
    public required int TextureCount { get; init; }

    [JsonPropertyName("textures")]
    public required IReadOnlyList<TextureInfo> Textures { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record LodInfo
{
    [JsonPropertyName("level")]
    public required string Level { get; init; }

    [JsonPropertyName("modelCount")]
    public required int ModelCount { get; init; }

    [JsonPropertyName("geometryCount")]
    public required int GeometryCount { get; init; }

    [JsonPropertyName("totalVertices")]
    public required long TotalVertices { get; init; }

    [JsonPropertyName("totalTriangles")]
    public required long TotalTriangles { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record YdrDetails : InspectDetailBase
{
    [JsonPropertyName("lods")]
    public required IReadOnlyList<LodInfo> Lods { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record DrawableInfo
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("totalVertices")]
    public required long TotalVertices { get; init; }

    [JsonPropertyName("totalTriangles")]
    public required long TotalTriangles { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record YddDetails : InspectDetailBase
{
    [JsonPropertyName("drawableCount")]
    public required int DrawableCount { get; init; }

    [JsonPropertyName("drawables")]
    public required IReadOnlyList<DrawableInfo> Drawables { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record YftDetails : InspectDetailBase
{
    [JsonPropertyName("lods")]
    public required IReadOnlyList<LodInfo> Lods { get; init; }

    [JsonPropertyName("hasDrawableCloth")]
    public required bool HasDrawableCloth { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record YmapDetails : InspectDetailBase
{
    [JsonPropertyName("entityCount")]
    public required int EntityCount { get; init; }

    [JsonPropertyName("carGeneratorCount")]
    public required int CarGeneratorCount { get; init; }

    [JsonPropertyName("entitiesExtentsMin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EntitiesExtentsMin { get; init; }

    [JsonPropertyName("entitiesExtentsMax")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EntitiesExtentsMax { get; init; }

    [JsonPropertyName("streamingExtentsMin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StreamingExtentsMin { get; init; }

    [JsonPropertyName("streamingExtentsMax")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StreamingExtentsMax { get; init; }

    [JsonPropertyName("isScripted")]
    public required bool IsScripted { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record YtypDetails : InspectDetailBase
{
    [JsonPropertyName("archetypeCount")]
    public required int ArchetypeCount { get; init; }

    [JsonPropertyName("baseCount")]
    public required int BaseCount { get; init; }

    [JsonPropertyName("timeCount")]
    public required int TimeCount { get; init; }

    [JsonPropertyName("mloCount")]
    public required int MloCount { get; init; }

    [JsonPropertyName("mloDetails")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<MloInfo>? MloDetails { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record MloInfo
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("entityCount")]
    public required int EntityCount { get; init; }

    [JsonPropertyName("roomCount")]
    public required int RoomCount { get; init; }

    [JsonPropertyName("portalCount")]
    public required int PortalCount { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record YbnDetails : InspectDetailBase
{
    [JsonPropertyName("boundsType")]
    public required string BoundsType { get; init; }

    [JsonPropertyName("childCount")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ChildCount { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record AwcStreamInfo
{
    [JsonPropertyName("id")]
    public required uint Id { get; init; }

    [JsonPropertyName("samplesPerSecond")]
    public required ushort SamplesPerSecond { get; init; }

    [JsonPropertyName("codec")]
    public required string Codec { get; init; }

    [JsonPropertyName("samples")]
    public required uint Samples { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record AwcDetails : InspectDetailBase
{
    [JsonPropertyName("streamCount")]
    public required int StreamCount { get; init; }

    [JsonPropertyName("streams")]
    public required IReadOnlyList<AwcStreamInfo> Streams { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record Gxt2EntryInfo
{
    [JsonPropertyName("hash")]
    public required string Hash { get; init; }

    [JsonPropertyName("text")]
    public required string Text { get; init; }
}

[ExcludeFromCodeCoverage]
internal sealed record Gxt2Details : InspectDetailBase
{
    [JsonPropertyName("entryCount")]
    public required int EntryCount { get; init; }

    [JsonPropertyName("entries")]
    public required IReadOnlyList<Gxt2EntryInfo> Entries { get; init; }
}
