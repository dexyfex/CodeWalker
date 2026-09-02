using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

using CodeWalker.Cli.Helpers;
using CodeWalker.GameFiles;

using SharpDX;

namespace CodeWalker.Cli.Handlers;

internal sealed record InspectOptions
{
    public required string RpfPath { get; init; }
    public required string ExePath { get; init; }
    public required bool Gen9 { get; init; }
    public required string[] Filters { get; init; }
    public required bool Verbose { get; init; }
    public required bool Json { get; init; }
    public required bool Recursive { get; init; }
    public required SizeFormat SizeFormat { get; init; }
    public required string FilePath { get; init; }
}

internal static class InspectHandler
{
    public static Command CreateCommand(CancellationToken cancellationToken = default)
    {
        Option<FileInfo> rpfOpt = CliOptions.Rpf();
        Option<DirectoryInfo> exeOpt = CliOptions.Exe();
        Option<bool> gen9Opt = CliOptions.Gen9();
        Option<string[]> filterOpt = CliOptions.Filter();
        Option<bool> recursiveOpt = CliOptions.Recursive();
        Option<bool> verboseOpt = CliOptions.Verbose();
        Option<bool> jsonOpt = CliOptions.Json();
        Option<bool> siOpt = CliOptions.Si();

        Argument<string> pathArg = new("path")
        {
            Description = "Path of the file within the RPF archive",
        };

        Command command = new(
            "inspect",
            "Show detailed metadata for a specific file in an RPF archive"
        )
        {
            pathArg,
            rpfOpt,
            exeOpt,
            gen9Opt,
            filterOpt,
            recursiveOpt,
            verboseOpt,
            jsonOpt,
            siOpt,
        };
        command.Aliases.Add("i");

        command.SetAction(parseResult =>
        {
            InspectOptions options = new()
            {
                RpfPath = parseResult.GetRequiredValue(rpfOpt).FullName,
                ExePath = parseResult.GetRequiredValue(exeOpt).FullName,
                Gen9 = parseResult.GetValue(gen9Opt),
                Filters = Filter.Normalize(parseResult.GetValue(filterOpt)),
                Verbose = parseResult.GetValue(verboseOpt),
                Json = parseResult.GetValue(jsonOpt),
                Recursive = parseResult.GetValue(recursiveOpt),
                SizeFormat = parseResult.GetValue(siOpt) ? SizeFormat.SI : SizeFormat.IEC,
                FilePath = parseResult.GetRequiredValue(pathArg),
            };
            return Execute(options, cancellationToken);
        });

        return command;
    }

    public static int Execute(InspectOptions options, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Json.InspectResult ErrorResult(string[] errorMessages) =>
            new()
            {
                Success = false,
                RpfFile = options.RpfPath,
                Path = options.FilePath,
                Name = "",
                Size = 0,
                SizeFormatted = "0 B",
                Type = "",
                Extension = "",
                NameHash = 0,
                ShortNameHash = 0,
                ErrorMessages = errorMessages,
            };

        string? initError = RpfHelper.ValidateAndLoadKeys(
            options.RpfPath,
            options.ExePath,
            options.Gen9,
            options.Json
        );
        if (initError != null)
        {
            return Output.ReportError(initError, options.Json, ErrorResult([]));
        }

        List<string> scanErrors = [];
        try
        {
            RpfFile rpf = RpfHelper.OpenRpf(
                options.RpfPath,
                options.Verbose,
                options.Json,
                scanErrors
            );

            if (!options.Json)
            {
                Console.Error.WriteLine();
            }

            string normalizedPath = options.FilePath.Replace('\\', '/');
            RpfFileEntry? found = FindEntry(rpf, normalizedPath, options.Recursive);

            if (found == null)
            {
                return Output.ReportError(
                    $"File not found in archive: {options.FilePath}",
                    options.Json,
                    ErrorResult([.. scanErrors])
                );
            }

            long size = found.GetFileSize();
            string ext = Path.GetExtension(found.Name).ToLowerInvariant();
            string fileType = RpfHelper.GetFileType(found);

            int? resourceVersion = null;
            long? systemSize = null;
            long? graphicsSize = null;
            long? uncompressedSize = null;
            uint? encryptionType = null;

            if (found is RpfResourceFileEntry rfe)
            {
                resourceVersion = rfe.Version;
                systemSize = rfe.SystemSize;
                graphicsSize = rfe.GraphicsSize;
            }
            else if (found is RpfBinaryFileEntry bfe)
            {
                uncompressedSize = bfe.FileUncompressedSize;
                encryptionType = bfe.EncryptionType;
            }

            Json.InspectResult result = new()
            {
                Success = scanErrors.Count == 0,
                RpfFile = options.RpfPath,
                Path = found.Path,
                Name = found.Name,
                Size = size,
                SizeFormatted = options.SizeFormat.ToFormattedString(size),
                Type = fileType,
                Extension = ext,
                NameHash = found.NameHash,
                ShortNameHash = found.ShortNameHash,
                ResourceVersion = resourceVersion,
                SystemSize = systemSize,
                GraphicsSize = graphicsSize,
                UncompressedSize = uncompressedSize,
                EncryptionType = encryptionType,
                Details = GetDetails(found, ext, options.Verbose),
                ErrorMessages = [.. scanErrors],
            };

            if (options.Json)
            {
                Console.WriteLine(
                    JsonSerializer.Serialize(result, Output.JsonSerializerOptions)
                );
            }
            else
            {
                PrintTextResult(result, options);
            }

            return scanErrors.Count > 0 ? 1 : 0;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return Output.ReportError(
                ex.Message,
                options.Json,
                ErrorResult([.. scanErrors]),
                options.Verbose ? ex.StackTrace : null
            );
        }
    }

