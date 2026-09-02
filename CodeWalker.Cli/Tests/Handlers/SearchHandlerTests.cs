using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using CodeWalker.Cli.Handlers;
using CodeWalker.Cli.Helpers;
using CodeWalker.GameFiles;

using Xunit;

namespace CodeWalker.Cli.Tests.Handlers;

// ErrorResult

public sealed class SearchErrorResultTests
{
    private static SearchOptions MakeOptions(string rpfPath = "/test.rpf", string pattern = "*.ydr") =>
        new()
        {
            RpfPath = rpfPath,
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = [],
            Verbose = false,
            Json = false,
            Recursive = false,
            SizeFormat = SizeFormat.IEC,
            Pattern = pattern,
        };

    [Fact]
    public void ErrorResult_SetsSuccessFalse()
    {
        Json.SearchResult result = SearchHandler.ErrorResult([], MakeOptions(pattern: "*.ydr"));
        Assert.False(result.Success);
    }

    [Fact]
    public void ErrorResult_PreservesRpfFile()
    {
        Json.SearchResult result = SearchHandler.ErrorResult([], MakeOptions("/my/test.rpf", pattern: "test"));
        Assert.Equal("/my/test.rpf", result.RpfFile);
    }

    [Fact]
    public void ErrorResult_PreservesPattern()
    {
        Json.SearchResult result = SearchHandler.ErrorResult([], MakeOptions(pattern: "adder"));
        Assert.Equal("adder", result.Pattern);
    }

    [Fact]
    public void ErrorResult_SetsMatchCountZero()
    {
        Json.SearchResult result = SearchHandler.ErrorResult([], MakeOptions(pattern: "*.ydr"));
        Assert.Equal(0, result.MatchCount);
    }

    [Fact]
    public void ErrorResult_SetsEmptyMatches()
    {
        Json.SearchResult result = SearchHandler.ErrorResult([], MakeOptions(pattern: "*.ydr"));
        Assert.Empty(result.Matches);
    }

    [Fact]
    public void ErrorResult_PreservesErrorMessages()
    {
        string[] msgs = ["err1", "err2"];
        Json.SearchResult result = SearchHandler.ErrorResult(msgs, MakeOptions(pattern: "*.ydr"));
        Assert.Equal(msgs, result.ErrorMessages);
    }
}

// CollectSearch

public sealed class SearchCollectSearchTests
{
    private static SearchOptions MakeOptions(
        string rpfPath = "/test.rpf",
        bool recursive = false,
        bool verbose = false,
        string[]? filters = null,
        string pattern = "") =>
        new()
        {
            RpfPath = rpfPath,
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = filters ?? [],
            Verbose = verbose,
            Json = false,
            Recursive = recursive,
            SizeFormat = SizeFormat.IEC,
            Pattern = pattern,
        };

    private static RpfFile MakeRpf() =>
        new("test.rpf", "test.rpf", 0);

    private static RpfBinaryFileEntry MakeBinary(string name, string path, uint fileSize = 1024) =>
        new()
        {
            Name = name,
            NameLower = name.ToLowerInvariant(),
            Path = path,
            FileSize = fileSize,
            FileUncompressedSize = fileSize,
        };

    private static RpfResourceFileEntry MakeResource(string name, string path, uint fileSize = 2048) =>
        new()
        {
            Name = name,
            NameLower = name.ToLowerInvariant(),
            Path = path,
            FileSize = fileSize,
        };

    [Fact]
    public void CollectSearch_EmptyEntries_ReturnsZeroMatches()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries = [];

