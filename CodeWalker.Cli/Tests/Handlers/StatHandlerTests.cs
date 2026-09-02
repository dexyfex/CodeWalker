using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

using CodeWalker.Cli.Handlers;
using CodeWalker.Cli.Helpers;
using CodeWalker.GameFiles;

using Xunit;

namespace CodeWalker.Cli.Tests.Handlers;

// ErrorResult

public sealed class StatErrorResultTests
{
    private static StatOptions MakeOptions(string rpfPath = "/test.rpf") =>
        new()
        {
            RpfPath = rpfPath,
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = [],
            Verbose = false,
            Json = false,
            Recursive = false,
            SizeFormat = SizeFormat.IEC
        };

    [Fact]
    public void ErrorResult_SetsSuccessFalse()
    {
        Json.StatResult result = StatHandler.ErrorResult([], MakeOptions());
        Assert.False(result.Success);
    }

    [Fact]
    public void ErrorResult_PreservesErrorMessages()
    {
        string[] msgs = ["err1", "err2"];
        Json.StatResult result = StatHandler.ErrorResult(msgs, MakeOptions());
        Assert.Equal(msgs, result.ErrorMessages);
    }

    [Fact]
    public void ErrorResult_SetsRpfFile()
    {
        Json.StatResult result = StatHandler.ErrorResult([], MakeOptions("/my/test.rpf"));
        Assert.Equal("/my/test.rpf", result.RpfFile);
    }

    [Fact]
    public void ErrorResult_AllStatsAreZero()
    {
        Json.StatResult result = StatHandler.ErrorResult([], MakeOptions());
        Assert.Equal(0, result.TotalFiles);
        Assert.Equal(0, result.TotalSize);
        Assert.Equal(0, result.ResourceCount);
        Assert.Equal(0, result.BinaryCount);
        Assert.Equal(0, result.CompressedSize);
        Assert.Equal(0, result.UncompressedSize);
        Assert.Equal(0, result.CompressionRatio);
    }

    [Fact]
    public void ErrorResult_FormattedSizesAreZeroB()
    {
        Json.StatResult result = StatHandler.ErrorResult([], MakeOptions());
        Assert.Equal("0 B", result.TotalSizeFormatted);
        Assert.Equal("0 B", result.CompressedSizeFormatted);
        Assert.Equal("0 B", result.UncompressedSizeFormatted);
    }

    [Fact]
    public void ErrorResult_ExtensionsAreEmpty()
    {
        Json.StatResult result = StatHandler.ErrorResult([], MakeOptions());
        Assert.Empty(result.Extensions);
    }
}

// CollectStats

public sealed class CollectStatsTests
{
    private static StatOptions MakeOptions(SizeFormat fmt = SizeFormat.IEC) =>
        new()
        {
            RpfPath = "/test.rpf",
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = [],
            Verbose = false,
            Json = false,
            Recursive = false,
            SizeFormat = fmt
        };

    private static RpfBinaryFileEntry MakeBinary(string name, uint fileSize, uint uncompressedSize) =>
        new()
        {
            Name = name,
            FileSize = fileSize,
            FileUncompressedSize = uncompressedSize
        };

    private static RpfResourceFileEntry MakeResource(string name, uint fileSize, uint sysFlags, uint gfxFlags) =>
        new()
        {
            Name = name,
            FileSize = fileSize,
            SystemFlags = new RpfResourcePageFlags(sysFlags),
            GraphicsFlags = new RpfResourcePageFlags(gfxFlags)
        };