    private static RpfFileEntry? FindEntry(RpfFile rpf, string normalizedPath, bool recursive)
    {
        RpfFileEntry? found = rpf.AllEntries?
            .OfType<RpfFileEntry>()
            .FirstOrDefault(fe =>
                fe.Path?.Replace('\\', '/').Equals(normalizedPath, StringComparison.OrdinalIgnoreCase) == true);

        if (found != null)
            return found;

        if (recursive && rpf.Children != null)
        {
            foreach (RpfFile child in rpf.Children)
            {
                found = FindEntry(child, normalizedPath, recursive);
                if (found != null)
                    return found;
            }
        }

        return null;
    }

    private static Json.InspectDetailBase? GetDetails(RpfFileEntry entry, string ext, bool verbose)
    {
        try
        {
            return ext switch
            {
                ".ytd" => GetYtdDetails(entry),
                ".ydr" => GetYdrDetails(entry),
                ".ydd" => GetYddDetails(entry),
                ".yft" => GetYftDetails(entry),
                ".ymap" => GetYmapDetails(entry),
                ".ytyp" => GetYtypDetails(entry),
                ".ybn" => GetYbnDetails(entry),
                ".awc" => GetAwcDetails(entry),
                ".gxt2" => GetGxt2Details(entry),
                _ => null,
            };
        }
        catch (Exception ex)
        {
            if (verbose)
            {
                Console.Error.WriteLine($"Warning: Failed to read details for {entry.Path}: {ex.Message}");
            }
            return null;
        }
    }

    private static Json.YtdDetails? GetYtdDetails(RpfFileEntry entry)
    {
        YtdFile file = RpfFile.GetFile<YtdFile>(entry);
        if (file?.TextureDict?.Textures?.data_items == null)
            return null;

        Texture[] textures = file.TextureDict.Textures.data_items;
        List<Json.TextureInfo> infos = textures
            .Where(tex => tex != null)
            .Select(tex => new Json.TextureInfo
            {
                Name = tex.Name ?? "",
                Width = tex.Width,
                Height = tex.Height,
                Format = tex.Format.ToString(),
                MipLevels = tex.Levels,
                Stride = tex.Stride,
            })
            .ToList();

        return new Json.YtdDetails { TextureCount = infos.Count, Textures = infos };
    }

    private static Json.YdrDetails? GetYdrDetails(RpfFileEntry entry)
    {
        YdrFile file = RpfFile.GetFile<YdrFile>(entry);
        if (file?.Drawable?.DrawableModels == null)
            return null;

        return new Json.YdrDetails { Lods = GetLodInfos(file.Drawable.DrawableModels) };
    }