        Json.SearchResult result = SearchHandler.CollectSearch(rpf, [], MakeOptions(pattern: "adder"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.Equal(0, result.MatchCount);
        Assert.Empty(result.Matches);
    }

    [Fact]
    public void CollectSearch_NullEntries_ReturnsZeroMatches()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries = null;

        Json.SearchResult result = SearchHandler.CollectSearch(rpf, [], MakeOptions(pattern: "adder"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.Equal(0, result.MatchCount);
    }

    [Fact]
    public void CollectSearch_SubstringMatch_FindsEntries()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries =
        [
            MakeBinary("adder.ydr", "vehicles/adder.ydr"),
            MakeBinary("zentorno.ydr", "vehicles/zentorno.ydr"),
            MakeBinary("adder.ytd", "vehicles/adder.ytd"),
        ];

        Json.SearchResult result = SearchHandler.CollectSearch(rpf, [], MakeOptions(pattern: "adder"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.Equal(2, result.MatchCount);
    }

    [Fact]
    public void CollectSearch_ExtensionSubstring_FindsEntries()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries =
        [
            MakeBinary("adder.ydr", "vehicles/adder.ydr"),
            MakeBinary("adder.ytd", "vehicles/adder.ytd"),
            MakeBinary("zentorno.ydr", "vehicles/zentorno.ydr"),
        ];

        Json.SearchResult result = SearchHandler.CollectSearch(rpf, [], MakeOptions(pattern: ".ydr"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.Equal(2, result.MatchCount);
    }

    [Fact]
    public void CollectSearch_MatchPopulatesAllFields()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries =
        [
            MakeBinary("adder.ydr", "vehicles/adder.ydr", fileSize: 4096),
        ];

        Json.SearchResult result = SearchHandler.CollectSearch(rpf, [], MakeOptions(pattern: "adder"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, result.MatchCount);
        Json.SearchMatch match = result.Matches[0];
        Assert.Equal("vehicles/adder.ydr", match.Path);
        Assert.Equal("adder.ydr", match.Name);
        Assert.Equal(4096, match.Size);
        Assert.Equal("binary", match.Type);
        Assert.Equal(".ydr", match.Extension);
    }

    [Fact]
    public void CollectSearch_ResourceEntry_SetsTypeResource()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries =
        [
            MakeResource("adder.ydr", "vehicles/adder.ydr"),
        ];

        Json.SearchResult result = SearchHandler.CollectSearch(rpf, [], MakeOptions(pattern: "adder"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, result.MatchCount);
        Assert.Equal("resource", result.Matches[0].Type);
    }

    [Fact]
    public void CollectSearch_DirectoryEntry_SetsTypeDirectory()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries =
        [
            new RpfDirectoryEntry
            {
                Name = "vehicles",
                NameLower = "vehicles",
                Path = "vehicles",
            },
        ];

        Json.SearchResult result = SearchHandler.CollectSearch(rpf, [], MakeOptions(pattern: "vehicles"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, result.MatchCount);
        Assert.Equal("directory", result.Matches[0].Type);
        Assert.Equal(0, result.Matches[0].Size);
        Assert.Equal("", result.Matches[0].Extension);
    }

    [Fact]
    public void CollectSearch_NoMatch_ReturnsEmpty()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries =
        [
            MakeBinary("adder.ydr", "vehicles/adder.ydr"),
        ];

        Json.SearchResult result = SearchHandler.CollectSearch(rpf, [], MakeOptions(pattern: "weapons"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.Equal(0, result.MatchCount);
        Assert.Empty(result.Matches);
    }

    [Fact]
    public void CollectSearch_ScanErrors_SetsSuccessFalse()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries =
        [
            MakeBinary("adder.ydr", "vehicles/adder.ydr"),
        ];
        List<string> scanErrors = ["scan error 1"];

        Json.SearchResult result = SearchHandler.CollectSearch(rpf, scanErrors, MakeOptions(pattern: "adder"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Equal(1, result.MatchCount);
        Assert.Contains("scan error 1", result.ErrorMessages);
    }

    [Fact]
    public void CollectSearch_WithFilter_NarrowsResults()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries =
        [
            MakeBinary("adder.ydr", "vehicles/adder.ydr"),
            MakeBinary("adder.ytd", "vehicles/adder.ytd"),
        ];

        Json.SearchResult result = SearchHandler.CollectSearch(rpf, [], MakeOptions(filters: Filter.Normalize(["*.ydr"]), pattern: "adder"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.Equal(1, result.MatchCount);
        Assert.Equal("adder.ydr", result.Matches[0].Name);
    }

    [Fact]
    public void CollectSearch_WithFilter_EmptyFilters_MatchesAll()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries =
        [
            MakeBinary("adder.ydr", "vehicles/adder.ydr"),
            MakeBinary("adder.ytd", "vehicles/adder.ytd"),
        ];

        Json.SearchResult result = SearchHandler.CollectSearch(rpf, [], MakeOptions(pattern: "adder"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.Equal(2, result.MatchCount);
    }

    [Fact]
    public void CollectSearch_BackslashPattern_NormalizesAndMatches()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries =
        [
            MakeBinary("adder.ydr", "vehicles/adder.ydr"),
            MakeBinary("zentorno.ydr", "vehicles/zentorno.ydr"),
        ];

        Json.SearchResult result = SearchHandler.CollectSearch(rpf, [], MakeOptions(pattern: "vehicles\\adder"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, result.MatchCount);
        Assert.Equal("vehicles/adder.ydr", result.Matches[0].Path);
    }

    [Fact]
    public void CollectSearch_CaseInsensitive_MatchesUppercasePattern()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries =
        [
            MakeBinary("adder.ydr", "vehicles/adder.ydr"),
            MakeBinary("zentorno.ydr", "vehicles/zentorno.ydr"),
        ];

        Json.SearchResult result = SearchHandler.CollectSearch(rpf, [], MakeOptions(pattern: "ADDER"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, result.MatchCount);
        Assert.Equal("adder.ydr", result.Matches[0].Name);
    }

    [Fact]
    public void CollectSearch_NullPathEntry_IsSkipped()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries =
        [
            new RpfBinaryFileEntry
            {
                Name = "adder.ydr",
                NameLower = "adder.ydr",
                Path = null,
                FileSize = 1024,
                FileUncompressedSize = 1024,
            },
        ];

        Json.SearchResult result = SearchHandler.CollectSearch(rpf, [], MakeOptions(pattern: "adder"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(0, result.MatchCount);
        Assert.Empty(result.Matches);
    }

    [Fact]
    public void CollectSearch_SetsRpfFileAndPattern()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries = [];

        Json.SearchResult result = SearchHandler.CollectSearch(rpf, [], MakeOptions("/my/archive.rpf", pattern: "test"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("/my/archive.rpf", result.RpfFile);
        Assert.Equal("test", result.Pattern);
    }
}

// CollectAllEntries

public sealed class SearchCollectAllEntriesTests
{
    private static RpfFile MakeRpf() =>
        new("test.rpf", "test.rpf", 0);

    private static RpfBinaryFileEntry MakeBinary(string name, string path) =>
        new()
        {
            Name = name,
            NameLower = name.ToLowerInvariant(),
            Path = path,
            FileSize = 1024,
            FileUncompressedSize = 1024,
        };

    [Fact]
    public void CollectAllEntries_NullEntries_CollectsNothing()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries = null;
        List<RpfEntry> entries = [];

        SearchHandler.CollectAllEntries(rpf, recursive: false, entries);

        Assert.Empty(entries);
    }

    [Fact]
    public void CollectAllEntries_FlatEntries_CollectsAll()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries = [MakeBinary("a.ydr", "a.ydr"), MakeBinary("b.ydr", "b.ydr")];
        List<RpfEntry> entries = [];

        SearchHandler.CollectAllEntries(rpf, recursive: false, entries);

        Assert.Equal(2, entries.Count);
    }

    [Fact]
    public void CollectAllEntries_NotRecursive_SkipsChildren()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries = [MakeBinary("a.ydr", "a.ydr")];

        RpfFile child = new("child.rpf", "child.rpf", 0)
        {
            AllEntries = [MakeBinary("b.ydr", "child.rpf/b.ydr")],
        };
        rpf.Children = [child];

        List<RpfEntry> entries = [];
        SearchHandler.CollectAllEntries(rpf, recursive: false, entries);

        _ = Assert.Single(entries);
        Assert.Equal("a.ydr", entries[0].Name);
    }

    [Fact]
    public void CollectAllEntries_Recursive_IncludesChildren()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries = [MakeBinary("a.ydr", "a.ydr")];

        RpfFile child = new("child.rpf", "child.rpf", 0)
        {
            AllEntries = [MakeBinary("b.ydr", "child.rpf/b.ydr")],
        };
        rpf.Children = [child];

        List<RpfEntry> entries = [];
        SearchHandler.CollectAllEntries(rpf, recursive: true, entries);

        Assert.Equal(2, entries.Count);
    }

    [Fact]
    public void CollectAllEntries_Recursive_NestedChildren()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries = [MakeBinary("a.ydr", "a.ydr")];

        RpfFile grandchild = new("grandchild.rpf", "grandchild.rpf", 0)
        {
            AllEntries = [MakeBinary("c.ydr", "grandchild.rpf/c.ydr")],
        };

        RpfFile child = new("child.rpf", "child.rpf", 0)
        {
            AllEntries = [MakeBinary("b.ydr", "child.rpf/b.ydr")],
            Children = [grandchild],
        };
        rpf.Children = [child];

        List<RpfEntry> entries = [];
        SearchHandler.CollectAllEntries(rpf, recursive: true, entries);

        Assert.Equal(3, entries.Count);
    }

    [Fact]
    public void CollectAllEntries_NullChildren_DoesNotThrow()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries = [MakeBinary("a.ydr", "a.ydr")];
        rpf.Children = null;

        List<RpfEntry> entries = [];
        SearchHandler.CollectAllEntries(rpf, recursive: true, entries);

        _ = Assert.Single(entries);
    }
}

// Cancellation

public sealed class SearchCancellationTests
{
    private static RpfFile MakeRpf() =>
        new("test.rpf", "test.rpf", 0);

    private static RpfBinaryFileEntry MakeBinary(string name, string path) =>
        new()
        {
            Name = name,
            NameLower = name.ToLowerInvariant(),
            Path = path,
            FileSize = 1024,
            FileUncompressedSize = 1024,
        };

    private static SearchOptions MakeOptions(string pattern = "*") =>
        new()
        {
            RpfPath = "/test.rpf",
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = [],
            Verbose = false,
            Json = false,
            Recursive = false,
            SizeFormat = SizeFormat.IEC,
            Pattern = pattern,
        };

    [Fact]
    public void CollectSearch_Cancelled_ThrowsOperationCanceledException()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries =
        [
            MakeBinary("a.ydr", "a.ydr"),
            MakeBinary("b.ydr", "b.ydr"),
        ];

        using CancellationTokenSource cts = new();
        cts.Cancel();

        _ = Assert.Throws<OperationCanceledException>(
            () => SearchHandler.CollectSearch(rpf, [], MakeOptions("*"), cancellationToken: cts.Token)
        );
    }
}

// PrintSearch / PrintJsonSearch

[Collection("ConsoleOutput")]
public sealed class SearchPrintTests
{
    private static readonly char[] SplitChars = ['\r', '\n'];

    private static SearchOptions MakeOptions(bool verbose = false, SizeFormat sizeFormat = SizeFormat.IEC) =>
        new()
        {
            RpfPath = "/test.rpf",
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = [],
            Verbose = verbose,
            Json = false,
            Recursive = false,
            SizeFormat = sizeFormat,
            Pattern = "",
        };

    private static Json.SearchResult MakeResult(
        List<Json.SearchMatch>? matches = null,
        string pattern = "*.ydr",
        int? matchCount = null,
        IReadOnlyList<string>? rpfFiles = null) =>
        new()
        {
            Success = true,
            RpfFile = "/test.rpf",
            RpfFiles = rpfFiles ?? ["/test.rpf"],
            Pattern = pattern,
            MatchCount = matchCount ?? matches?.Count ?? 0,
            Matches = matches ?? [],
            ErrorMessages = [],
        };

    private static Json.SearchMatch MakeMatch(
        string path = "vehicles/adder.ydr",
        string name = "adder.ydr",
        long size = 4096,
        string type = "binary",
        string extension = ".ydr",
        string archive = "/test.rpf") =>
        new()
        {
            Archive = archive,
            Path = path,
            Name = name,
            Size = size,
            Type = type,
            Extension = extension,
        };

    // PrintSearch (text)

    [Fact]
    public void PrintSearch_NonVerbose_PrintsPathsOnly()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            StringWriter stderr = new();
            Console.SetOut(stdout);
            Console.SetError(stderr);

            Json.SearchResult result = MakeResult([MakeMatch(), MakeMatch("weapons/pistol.ydr", "pistol.ydr")]);
            SearchHandler.PrintSearch(result, MakeOptions(verbose: false));

            string output = stdout.ToString();
            string[] lines = output.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, lines.Length);
            Assert.Equal("vehicles/adder.ydr", lines[0]);
            Assert.Equal("weapons/pistol.ydr", lines[1]);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void PrintSearch_Verbose_PrintsSizeAndPath()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            StringWriter stderr = new();
            Console.SetOut(stdout);
            Console.SetError(stderr);

            Json.SearchResult result = MakeResult([MakeMatch(size: 1024)]);
            SearchHandler.PrintSearch(result, MakeOptions(verbose: true));

            string output = stdout.ToString();
            Assert.Contains("1 KiB", output);
            Assert.Contains("vehicles/adder.ydr", output);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void PrintSearch_PrintsSummaryToStderr()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            StringWriter stderr = new();
            Console.SetOut(stdout);
            Console.SetError(stderr);

            Json.SearchResult result = MakeResult(
                [MakeMatch()],
                pattern: "adder");
            SearchHandler.PrintSearch(result, MakeOptions());

            string errOutput = stderr.ToString();
            Assert.Contains("Found 1 match for", errOutput);
            Assert.Contains("'adder'", errOutput);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void PrintSearch_EmptyResults_PrintsSummaryOnly()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            StringWriter stderr = new();
            Console.SetOut(stdout);
            Console.SetError(stderr);

            Json.SearchResult result = MakeResult(pattern: "nothing");
            SearchHandler.PrintSearch(result, MakeOptions());

            Assert.Equal("", stdout.ToString());
            Assert.Contains("Found 0 matches", stderr.ToString());
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void PrintSearch_Verbose_SingleArchive_NoArchiveHeaders()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            StringWriter stderr = new();
            Console.SetOut(stdout);
            Console.SetError(stderr);

            Json.SearchResult result = MakeResult([MakeMatch(size: 2048)]);
            SearchHandler.PrintSearch(result, MakeOptions(verbose: true));

            string errOutput = stderr.ToString();
            Assert.DoesNotContain("==", errOutput);

            string stdoutOutput = stdout.ToString();
            Assert.Contains("2 KiB", stdoutOutput);
            Assert.Contains("vehicles/adder.ydr", stdoutOutput);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void PrintSearch_Verbose_SIFormat_PrintsSIUnits()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            StringWriter stderr = new();
            Console.SetOut(stdout);
            Console.SetError(stderr);

            Json.SearchResult result = MakeResult([MakeMatch(size: 1000)]);
            SearchHandler.PrintSearch(result, MakeOptions(verbose: true, sizeFormat: SizeFormat.SI));

            string output = stdout.ToString();
            Assert.Contains("1 KB", output);
            Assert.DoesNotContain("KiB", output);
            Assert.Contains("vehicles/adder.ydr", output);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    // PrintJsonSearch

    [Fact]
    public void PrintJsonSearch_OutputsValidJson()
    {
        TextWriter origOut = Console.Out;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);

            Json.SearchResult result = MakeResult(
                [MakeMatch()],
                pattern: "adder");
            SearchHandler.PrintJsonSearch(result);

            string output = stdout.ToString();
            Assert.Contains("\"success\": true", output);
            Assert.Contains("\"pattern\": \"adder\"", output);
            Assert.Contains("\"matchCount\": 1", output);
            Assert.Contains("\"path\": \"vehicles/adder.ydr\"", output);
        }
        finally
        {
            Console.SetOut(origOut);
        }
    }

