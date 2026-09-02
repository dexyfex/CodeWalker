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

[Collection("ConsoleOutput")]
public sealed class ListHandlerTests
{
    private static ListOptions MakeOptions(
        string rpfPath = "/test/test.rpf",
        bool json = false,
        bool verbose = false,
        SizeFormat sizeFormat = SizeFormat.IEC) =>
        new()
        {
            RpfPath = rpfPath,
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = [],
            Verbose = verbose,
            Json = json,
            Recursive = false,
            SizeFormat = sizeFormat,
        };

    private static readonly char[] SplitChars = ['\r', '\n'];

    private static RpfBinaryFileEntry MakeBinary(string name, string path, uint fileSize) =>
        new()
        {
            Name = name,
            NameLower = name.ToLowerInvariant(),
            Path = path,
            FileSize = fileSize,
            FileUncompressedSize = fileSize,
        };

    private static RpfResourceFileEntry MakeResource(string name, string path, uint fileSize) =>
        new()
        {
            Name = name,
            NameLower = name.ToLowerInvariant(),
            Path = path,
            FileSize = fileSize,
        };

    private static RpfFile MakeRpf(uint grandTotalRpfCount = 1) =>
        new("test.rpf", "test.rpf", 0) { GrandTotalRpfCount = grandTotalRpfCount };

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

            int exitCode = ListHandler.Execute(MakeOptions("/nonexistent/test.rpf"), TestContext.Current.CancellationToken);

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