    private static Json.YddDetails? GetYddDetails(RpfFileEntry entry)
    {
        YddFile file = RpfFile.GetFile<YddFile>(entry);
        if (file?.DrawableDict?.Drawables?.data_items == null)
            return null;

        Drawable?[] drawables = file.DrawableDict.Drawables.data_items;
        List<Json.DrawableInfo> infos = drawables
            .Where(d => d != null)
            .Select(d =>
            {
                DrawableGeometry[] geoms = (d!.AllModels ?? [])
                    .Where(m => m?.Geometries != null)
                    .SelectMany(m => m.Geometries)
                    .ToArray();
                return new Json.DrawableInfo
                {
                    Name = d.Name ?? "",
                    TotalVertices = geoms.Sum(g => (long)g.VerticesCount),
                    TotalTriangles = geoms.Sum(g => (long)g.TrianglesCount),
                };
            })
            .ToList();

        return new Json.YddDetails { DrawableCount = infos.Count, Drawables = infos };
    }

    private static Json.YftDetails? GetYftDetails(RpfFileEntry entry)
    {
        YftFile file = RpfFile.GetFile<YftFile>(entry);
        if (file?.Fragment == null)
            return null;

        List<Json.LodInfo> lods = [];
        if (file.Fragment.Drawable?.DrawableModels != null)
        {
            lods = GetLodInfos(file.Fragment.Drawable.DrawableModels);
        }

        return new Json.YftDetails
        {
            Lods = lods,
            HasDrawableCloth = file.Fragment.DrawableCloth != null,
        };
    }

    private static Json.YmapDetails? GetYmapDetails(RpfFileEntry entry)
    {
        YmapFile file = RpfFile.GetFile<YmapFile>(entry);
        if (file == null)
            return null;

        string? entExtMin = null;
        string? entExtMax = null;
        string? strExtMin = null;
        string? strExtMax = null;

        if (file._CMapData.entitiesExtentsMin != default)
            entExtMin = FormatVector3(file._CMapData.entitiesExtentsMin);
        if (file._CMapData.entitiesExtentsMax != default)
            entExtMax = FormatVector3(file._CMapData.entitiesExtentsMax);
        if (file._CMapData.streamingExtentsMin != default)
            strExtMin = FormatVector3(file._CMapData.streamingExtentsMin);
        if (file._CMapData.streamingExtentsMax != default)
            strExtMax = FormatVector3(file._CMapData.streamingExtentsMax);

        return new Json.YmapDetails
        {
            EntityCount = file.AllEntities?.Length ?? 0,
            CarGeneratorCount = file.CarGenerators?.Length ?? 0,
            EntitiesExtentsMin = entExtMin,
            EntitiesExtentsMax = entExtMax,
            StreamingExtentsMin = strExtMin,
            StreamingExtentsMax = strExtMax,
            IsScripted = file.IsScripted,
        };
    }

    private static Json.YtypDetails? GetYtypDetails(RpfFileEntry entry)
    {
        YtypFile file = RpfFile.GetFile<YtypFile>(entry);
        if (file?.AllArchetypes == null)
            return null;

        List<Json.MloInfo> mloDetails = file.AllArchetypes
            .OfType<MloArchetype>()
            .Select(mlo => new Json.MloInfo
            {
                Name = mlo.Hash.ToString(),
                EntityCount = mlo.entities?.Length ?? 0,
                RoomCount = mlo.rooms?.Length ?? 0,
                PortalCount = mlo.portals?.Length ?? 0,
            })
            .ToList();

        int mloCount = mloDetails.Count;
        int timeCount = file.AllArchetypes.OfType<TimeArchetype>().Count();
        int baseCount = file.AllArchetypes.Length - mloCount - timeCount;

        return new Json.YtypDetails
        {
            ArchetypeCount = file.AllArchetypes.Length,
            BaseCount = baseCount,
            TimeCount = timeCount,
            MloCount = mloCount,
            MloDetails = mloDetails.Count > 0 ? mloDetails : null,
        };
    }