    [Fact]
    public void PrintJsonSearch_EmptyMatches_OutputsEmptyArray()
    {
        TextWriter origOut = Console.Out;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);

            Json.SearchResult result = MakeResult();
            SearchHandler.PrintJsonSearch(result);

            string output = stdout.ToString();
            Assert.Contains("\"matches\": []", output);
            Assert.Contains("\"matchCount\": 0", output);
        }
        finally
        {
            Console.SetOut(origOut);
        }
    }
}

// Execute (validation failures)

[Collection("ConsoleOutput")]
public sealed class SearchHandlerExecuteTests
{
    private static SearchOptions MakeOptions(string rpfPath, bool json, string pattern = "", string? dirPath = null) =>
        new()
        {
            RpfPath = rpfPath,
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = [],
            Verbose = false,
            Json = json,
            Recursive = false,
            SizeFormat = SizeFormat.IEC,
            Pattern = pattern,
            DirPath = dirPath,
        };

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

            int exitCode = SearchHandler.Execute(MakeOptions("/nonexistent/test.rpf", json: false, pattern: "*.ydr"), cancellationToken: TestContext.Current.CancellationToken);

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

            int exitCode = SearchHandler.Execute(MakeOptions("/nonexistent/test.rpf", json: true, pattern: "adder"), cancellationToken: TestContext.Current.CancellationToken);

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
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);

            _ = SearchHandler.Execute(MakeOptions("/nonexistent/test.rpf", json: true, pattern: "test*"), cancellationToken: TestContext.Current.CancellationToken);

            string output = stdout.ToString();
            Assert.Contains("\"rpfFile\":", output);
            Assert.Contains("\"pattern\":", output);
            Assert.Contains("\"matchCount\": 0", output);
        }
        finally { Console.SetOut(origOut); }
    }

    [Fact]
    public void Execute_WithDirPath_DelegatesToExecuteDirectory()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stderr = new();
            Console.SetOut(new StringWriter());
            Console.SetError(stderr);

            int exitCode = SearchHandler.Execute(
                MakeOptions("/unused.rpf", json: false, pattern: "*.ydr", dirPath: "/nonexistent_dir_xyz_12345"),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(1, exitCode);
            Assert.Contains("Directory not found", stderr.ToString());
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void Execute_WithDirPath_Json_DelegatesToExecuteDirectory()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);
            Console.SetError(new StringWriter());

            int exitCode = SearchHandler.Execute(
                MakeOptions("/unused.rpf", json: true, pattern: "adder", dirPath: "/nonexistent_dir_xyz_12345"),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(1, exitCode);
            string output = stdout.ToString();
            Assert.Contains("\"success\": false", output);
            Assert.Contains("Directory not found", output);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }
}