            int exitCode = ListHandler.Execute(MakeOptions("/nonexistent/test.rpf", json: true), TestContext.Current.CancellationToken);

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
    public void Execute_Json_ErrorContainsExpectedFields()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);
            Console.SetError(new StringWriter());

            _ = ListHandler.Execute(MakeOptions("/nonexistent/test.rpf", json: true), TestContext.Current.CancellationToken);

            string output = stdout.ToString();
            Assert.Contains("\"rpfFile\":", output);
            Assert.Contains("\"totalFiles\": 0", output);
            Assert.Contains("\"totalSize\": 0", output);
            Assert.Contains("\"totalSizeFormatted\": \"0 B\"", output);
            Assert.Contains("\"nestedRpfCount\": 0", output);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void Execute_MissingExe_WithExistingRpf_ReturnsOne()
    {
        string dir = Path.Combine(Path.GetTempPath(), "cw_list_" + Guid.NewGuid().ToString("N"));
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

                int exitCode = ListHandler.Execute(MakeOptions(rpf), TestContext.Current.CancellationToken);

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

    // CollectList

    [Fact]
    public void CollectList_EmptyEntries_ReturnsZeroTotals()
    {
        Json.ListResult result = ListHandler.CollectList(
            [],
            MakeRpf(),
            [],
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        Assert.True(result.Success);
        Assert.Equal(0, result.TotalFiles);
        Assert.Equal(0L, result.TotalSize);
        Assert.Equal("0 B", result.TotalSizeFormatted);
        Assert.Empty(result.Files);
        Assert.Empty(result.ErrorMessages);
    }

    [Fact]
    public void CollectList_BinaryEntries_SumsCorrectly()
    {
        RpfFile rpf = MakeRpf(grandTotalRpfCount: 3);
        RpfBinaryFileEntry e1 = MakeBinary("data.dat", "common\\data.dat", 1024);
        RpfBinaryFileEntry e2 = MakeBinary("info.bin", "common\\info.bin", 2048);

        List<(RpfFile rpf, RpfFileEntry entry)> entries = [(rpf, e1), (rpf, e2)];

        Json.ListResult result = ListHandler.CollectList(
            entries,
            rpf,
            [],
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        Assert.True(result.Success);
        Assert.Equal(2, result.TotalFiles);
        Assert.Equal(3072L, result.TotalSize);
        Assert.Equal(SizeFormat.IEC.ToFormattedString(3072), result.TotalSizeFormatted);
        Assert.Equal(3L, result.NestedRpfCount);
        Assert.Empty(result.ErrorMessages);
    }

    [Fact]
    public void CollectList_FileEntry_HasCorrectFields()
    {
        RpfFile rpf = MakeRpf();
        RpfBinaryFileEntry entry = MakeBinary("data.dat", "common\\data.dat", 512);

        Json.ListResult result = ListHandler.CollectList(
            [(rpf, entry)],
            rpf,
            [],
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        _ = Assert.Single(result.Files);
        Json.FileEntry file = result.Files[0];
        Assert.Equal("common\\data.dat", file.Path);
        Assert.Equal("data.dat", file.Name);
        Assert.Equal(512L, file.Size);
        Assert.Equal(SizeFormat.IEC.ToFormattedString(512), file.SizeFormatted);
        Assert.Equal("binary", file.Type);
        Assert.Equal(".dat", file.Extension);
    }

    [Fact]
    public void CollectList_ResourceEntry_TypeIsResource()
    {
        RpfFile rpf = MakeRpf();
        RpfResourceFileEntry entry = MakeResource("model.ydr", "x64\\model.ydr", 4096);

        Json.ListResult result = ListHandler.CollectList(
            [(rpf, entry)],
            rpf,
            [],
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        _ = Assert.Single(result.Files);
        Assert.Equal("resource", result.Files[0].Type);
        Assert.Equal(".ydr", result.Files[0].Extension);
    }

    [Fact]
    public void CollectList_SIFormat_UsesCorrectFormatting()
    {
        RpfFile rpf = MakeRpf();
        RpfBinaryFileEntry entry = MakeBinary("data.dat", "common\\data.dat", 2000);

        Json.ListResult result = ListHandler.CollectList(
            [(rpf, entry)],
            rpf,
            [],
            MakeOptions(sizeFormat: SizeFormat.SI),
            TestContext.Current.CancellationToken
        );

        Assert.Equal(SizeFormat.SI.ToFormattedString(2000), result.TotalSizeFormatted);
        Assert.Equal(SizeFormat.SI.ToFormattedString(2000), result.Files[0].SizeFormatted);
    }

    [Fact]
    public void CollectList_WithScanErrors_SetsSuccessFalse()
    {
        RpfFile rpf = MakeRpf();
        List<string> scanErrors = ["scan error 1", "scan error 2"];

        Json.ListResult result = ListHandler.CollectList(
            [],
            rpf,
            scanErrors,
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        Assert.False(result.Success);
        Assert.Equal(2, result.ErrorMessages.Count);
        Assert.Equal("scan error 1", result.ErrorMessages[0]);
        Assert.Equal("scan error 2", result.ErrorMessages[1]);
    }

    [Fact]
    public void CollectList_NoExtension_ReturnsEmptyExtension()
    {
        RpfFile rpf = MakeRpf();
        RpfBinaryFileEntry entry = MakeBinary("README", "common\\README", 100);

        Json.ListResult result = ListHandler.CollectList(
            [(rpf, entry)],
            rpf,
            [],
            MakeOptions(),
            TestContext.Current.CancellationToken
        );

        Assert.Equal("", result.Files[0].Extension);
    }

    [Fact]
    public void CollectList_Cancelled_ThrowsOperationCanceledException()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        RpfFile rpf = MakeRpf();
        RpfBinaryFileEntry entry = MakeBinary("data.dat", "common\\data.dat", 100);

        _ = Assert.Throws<OperationCanceledException>(
            () => ListHandler.CollectList(
                [(rpf, entry)],
                rpf,
                [],
                MakeOptions(),
                cts.Token
            )
        );
    }

    // ErrorResult

    [Fact]
    public void ErrorResult_HasExpectedDefaults()
    {
        Json.ListResult result = ListHandler.ErrorResult([], MakeOptions("/some/path.rpf"));

        Assert.False(result.Success);
        Assert.Equal("/some/path.rpf", result.RpfFile);
        Assert.Equal(0, result.TotalFiles);
        Assert.Equal(0L, result.TotalSize);
        Assert.Equal("0 B", result.TotalSizeFormatted);
        Assert.Equal(0L, result.NestedRpfCount);
        Assert.Empty(result.Files);
        Assert.Empty(result.ErrorMessages);
    }

    [Fact]
    public void ErrorResult_PreservesErrorMessages()
    {
        string[] errors = ["err1", "err2"];

        Json.ListResult result = ListHandler.ErrorResult(errors, MakeOptions());

        Assert.Equal(2, result.ErrorMessages.Count);
        Assert.Equal("err1", result.ErrorMessages[0]);
        Assert.Equal("err2", result.ErrorMessages[1]);
    }

    // PrintList (text output)

    [Fact]
    public void PrintList_NonVerbose_PrintsPathsOnly()
    {
        Json.ListResult result = new()
        {
            Success = true,
            RpfFile = "test.rpf",
            TotalFiles = 2,
            TotalSize = 3072,
            TotalSizeFormatted = "3 KiB",
            NestedRpfCount = 1,
            Files =
            [
                new Json.FileEntry { Path = "common\\data.dat", Name = "data.dat", Size = 1024, SizeFormatted = "1 KiB", Type = "binary", Extension = ".dat" },
                new Json.FileEntry { Path = "common\\info.bin", Name = "info.bin", Size = 2048, SizeFormatted = "2 KiB", Type = "binary", Extension = ".bin" },
            ],
            ErrorMessages = [],
        };

        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);
            Console.SetError(new StringWriter());

            ListHandler.PrintList(result, MakeOptions(), TestContext.Current.CancellationToken);

            string output = stdout.ToString();
            string[] lines = output.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, lines.Length);
            Assert.Equal("common\\data.dat", lines[0]);
            Assert.Equal("common\\info.bin", lines[1]);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void PrintList_Verbose_PrintsSizeAndPath()
    {
        Json.ListResult result = new()
        {
            Success = true,
            RpfFile = "test.rpf",
            TotalFiles = 1,
            TotalSize = 1024,
            TotalSizeFormatted = "1 KiB",
            NestedRpfCount = 1,
            Files =
            [
                new Json.FileEntry { Path = "common\\data.dat", Name = "data.dat", Size = 1024, SizeFormatted = "1 KiB", Type = "binary", Extension = ".dat" },
            ],
            ErrorMessages = [],
        };

        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);
            Console.SetError(new StringWriter());

            ListHandler.PrintList(result, MakeOptions(verbose: true), TestContext.Current.CancellationToken);

            string output = stdout.ToString();
            Assert.Contains("1 KiB", output);
            Assert.Contains("common\\data.dat", output);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void PrintList_Verbose_SizeIsPaddedTo12Chars()
    {
        Json.ListResult result = new()
        {
            Success = true,
            RpfFile = "test.rpf",
            TotalFiles = 1,
            TotalSize = 100,
            TotalSizeFormatted = "100 B",
            NestedRpfCount = 1,
            Files =
            [
                new Json.FileEntry { Path = "a.dat", Name = "a.dat", Size = 100, SizeFormatted = "100 B", Type = "binary", Extension = ".dat" },
            ],
            ErrorMessages = [],
        };

        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);
            Console.SetError(new StringWriter());

            ListHandler.PrintList(result, MakeOptions(verbose: true), TestContext.Current.CancellationToken);

            string[] lines = stdout.ToString().Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
            _ = Assert.Single(lines);
            // "100 B" (5 chars) padded left to 12 = 7 spaces + "100 B" + "  " + path
            Assert.Equal("       100 B  a.dat", lines[0]);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void PrintList_WritesSummaryToStderr()
    {
        Json.ListResult result = new()
        {
            Success = true,
            RpfFile = "test.rpf",
            TotalFiles = 5,
            TotalSize = 10240,
            TotalSizeFormatted = "10 KiB",
            NestedRpfCount = 1,
            Files = [],
            ErrorMessages = [],
        };

        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            Console.SetOut(new StringWriter());
            StringWriter stderr = new();
            Console.SetError(stderr);

            ListHandler.PrintList(result, MakeOptions(), TestContext.Current.CancellationToken);

            string errOutput = stderr.ToString();
            Assert.Contains("Total: 5 files, 10 KiB", errOutput);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void PrintList_Cancelled_ThrowsOperationCanceledException()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Json.ListResult result = new()
        {
            Success = true,
            RpfFile = "test.rpf",
            TotalFiles = 1,
            TotalSize = 100,
            TotalSizeFormatted = "100 B",
            NestedRpfCount = 1,
            Files =
            [
                new Json.FileEntry { Path = "a.dat", Name = "a.dat", Size = 100, SizeFormatted = "100 B", Type = "binary", Extension = ".dat" },
            ],
            ErrorMessages = [],
        };

        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            Console.SetOut(new StringWriter());
            Console.SetError(new StringWriter());

            _ = Assert.Throws<OperationCanceledException>(
                () => ListHandler.PrintList(result, MakeOptions(), cts.Token)
            );
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    // PrintJsonList

    [Fact]
    public void PrintJsonList_SerializesToStdout()
    {
        Json.ListResult result = new()
        {
            Success = true,
            RpfFile = "test.rpf",
            TotalFiles = 1,
            TotalSize = 512,
            TotalSizeFormatted = "512 B",
            NestedRpfCount = 1,
            Files =
            [
                new Json.FileEntry { Path = "common\\data.dat", Name = "data.dat", Size = 512, SizeFormatted = "512 B", Type = "binary", Extension = ".dat" },
            ],
            ErrorMessages = [],
        };

        TextWriter origOut = Console.Out;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);

            ListHandler.PrintJsonList(result);

            string output = stdout.ToString();
            Assert.Contains("\"success\": true", output);
            Assert.Contains("\"rpfFile\": \"test.rpf\"", output);
            Assert.Contains("\"totalFiles\": 1", output);
            Assert.Contains("\"totalSize\": 512", output);
            Assert.Contains("\"totalSizeFormatted\": \"512 B\"", output);
            Assert.Contains("\"nestedRpfCount\": 1", output);
            Assert.Contains("\"path\": \"common\\\\data.dat\"", output);
            Assert.Contains("\"name\": \"data.dat\"", output);
            Assert.Contains("\"size\": 512", output);
            Assert.Contains("\"type\": \"binary\"", output);
            Assert.Contains("\"extension\": \".dat\"", output);
        }
        finally { Console.SetOut(origOut); }
    }

    [Fact]
    public void PrintJsonList_IsValidJson()
    {
        Json.ListResult result = new()
        {
            Success = true,
            RpfFile = "test.rpf",
            TotalFiles = 0,
            TotalSize = 0,
            TotalSizeFormatted = "0 B",
            NestedRpfCount = 0,
            Files = [],
            ErrorMessages = [],
        };

        TextWriter origOut = Console.Out;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);

            ListHandler.PrintJsonList(result);

            string output = stdout.ToString().Trim();
            JsonDocument doc = JsonDocument.Parse(output);
            Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
        }
        finally { Console.SetOut(origOut); }
    }

    [Fact]
    public void PrintJsonList_OmitsNullStatus()
    {
        Json.ListResult result = new()
        {
            Success = true,
            RpfFile = "test.rpf",
            TotalFiles = 1,
            TotalSize = 100,
            TotalSizeFormatted = "100 B",
            NestedRpfCount = 0,
            Files =
            [
                new Json.FileEntry { Path = "a.dat", Name = "a.dat", Size = 100, SizeFormatted = "100 B", Type = "binary", Extension = ".dat" },
            ],
            ErrorMessages = [],
        };

        TextWriter origOut = Console.Out;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);

            ListHandler.PrintJsonList(result);

            string output = stdout.ToString();
            Assert.DoesNotContain("\"status\"", output);
        }
        finally { Console.SetOut(origOut); }
    }

    [Fact]
    public void PrintJsonList_IncludesErrorMessages()
    {
        Json.ListResult result = new()
        {
            Success = false,
            RpfFile = "test.rpf",
            TotalFiles = 0,
            TotalSize = 0,
            TotalSizeFormatted = "0 B",
            NestedRpfCount = 0,
            Files = [],
            ErrorMessages = ["something broke"],
        };

        TextWriter origOut = Console.Out;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);

            ListHandler.PrintJsonList(result);

            string output = stdout.ToString();
            Assert.Contains("\"errorMessages\"", output);
            Assert.Contains("something broke", output);
        }
        finally { Console.SetOut(origOut); }
    }
}