    private static Json.YbnDetails? GetYbnDetails(RpfFileEntry entry)
    {
        YbnFile file = RpfFile.GetFile<YbnFile>(entry);
        if (file?.Bounds == null)
            return null;

        int? childCount = null;
        if (file.Bounds is BoundComposite composite)
        {
            childCount = composite.Children?.data_items?.Length ?? 0;
        }

        return new Json.YbnDetails
        {
            BoundsType = file.Bounds.Type.ToString(),
            ChildCount = childCount,
        };
    }

    private static Json.AwcDetails? GetAwcDetails(RpfFileEntry entry)
    {
        AwcFile file = RpfFile.GetFile<AwcFile>(entry);
        if (file?.Streams == null)
            return null;

        List<Json.AwcStreamInfo> infos = file.Streams
            .Where(s => s?.StreamInfo != null)
            .Select(s =>
            {
                AwcFormatChunk? fmt = s.FormatChunk;
                return new Json.AwcStreamInfo
                {
                    Id = s.StreamInfo.Id,
                    SamplesPerSecond = fmt?.SamplesPerSecond ?? 0,
                    Codec = fmt?.Codec.ToString() ?? "unknown",
                    Samples = fmt?.Samples ?? 0,
                };
            })
            .ToList();

        return new Json.AwcDetails { StreamCount = infos.Count, Streams = infos };
    }

    private static Json.Gxt2Details? GetGxt2Details(RpfFileEntry entry)
    {
        Gxt2File file = RpfFile.GetFile<Gxt2File>(entry);
        if (file?.TextEntries == null)
            return null;

        List<Json.Gxt2EntryInfo> infos = file.TextEntries
            .Take(50)
            .Select(e =>
            {
                string text = e.Text ?? "";
                if (text.Length > 100)
                    text = text[..100] + "...";
                return new Json.Gxt2EntryInfo { Hash = $"0x{e.Hash:X8}", Text = text };
            })
            .ToList();

        return new Json.Gxt2Details { EntryCount = file.TextEntries.Length, Entries = infos };
    }

    private static List<Json.LodInfo> GetLodInfos(DrawableModelsBlock models)
    {
        List<Json.LodInfo> lods = [];
        AddLod(lods, "High", models.High);
        AddLod(lods, "Med", models.Med);
        AddLod(lods, "Low", models.Low);
        AddLod(lods, "VLow", models.VLow);
        return lods;
    }

    private static void AddLod(List<Json.LodInfo> lods, string level, DrawableModel[]? models)
    {
        if (models == null || models.Length == 0)
            return;

        DrawableGeometry[] allGeoms = models
            .Where(m => m?.Geometries != null)
            .SelectMany(m => m.Geometries)
            .ToArray();

        lods.Add(
            new Json.LodInfo
            {
                Level = level,
                ModelCount = models.Length,
                GeometryCount = allGeoms.Length,
                TotalVertices = allGeoms.Sum(g => (long)g.VerticesCount),
                TotalTriangles = allGeoms.Sum(g => (long)g.TrianglesCount),
            }
        );
    }

    internal static string FormatVector3(Vector3 v) =>
        string.Format(CultureInfo.InvariantCulture, "{0:F2}, {1:F2}, {2:F2}", v.X, v.Y, v.Z);