// CollectSearch Archive field

public sealed class SearchCollectSearchArchiveTests
{
    private static SearchOptions MakeOptions(string rpfPath = "/test.rpf", string pattern = "") =>
        new()
        {
            RpfPath = rpfPath,
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = [],
            Verbose = false,
            Json = false,
            Recursive = false,
            SizeFormat = SizeFormat.IEC,
            Pattern = pattern,
        };

    private static RpfFile MakeRpf() =>
        new("test.rpf", "test.rpf", 0);

    private static RpfBinaryFileEntry MakeBinary(string name, string path, uint fileSize = 1024) =>
        new()
        {
            Name = name,
            NameLower = name.ToLowerInvariant(),
            Path = path,
            FileSize = fileSize,
            FileUncompressedSize = fileSize,
        };

    [Fact]
    public void CollectSearch_DefaultArchive_UsesRpfPath()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries = [MakeBinary("a.ydr", "a.ydr")];

        Json.SearchResult result = SearchHandler.CollectSearch(rpf, [], MakeOptions("/my/archive.rpf", pattern: "a"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("/my/archive.rpf", result.Matches[0].Archive);
        Assert.Equal("/my/archive.rpf", result.RpfFile);
        _ = Assert.Single(result.RpfFiles);
        Assert.Equal("/my/archive.rpf", result.RpfFiles[0]);
    }

    [Fact]
    public void CollectSearch_ExplicitArchive_MultipleMatches_AllHaveArchiveField()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries =
        [
            MakeBinary("a.ydr", "a.ydr"),
            MakeBinary("b.ydr", "b.ydr"),
        ];

        Json.SearchResult result = SearchHandler.CollectSearch(rpf, [], MakeOptions(pattern: ".ydr"), archive: "/dir/test.rpf", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, result.MatchCount);
        Assert.All(result.Matches, m => Assert.Equal("/dir/test.rpf", m.Archive));
    }