    [Fact]
    public void EmptyEntries_ReturnsAllZeros()
    {
        Json.StatResult result = StatHandler.CollectStats(
            [],
            [],
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        Assert.True(result.Success);
        Assert.Equal(0, result.TotalFiles);
        Assert.Equal(0, result.TotalSize);
        Assert.Equal(0, result.ResourceCount);
        Assert.Equal(0, result.BinaryCount);
        Assert.Equal(0, result.CompressedSize);
        Assert.Equal(0, result.UncompressedSize);
        Assert.Equal(0, result.CompressionRatio);
        Assert.Empty(result.Extensions);
    }

    [Fact]
    public void SingleBinary_CountsCorrectly()
    {
        RpfBinaryFileEntry entry = MakeBinary(
            "data.dat",
            fileSize: 200,
            uncompressedSize: 400
        );

        List<(RpfFile, RpfFileEntry)> entries = [
            (null!, entry)
        ];

        Json.StatResult result = StatHandler.CollectStats(
            entries,
            [],
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        Assert.Equal(1, result.TotalFiles);
        Assert.Equal(200, result.TotalSize); // GetFileSize() returns FileSize when non-zero
        Assert.Equal(0, result.ResourceCount);
        Assert.Equal(1, result.BinaryCount);
        Assert.Equal(200, result.CompressedSize);
        Assert.Equal(400, result.UncompressedSize);
    }

    [Fact]
    public void SingleResource_CountsCorrectly()
    {
        // 0x08000000 -> SystemFlags.Size = 512, 0x04000000 -> GraphicsFlags.Size = 1024
        RpfResourceFileEntry entry = MakeResource(
            "model.ydr",
            fileSize: 300,
            sysFlags: 0x08000000,
            gfxFlags: 0x04000000
        );

        List<(RpfFile, RpfFileEntry)> entries = [
            (null!, entry)
        ];

        Json.StatResult result = StatHandler.CollectStats(
            entries,
            [],
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        Assert.Equal(1, result.TotalFiles);
        Assert.Equal(300, result.TotalSize); // GetFileSize() returns FileSize when non-zero
        Assert.Equal(1, result.ResourceCount);
        Assert.Equal(0, result.BinaryCount);
        Assert.Equal(300, result.CompressedSize);
        Assert.Equal(512 + 1024, result.UncompressedSize);
    }

    [Fact]
    public void MixedEntries_AggregatesCorrectly()
    {
        RpfBinaryFileEntry bin = MakeBinary(
            "data.dat",
            fileSize: 200,
            uncompressedSize: 400
        );

        RpfResourceFileEntry res = MakeResource(
            "model.ydr",
            fileSize: 300,
            sysFlags: 0x08000000,
            gfxFlags: 0x04000000
        );

        List<(RpfFile, RpfFileEntry)> entries = [
            (null!, bin),
            (null!, res)
        ];

        Json.StatResult result = StatHandler.CollectStats(
            entries,
            [],
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        Assert.Equal(2, result.TotalFiles);
        Assert.Equal(200 + 300, result.TotalSize);
        Assert.Equal(1, result.ResourceCount);
        Assert.Equal(1, result.BinaryCount);
        Assert.Equal(200 + 300, result.CompressedSize);
        Assert.Equal(400 + 512 + 1024, result.UncompressedSize);
    }

    [Fact]
    public void CompressionRatio_CalculatedCorrectly()
    {
        RpfBinaryFileEntry entry = MakeBinary(
            "data.dat",
            fileSize: 250,
            uncompressedSize: 1000
        );

        List<(RpfFile, RpfFileEntry)> entries = [
            (null!, entry)
        ];

        Json.StatResult result = StatHandler.CollectStats(
            entries,
            [],
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        Assert.Equal(0.25, result.CompressionRatio);
    }

    [Fact]
    public void CompressionRatio_ZeroWhenNoUncompressed()
    {
        // Entry with FileSize=0 and FileUncompressedSize=0 -> GetFileSize() returns 0
        RpfBinaryFileEntry entry = MakeBinary(
            "empty.dat",
            fileSize: 0,
            uncompressedSize: 0
        );

        List<(RpfFile, RpfFileEntry)> entries = [
            (null!, entry)
        ];

        Json.StatResult result = StatHandler.CollectStats(
            entries,
            [],
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        Assert.Equal(0, result.CompressionRatio);
    }

    [Fact]
    public void ExtensionStats_GroupedAndSorted()
    {
        RpfBinaryFileEntry small = MakeBinary(
            "a.dat",
            fileSize: 100,
            uncompressedSize: 100
        );
        RpfBinaryFileEntry large1 = MakeBinary(
            "b.ydr",
            fileSize: 500,
            uncompressedSize: 500
        );
        RpfBinaryFileEntry large2 = MakeBinary(
            "c.ydr",
            fileSize: 600,
            uncompressedSize: 600
        );

        List<(RpfFile, RpfFileEntry)> entries = [
            (null!, small),
            (null!, large1),
            (null!, large2)
        ];

        Json.StatResult result = StatHandler.CollectStats(
            entries,
            [],
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        Assert.Equal(2, result.Extensions.Count);
        // .ydr total (1100) > .dat total (100), so .ydr comes first
        Assert.Equal(".ydr", result.Extensions[0].Extension);
        Assert.Equal(2, result.Extensions[0].Count);
        Assert.Equal(1100, result.Extensions[0].TotalSize);
        Assert.Equal(".dat", result.Extensions[1].Extension);
        Assert.Equal(1, result.Extensions[1].Count);
        Assert.Equal(100, result.Extensions[1].TotalSize);
    }

    [Fact]
    public void ExtensionStats_MinMaxAvg()
    {
        RpfBinaryFileEntry a = MakeBinary(
            "a.dat",
            fileSize: 100,
            uncompressedSize: 100
        );
        RpfBinaryFileEntry b = MakeBinary(
            "b.dat",
            fileSize: 300,
            uncompressedSize: 300
        );

        List<(RpfFile, RpfFileEntry)> entries = [
            (null!, a),
            (null!, b)
        ];

        Json.StatResult result = StatHandler.CollectStats(
            entries,
            [],
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        Json.ExtensionStat ext = result.Extensions[0];
        Assert.Equal(".dat", ext.Extension);
        Assert.Equal(100, ext.MinSize);
        Assert.Equal(300, ext.MaxSize);
        Assert.Equal(200, ext.AvgSize); // (100 + 300) / 2
    }

    [Fact]
    public void NoExtension_CategorizedAsNone()
    {
        RpfBinaryFileEntry entry = MakeBinary(
            "README",
            fileSize: 50,
            uncompressedSize: 50
        );

        List<(RpfFile, RpfFileEntry)> entries = [
            (null!, entry)
        ];

        Json.StatResult result = StatHandler.CollectStats(
            entries,
            [],
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        Assert.Equal("(none)", result.Extensions[0].Extension);
    }

    [Fact]
    public void ScanErrors_SetSuccessFalse()
    {
        List<string> errors = ["scan failed"];

        Json.StatResult result = StatHandler.CollectStats(
            [],
            errors,
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        Assert.False(result.Success);
        Assert.Equal(errors, result.ErrorMessages);
    }

    [Fact]
    public void ScanErrors_Empty_SetSuccessTrue()
    {
        Json.StatResult result = StatHandler.CollectStats(
            [],
            [],
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        Assert.True(result.Success);
        Assert.Empty(result.ErrorMessages);
    }

    [Fact]
    public void CaseInsensitiveExtensionGrouping()
    {
        RpfBinaryFileEntry upper = MakeBinary(
            "A.DAT",
            fileSize: 100,
            uncompressedSize: 100
        );
        RpfBinaryFileEntry lower = MakeBinary(
            "b.dat",
            fileSize: 200,
            uncompressedSize: 200
        );

        List<(RpfFile, RpfFileEntry)> entries = [
            (null!, upper),
            (null!, lower)
        ];

        Json.StatResult result = StatHandler.CollectStats(entries, [], MakeOptions(), TestContext.Current.CancellationToken);

        _ = Assert.Single(result.Extensions);
        Assert.Equal(".dat", result.Extensions[0].Extension);
        Assert.Equal(2, result.Extensions[0].Count);
        Assert.Equal(300, result.Extensions[0].TotalSize);
    }

    [Fact]
    public void CompressionRatio_RoundedToFourDecimals()
    {
        // 1 / 3 = 0.33333... -> should round to 0.3333
        RpfBinaryFileEntry entry = MakeBinary(
            "data.dat",
            fileSize: 1,
            uncompressedSize: 3
        );

        List<(RpfFile, RpfFileEntry)> entries = [
            (null!, entry)
        ];

        Json.StatResult result = StatHandler.CollectStats(
            entries,
            [],
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        Assert.Equal(0.3333, result.CompressionRatio);
    }

    [Fact]
    public void AvgSize_TruncatedByIntegerDivision()
    {
        // 3 files totalling 10 bytes -> avg = 10 / 3 = 3 (integer truncation, not 3.33)
        RpfBinaryFileEntry a = MakeBinary(
            "a.dat",
            fileSize: 1,
            uncompressedSize: 1
        );
        RpfBinaryFileEntry b = MakeBinary(
            "b.dat",
            fileSize: 4,
            uncompressedSize: 4
        );
        RpfBinaryFileEntry c = MakeBinary(
            "c.dat",
            fileSize: 5,
            uncompressedSize: 5
        );

        List<(RpfFile, RpfFileEntry)> entries = [
            (null!, a),
            (null!, b),
            (null!, c)
        ];

        Json.StatResult result = StatHandler.CollectStats(
            entries,
            [],
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        Assert.Equal(3, result.Extensions[0].AvgSize); // 10 / 3 = 3, not 4
    }

    [Fact]
    public void Cancelled_ThrowsOperationCanceledException()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        RpfBinaryFileEntry entry = MakeBinary(
            "data.dat",
            fileSize: 100,
            uncompressedSize: 100
        );

        List<(RpfFile, RpfFileEntry)> entries = [
            (null!, entry)
        ];

        _ = Assert.Throws<OperationCanceledException>(
            () => StatHandler.CollectStats(
                entries,
                [],
                MakeOptions(),
                cts.Token
            )
        );
    }
}

// PrintJsonStats

[Collection("ConsoleOutput")]
public sealed class PrintJsonStatsTests
{
    private static Json.StatResult MakeResult(
        bool success = true,
        int totalFiles = 5,
        long totalSize = 1024,
        int resourceCount = 3,
        int binaryCount = 2,
        long compressedSize = 800,
        long uncompressedSize = 1200,
        double compressionRatio = 0.6667) =>
        new()
        {
            Success = success,
            RpfFile = "/test.rpf",
            TotalFiles = totalFiles,
            TotalSize = totalSize,
            TotalSizeFormatted = SizeFormat.IEC.ToFormattedString(totalSize),
            ResourceCount = resourceCount,
            BinaryCount = binaryCount,
            CompressedSize = compressedSize,
            CompressedSizeFormatted = SizeFormat.IEC.ToFormattedString(compressedSize),
            UncompressedSize = uncompressedSize,
            UncompressedSizeFormatted = SizeFormat.IEC.ToFormattedString(uncompressedSize),
            CompressionRatio = compressionRatio,
            Extensions =
            [
                new Json.ExtensionStat
                {
                    Extension = ".dat",
                    Count = 2,
                    TotalSize = 500,
                    TotalSizeFormatted = "500 B",
                    AvgSize = 250,
                    AvgSizeFormatted = "250 B",
                    MinSize = 200,
                    MinSizeFormatted = "200 B",
                    MaxSize = 300,
                    MaxSizeFormatted = "300 B"
                }
            ],
            ErrorMessages = []
        };

    [Fact]
    public void PrintJsonStats_WritesValidJson()
    {
        TextWriter orig = Console.Out;
        try
        {
            StringWriter sw = new();
            Console.SetOut(sw);

            StatHandler.PrintJsonStats(MakeResult());

            Json.StatResult? parsed = JsonSerializer.Deserialize<Json.StatResult>(
                sw.ToString().Trim(), Output.JsonSerializerOptions
            );
            Assert.NotNull(parsed);
        }
        finally { Console.SetOut(orig); }
    }

    [Fact]
    public void PrintJsonStats_ContainsAllFields()
    {
        TextWriter orig = Console.Out;
        try
        {
            StringWriter sw = new();
            Console.SetOut(sw);

            StatHandler.PrintJsonStats(MakeResult());

            string json = sw.ToString();
            Assert.Contains("\"success\": true", json);
            Assert.Contains("\"rpfFile\": \"/test.rpf\"", json);
            Assert.Contains("\"totalFiles\": 5", json);
            Assert.Contains("\"totalSize\": 1024", json);
            Assert.Contains("\"totalSizeFormatted\":", json);
            Assert.Contains("\"resourceCount\": 3", json);
            Assert.Contains("\"binaryCount\": 2", json);
            Assert.Contains("\"compressedSize\": 800", json);
            Assert.Contains("\"compressedSizeFormatted\":", json);
            Assert.Contains("\"uncompressedSize\": 1200", json);
            Assert.Contains("\"uncompressedSizeFormatted\":", json);
            Assert.Contains($"\"compressionRatio\": {JsonSerializer.Serialize(0.6667)}", json);
            Assert.Contains("\"extensions\":", json);
        }
        finally { Console.SetOut(orig); }
    }

    [Fact]
    public void PrintJsonStats_ExtensionStatFields()
    {
        TextWriter orig = Console.Out;
        try
        {
            StringWriter sw = new();
            Console.SetOut(sw);

            StatHandler.PrintJsonStats(MakeResult());

            string json = sw.ToString();
            Assert.Contains("\"extension\": \".dat\"", json);
            Assert.Contains("\"count\": 2", json);
            Assert.Contains("\"totalSize\": 500", json);
            Assert.Contains("\"avgSize\": 250", json);
            Assert.Contains("\"minSize\": 200", json);
            Assert.Contains("\"maxSize\": 300", json);
        }
        finally { Console.SetOut(orig); }
    }

    [Fact]
    public void PrintJsonStats_ErrorResult_ShowsSuccessFalse()
    {
        TextWriter orig = Console.Out;
        try
        {
            StringWriter sw = new();
            Console.SetOut(sw);

            StatHandler.PrintJsonStats(MakeResult(success: false));

            Assert.Contains("\"success\": false", sw.ToString());
        }
        finally { Console.SetOut(orig); }
    }

    [Fact]
    public void PrintJsonStats_RoundTripsCorrectly()
    {
        TextWriter orig = Console.Out;
        try
        {
            StringWriter sw = new();
            Console.SetOut(sw);

            Json.StatResult input = MakeResult();
            StatHandler.PrintJsonStats(input);

            Json.StatResult? parsed = JsonSerializer.Deserialize<Json.StatResult>(
                sw.ToString().Trim(), Output.JsonSerializerOptions
            );
            Assert.NotNull(parsed);
            Assert.Equal(input.TotalFiles, parsed.TotalFiles);
            Assert.Equal(input.TotalSize, parsed.TotalSize);
            Assert.Equal(input.ResourceCount, parsed.ResourceCount);
            Assert.Equal(input.BinaryCount, parsed.BinaryCount);
            Assert.Equal(input.CompressedSize, parsed.CompressedSize);
            Assert.Equal(input.UncompressedSize, parsed.UncompressedSize);
            Assert.Equal(input.CompressionRatio, parsed.CompressionRatio);
            Assert.Equal(input.Extensions.Count, parsed.Extensions.Count);
        }
        finally { Console.SetOut(orig); }
    }
}

// PrintStats (text)

[Collection("ConsoleOutput")]
public sealed class PrintStatsTests
{
    private static StatOptions MakeOptions(SizeFormat fmt = SizeFormat.IEC) =>
        new()
        {
            RpfPath = "/test.rpf",
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = [],
            Verbose = false,
            Json = false,
            Recursive = false,
            SizeFormat = fmt
        };

    private static (string stdout, string stderr) Capture(Json.StatResult result, StatOptions? options = null)
    {
        options ??= MakeOptions();
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter sw = new();
            StringWriter se = new();
            Console.SetOut(sw);
            Console.SetError(se);
            StatHandler.PrintStats(result, options);
            return (sw.ToString(), se.ToString());
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    private static Json.StatResult MakeResult(IReadOnlyList<Json.ExtensionStat>? extensions = null) =>
        new()
        {
            Success = true,
            RpfFile = "/test.rpf",
            TotalFiles = 10,
            TotalSize = 2048,
            TotalSizeFormatted = "2.0 KiB",
            ResourceCount = 6,
            BinaryCount = 4,
            CompressedSize = 1500,
            CompressedSizeFormatted = "1.5 KiB",
            UncompressedSize = 3000,
            UncompressedSizeFormatted = "2.9 KiB",
            CompressionRatio = 0.5,
            Extensions = extensions ??
            [
                new Json.ExtensionStat
                {
                    Extension = ".ydr",
                    Count = 3,
                    TotalSize = 1500,
                    TotalSizeFormatted = "1.5 KiB",
                    AvgSize = 500,
                    AvgSizeFormatted = "500 B",
                    MinSize = 200,
                    MinSizeFormatted = "200 B",
                    MaxSize = 800,
                    MaxSizeFormatted = "800 B"
                }
            ],
            ErrorMessages = []
        };

    [Fact]
    public void PrintStats_TableHasHeaders()
    {
        (string stdout, _) = Capture(MakeResult());
        Assert.Contains("Extension", stdout);
        Assert.Contains("Count", stdout);
        Assert.Contains("Total", stdout);
        Assert.Contains("Avg", stdout);
        Assert.Contains("Min", stdout);
        Assert.Contains("Max", stdout);
    }

    [Fact]
    public void PrintStats_TableHasSeparator()
    {
        (string stdout, _) = Capture(MakeResult());
        Assert.Contains("---", stdout);
        Assert.Contains("+", stdout);
    }

    [Fact]
    public void PrintStats_TableHasExtensionRow()
    {
        (string stdout, _) = Capture(MakeResult());
        Assert.Contains(".ydr", stdout);
    }

    [Fact]
    public void PrintStats_StderrHasSummary()
    {
        (_, string stderr) = Capture(MakeResult());
        Assert.Contains("Total: 10 files", stderr);
        Assert.Contains("Types: 6 resource, 4 binary", stderr);
    }

    [Fact]
    public void PrintStats_StderrHasCompression()
    {
        (_, string stderr) = Capture(MakeResult());
        Assert.Contains("Compression:", stderr);
    }

    [Fact]
    public void PrintStats_NoCompression_WhenUncompressedIsZero()
    {
        Json.StatResult result = MakeResult() with { UncompressedSize = 0 };
        (_, string stderr) = Capture(result);
        Assert.DoesNotContain("Compression:", stderr);
    }

    [Fact]
    public void PrintStats_EmptyExtensions_PrintsHeaderOnly()
    {
        Json.StatResult result = MakeResult(extensions: []);
        (string stdout, _) = Capture(result);
        Assert.Contains("Extension", stdout);
        Assert.DoesNotContain(".ydr", stdout);
    }

    [Fact]
    public void PrintStats_SIFormat_UsesDecimalUnits()
    {
        Json.StatResult result = MakeResult() with
        {
            TotalSize = 2000,
            TotalSizeFormatted = SizeFormat.SI.ToFormattedString(2000)
        };
        StatOptions options = MakeOptions(SizeFormat.SI);

        (string stdout, string stderr) = Capture(result, options);

        // SI uses KB (1000-based) not KiB (1024-based); format is "0.##" so "2 KB" not "2.0 KB"
        Assert.Contains("2 KB", stderr);
        Assert.DoesNotContain("KiB", stdout);
    }
}

// Execute (integration)

[Collection("ConsoleOutput")]
public sealed class StatExecuteTests
{
    private static StatOptions MakeOptions(string rpfPath, bool json) =>
        new()
        {
            RpfPath = rpfPath,
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = [],
            Verbose = false,
            Json = json,
            Recursive = false,
            SizeFormat = SizeFormat.IEC
        };

    // Validation failures

    [Fact]
    public void Execute_MissingRpf_ReturnsOne()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stderr = new();
            Console.SetOut(new StringWriter());
            Console.SetError(stderr);

            int exitCode = StatHandler.Execute(
                MakeOptions("/nonexistent/test.rpf", json: false),
                TestContext.Current.CancellationToken
            );

            Assert.Equal(1, exitCode);
            Assert.Contains("Error:", stderr.ToString());
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void Execute_MissingRpf_Json_ReturnsErrorJson()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);
            Console.SetError(new StringWriter());

            int exitCode = StatHandler.Execute(
                MakeOptions("/nonexistent/test.rpf", json: true),
                TestContext.Current.CancellationToken
            );

            Assert.Equal(1, exitCode);
            string output = stdout.ToString();
            Assert.Contains("\"success\": false", output);
            Assert.Contains("RPF file not found", output);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void Execute_Json_ErrorContainsAllExpectedFields()
    {
        TextWriter origOut = Console.Out;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);

            _ = StatHandler.Execute(
                MakeOptions("/nonexistent/test.rpf", json: true),
                TestContext.Current.CancellationToken
            );

            string output = stdout.ToString();
            Assert.Contains("\"rpfFile\":", output);
            Assert.Contains("\"totalFiles\": 0", output);
            Assert.Contains("\"totalSize\": 0", output);
            Assert.Contains("\"totalSizeFormatted\":", output);
            Assert.Contains("\"resourceCount\": 0", output);
            Assert.Contains("\"binaryCount\": 0", output);
            Assert.Contains("\"compressedSize\": 0", output);
            Assert.Contains("\"compressedSizeFormatted\":", output);
            Assert.Contains("\"uncompressedSize\": 0", output);
            Assert.Contains("\"uncompressedSizeFormatted\":", output);
            Assert.Contains("\"compressionRatio\": 0", output);
            Assert.Contains("\"extensions\": []", output);
        }
        finally { Console.SetOut(origOut); }
    }

    [Fact]
    public void Execute_MissingExe_WithExistingRpf_ReturnsOne()
    {
        string dir = Path.Combine(Path.GetTempPath(), "cw_stat_" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(dir);
        string rpf = Path.Combine(dir, "test.rpf");
        File.WriteAllBytes(rpf, []);
        try
        {
            TextWriter origOut = Console.Out;
            TextWriter origErr = Console.Error;
            try
            {
                Console.SetOut(new StringWriter());
                StringWriter stderr = new();
                Console.SetError(stderr);

                int exitCode = StatHandler.Execute(
                    MakeOptions(rpf, json: false),
                    TestContext.Current.CancellationToken
                );

                Assert.Equal(1, exitCode);
                Assert.Contains("Error:", stderr.ToString());
            }
            finally
            {
                Console.SetOut(origOut);
                Console.SetError(origErr);
            }
        }
        finally { Directory.Delete(dir, true); }
    }
}