    private static void PrintTextResult(Json.InspectResult result, InspectOptions options)
    {
        Console.WriteLine($"Path:       {result.Path}");
        Console.WriteLine($"Name:       {result.Name}");
        Console.WriteLine($"Size:       {result.SizeFormatted} ({result.Size} bytes)");
        Console.WriteLine($"Type:       {result.Type}");
        Console.WriteLine($"Extension:  {result.Extension}");
        Console.WriteLine($"NameHash:   0x{result.NameHash:X8}");
        Console.WriteLine($"ShortHash:  0x{result.ShortNameHash:X8}");

        if (result.ResourceVersion != null)
        {
            Console.WriteLine($"Version:    {result.ResourceVersion}");
            Console.WriteLine($"SystemSize: {result.SystemSize}");
            Console.WriteLine($"GraphSize:  {result.GraphicsSize}");
        }

        if (result.UncompressedSize != null)
        {
            Console.WriteLine(
                $"Uncompressed: {options.SizeFormat.ToFormattedString(result.UncompressedSize.Value)}"
            );
            Console.WriteLine($"Encryption:   {result.EncryptionType}");
        }

        if (result.Details == null)
            return;

        Console.WriteLine();

        switch (result.Details)
        {
            case Json.YtdDetails ytd:
                Console.WriteLine($"Textures: {ytd.TextureCount}");
                foreach (Json.TextureInfo tex in ytd.Textures)
                {
                    Console.WriteLine(
                        $"  {tex.Name}: {tex.Width}x{tex.Height} {tex.Format} mips={tex.MipLevels} stride={tex.Stride}"
                    );
                }
                break;

            case Json.YdrDetails ydr:
                PrintLods(ydr.Lods);
                break;

            case Json.YddDetails ydd:
                Console.WriteLine($"Drawables: {ydd.DrawableCount}");
                foreach (Json.DrawableInfo d in ydd.Drawables)
                {
                    Console.WriteLine(
                        $"  {d.Name}: {d.TotalVertices} vertices, {d.TotalTriangles} triangles"
                    );
                }
                break;

            case Json.YftDetails yft:
                PrintLods(yft.Lods);
                Console.WriteLine($"DrawableCloth: {(yft.HasDrawableCloth ? "yes" : "no")}");
                break;

            case Json.YmapDetails ymap:
                Console.WriteLine($"Entities:       {ymap.EntityCount}");
                Console.WriteLine($"Car Generators: {ymap.CarGeneratorCount}");
                if (ymap.EntitiesExtentsMin != null)
                {
                    Console.WriteLine(
                        $"Entity Extents: [{ymap.EntitiesExtentsMin}] to [{ymap.EntitiesExtentsMax}]"
                    );
                }
                if (ymap.StreamingExtentsMin != null)
                {
                    Console.WriteLine(
                        $"Stream Extents: [{ymap.StreamingExtentsMin}] to [{ymap.StreamingExtentsMax}]"
                    );
                }
                Console.WriteLine($"Scripted:       {(ymap.IsScripted ? "yes" : "no")}");
                break;

            case Json.YtypDetails ytyp:
                Console.WriteLine($"Archetypes: {ytyp.ArchetypeCount}");
                Console.WriteLine(
                    $"  Base: {ytyp.BaseCount}, Time: {ytyp.TimeCount}, MLO: {ytyp.MloCount}"
                );
                if (ytyp.MloDetails != null)
                {
                    foreach (Json.MloInfo mlo in ytyp.MloDetails)
                    {
                        Console.WriteLine(
                            $"  MLO {mlo.Name}: {mlo.EntityCount} entities, {mlo.RoomCount} rooms, {mlo.PortalCount} portals"
                        );
                    }
                }
                break;

            case Json.YbnDetails ybn:
                Console.WriteLine($"Bounds Type: {ybn.BoundsType}");
                if (ybn.ChildCount != null)
                    Console.WriteLine($"Children:    {ybn.ChildCount}");
                break;

            case Json.AwcDetails awc:
                Console.WriteLine($"Streams: {awc.StreamCount}");
                foreach (Json.AwcStreamInfo s in awc.Streams)
                {
                    Console.WriteLine(
                        $"  Stream {s.Id}: {s.Codec} {s.SamplesPerSecond}Hz {s.Samples} samples"
                    );
                }
                break;

            case Json.Gxt2Details gxt2:
                Console.WriteLine($"Text Entries: {gxt2.EntryCount}");
                foreach (Json.Gxt2EntryInfo e in gxt2.Entries)
                {
                    Console.WriteLine($"  {e.Hash}: {e.Text}");
                }
                if (gxt2.EntryCount > gxt2.Entries.Count)
                    Console.WriteLine($"  ... and {gxt2.EntryCount - gxt2.Entries.Count} more");
                break;
        }
    }

    private static void PrintLods(IReadOnlyList<Json.LodInfo> lods)
    {
        foreach (Json.LodInfo lod in lods)
        {
            Console.WriteLine(
                $"  {lod.Level}: {lod.ModelCount} models, {lod.GeometryCount} geometries, {lod.TotalVertices} vertices, {lod.TotalTriangles} triangles"
            );
        }
    }
}