    [Fact]
    public void CollectSearch_ExplicitArchive_SetsArchiveField()
    {
        RpfFile rpf = MakeRpf();
        rpf.AllEntries = [MakeBinary("a.ydr", "a.ydr")];

        Json.SearchResult result = SearchHandler.CollectSearch(rpf, [], MakeOptions(pattern: "a"), archive: "/dir/custom.rpf", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("/dir/custom.rpf", result.Matches[0].Archive);
        Assert.Equal("/dir/custom.rpf", result.RpfFile);
        _ = Assert.Single(result.RpfFiles);
        Assert.Equal("/dir/custom.rpf", result.RpfFiles[0]);
    }
}

// ErrorResult RpfFiles

public sealed class SearchErrorResultRpfFilesTests
{
    private static SearchOptions MakeOptions(string pattern = "*.ydr") =>
        new()
        {
            RpfPath = "/test.rpf",
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = [],
            Verbose = false,
            Json = false,
            Recursive = false,
            SizeFormat = SizeFormat.IEC,
            Pattern = pattern,
        };

    [Fact]
    public void ErrorResult_SetsEmptyRpfFiles()
    {
        Json.SearchResult result = SearchHandler.ErrorResult([], MakeOptions("*.ydr"));
        Assert.Empty(result.RpfFiles);
    }
}

// PrintSearch multi-archive

[Collection("ConsoleOutput")]
public sealed class SearchPrintMultiArchiveTests
{
    private static readonly char[] SplitChars = ['\r', '\n'];

    private static SearchOptions MakeOptions(bool verbose = false) =>
        new()
        {
            RpfPath = "/dir",
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = [],
            Verbose = verbose,
            Json = false,
            Recursive = false,
            SizeFormat = SizeFormat.IEC,
            Pattern = "",
        };

    private static Json.SearchMatch MakeMatch(string archive, string path, string name) =>
        new()
        {
            Archive = archive,
            Path = path,
            Name = name,
            Size = 1024,
            Type = "binary",
            Extension = ".ydr",
        };

    [Fact]
    public void PrintSearch_MultiRpf_GroupsByArchive()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            StringWriter stderr = new();
            Console.SetOut(stdout);
            Console.SetError(stderr);

            Json.SearchResult result = new()
            {
                Success = true,
                RpfFile = "/dir",
                RpfFiles = ["/dir/a.rpf", "/dir/b.rpf"],
                Pattern = "*.ydr",
                MatchCount = 2,
                Matches =
                [
                    MakeMatch("/dir/a.rpf", "vehicles/adder.ydr", "adder.ydr"),
                    MakeMatch("/dir/b.rpf", "vehicles/zentorno.ydr", "zentorno.ydr"),
                ],
                ErrorMessages = [],
            };

            SearchHandler.PrintSearch(result, MakeOptions());

            string errOutput = stderr.ToString();
            Assert.Contains("== a.rpf ==", errOutput);
            Assert.Contains("== b.rpf ==", errOutput);

            string[] stdoutLines = stdout.ToString().Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, stdoutLines.Length);
            Assert.Equal("vehicles/adder.ydr", stdoutLines[0]);
            Assert.Equal("vehicles/zentorno.ydr", stdoutLines[1]);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void PrintSearch_MultiRpf_SummaryIncludesArchiveCount()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            StringWriter stderr = new();
            Console.SetOut(stdout);
            Console.SetError(stderr);

            Json.SearchResult result = new()
            {
                Success = true,
                RpfFile = "/dir",
                RpfFiles = ["/dir/a.rpf", "/dir/b.rpf"],
                Pattern = "adder",
                MatchCount = 3,
                Matches =
                [
                    MakeMatch("/dir/a.rpf", "vehicles/adder.ydr", "adder.ydr"),
                    MakeMatch("/dir/a.rpf", "vehicles/adder.ytd", "adder.ytd"),
                    MakeMatch("/dir/b.rpf", "vehicles/adder.yft", "adder.yft"),
                ],
                ErrorMessages = [],
            };

            SearchHandler.PrintSearch(result, MakeOptions());

            string errOutput = stderr.ToString();
            Assert.Contains("Found 3 matches across 2 archive(s)", errOutput);
            Assert.Contains("'adder'", errOutput);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void PrintSearch_SingleRpf_NoArchiveHeaders()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            StringWriter stderr = new();
            Console.SetOut(stdout);
            Console.SetError(stderr);

            Json.SearchResult result = new()
            {
                Success = true,
                RpfFile = "/test.rpf",
                RpfFiles = ["/test.rpf"],
                Pattern = "*.ydr",
                MatchCount = 1,
                Matches =
                [
                    MakeMatch("/test.rpf", "vehicles/adder.ydr", "adder.ydr"),
                ],
                ErrorMessages = [],
            };

            SearchHandler.PrintSearch(result, MakeOptions());

            string errOutput = stderr.ToString();
            Assert.DoesNotContain("==", errOutput);
            Assert.Contains("Found 1 match for", errOutput);
            Assert.DoesNotContain("archive(s)", errOutput);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void PrintSearch_MultiRpf_Verbose_ShowsSizeAndArchiveHeaders()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            StringWriter stderr = new();
            Console.SetOut(stdout);
            Console.SetError(stderr);

            Json.SearchResult result = new()
            {
                Success = true,
                RpfFile = "/dir",
                RpfFiles = ["/dir/a.rpf", "/dir/b.rpf"],
                Pattern = "*.ydr",
                MatchCount = 2,
                Matches =
                [
                    MakeMatch("/dir/a.rpf", "vehicles/adder.ydr", "adder.ydr"),
                    MakeMatch("/dir/b.rpf", "vehicles/zentorno.ydr", "zentorno.ydr"),
                ],
                ErrorMessages = [],
            };

            SearchHandler.PrintSearch(result, MakeOptions(verbose: true));

            string errOutput = stderr.ToString();
            Assert.Contains("== a.rpf ==", errOutput);
            Assert.Contains("== b.rpf ==", errOutput);

            string stdoutOutput = stdout.ToString();
            Assert.Contains("1 KiB", stdoutOutput);
            Assert.Contains("vehicles/adder.ydr", stdoutOutput);
            Assert.Contains("vehicles/zentorno.ydr", stdoutOutput);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }
}

// RelativePath

public sealed class SearchRelativePathTests
{
    [Fact]
    public void RelativePath_StripsPrefixCorrectly()
    {
        string result = SearchHandler.RelativePath("/dir", "/dir/a.rpf");
        Assert.Equal("a.rpf", result);
    }

    [Fact]
    public void RelativePath_NestedPath_StripsFullPrefix()
    {
        string result = SearchHandler.RelativePath("/base/dir", "/base/dir/sub/deep/file.rpf");
        Assert.Equal("sub/deep/file.rpf", result);
    }

    [Fact]
    public void RelativePath_HandlesBackslashes()
    {
        string result = SearchHandler.RelativePath("C:\\dir", "C:\\dir\\sub\\a.rpf");
        Assert.Equal("sub/a.rpf", result);
    }

    [Fact]
    public void RelativePath_CaseInsensitiveMatch()
    {
        string result = SearchHandler.RelativePath("/DIR", "/dir/a.rpf");
        Assert.Equal("a.rpf", result);
    }

    [Fact]
    public void RelativePath_BaseAlreadyHasTrailingSlash()
    {
        string result = SearchHandler.RelativePath("/dir/", "/dir/a.rpf");
        Assert.Equal("a.rpf", result);
    }

    [Fact]
    public void RelativePath_NoCommonPrefix_FallsBackToFileName()
    {
        string result = SearchHandler.RelativePath("/other", "/dir/a.rpf");
        Assert.Equal("a.rpf", result);
    }

    [Fact]
    public void RelativePath_MixedForwardAndBackslash()
    {
        string result = SearchHandler.RelativePath("/base/dir", "/base/dir\\sub\\a.rpf");
        Assert.Equal("sub/a.rpf", result);
    }
}

// ExecuteDirectory

[Collection("ConsoleOutput")]
public sealed class SearchExecuteDirectoryTests
{
    private static SearchOptions MakeOptions(bool json = false, string pattern = "", string? dirPath = null) =>
        new()
        {
            RpfPath = "",
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = [],
            Verbose = false,
            Json = json,
            Recursive = false,
            SizeFormat = SizeFormat.IEC,
            Pattern = pattern,
            DirPath = dirPath,
        };

    [Fact]
    public void ExecuteDirectory_DirNotFound_ReturnsOne()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stderr = new();
            Console.SetOut(new StringWriter());
            Console.SetError(stderr);

            int exitCode = SearchHandler.ExecuteDirectory(
                MakeOptions(pattern: "*.ydr", dirPath: "/nonexistent_dir_xyz_12345"),
                TestContext.Current.CancellationToken);

            Assert.Equal(1, exitCode);
            Assert.Contains("Directory not found", stderr.ToString());
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void ExecuteDirectory_DirNotFound_Json_ReturnsErrorJson()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);
            Console.SetError(new StringWriter());

            int exitCode = SearchHandler.ExecuteDirectory(
                MakeOptions(json: true, pattern: "adder", dirPath: "/nonexistent_dir_xyz_12345"),
                TestContext.Current.CancellationToken);

            Assert.Equal(1, exitCode);
            string output = stdout.ToString();
            Assert.Contains("\"success\": false", output);
            Assert.Contains("Directory not found", output);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void ExecuteDirectory_NoRpfFiles_ReturnsOne()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        string tempDir = Path.Combine(Path.GetTempPath(), $"cw_test_{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(tempDir);
        try
        {
            StringWriter stderr = new();
            Console.SetOut(new StringWriter());
            Console.SetError(stderr);

            int exitCode = SearchHandler.ExecuteDirectory(
                MakeOptions(pattern: "*.ydr", dirPath: tempDir),
                TestContext.Current.CancellationToken);

            Assert.Equal(1, exitCode);
            Assert.Contains("No .rpf files found", stderr.ToString());
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ExecuteDirectory_NoRpfFiles_Json_ReturnsErrorJson()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        string tempDir = Path.Combine(Path.GetTempPath(), $"cw_test_{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(tempDir);
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);
            Console.SetError(new StringWriter());

            int exitCode = SearchHandler.ExecuteDirectory(
                MakeOptions(json: true, pattern: "adder", dirPath: tempDir),
                TestContext.Current.CancellationToken);

            Assert.Equal(1, exitCode);
            string output = stdout.ToString();
            Assert.Contains("\"success\": false", output);
            Assert.Contains("No .rpf files found", output);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ExecuteDirectory_ExeValidationFails_ReturnsOne()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        string tempDir = Path.Combine(Path.GetTempPath(), $"cw_test_{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, "fake.rpf"), "");
        try
        {
            StringWriter stderr = new();
            Console.SetOut(new StringWriter());
            Console.SetError(stderr);

            int exitCode = SearchHandler.ExecuteDirectory(
                MakeOptions(pattern: "*.ydr", dirPath: tempDir),
                TestContext.Current.CancellationToken);

            Assert.Equal(1, exitCode);
            Assert.Contains("GTA5.exe not found", stderr.ToString());
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
            Directory.Delete(tempDir, true);
        }
    }
}
