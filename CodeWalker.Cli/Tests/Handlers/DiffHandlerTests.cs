using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CodeWalker.Cli.Handlers;
using CodeWalker.Cli.Helpers;

using Xunit;

namespace CodeWalker.Cli.Tests.Handlers;

public sealed class DiffHandlerTests
{
    private static DiffHandler.SideEntry Entry(long size, string? hash = null, string type = "binary") =>
        new()
        {
            Name = "file.dat",
            Size = size,
            Type = type,
            Hash = hash,
        };

    private static DiffOptions Options() =>
        new()
        {
            LeftPath = "left.rpf",
            RightPath = "right.rpf",
            LeftExePath = "/left",
            RightExePath = "/right",
            LeftGen9 = false,
            RightGen9 = false,
            Recursive = false,
            Progress = false,
            Verbose = false,
            Json = false,
            SizeFormat = SizeFormat.IEC,
            Threads = 1,
        };

    // RelativeKey

    [Fact]
    public void RelativeKey_StripsArchiveNameAndSeparator() =>
        Assert.Equal(
            @"data\maps\paths.ipl",
            DiffHandler.RelativeKey(@"packed.rpf\data\maps\paths.ipl", "packed.rpf")
        );

    [Fact]
    public void RelativeKey_IgnoresCaseOfArchiveName() =>
        Assert.Equal(
            @"data\a.ipl",
            DiffHandler.RelativeKey(@"Packed.RPF\data\a.ipl", "packed.rpf")
        );

    [Fact]
    public void RelativeKey_LeavesPathAloneWhenPrefixDoesNotMatch() =>
        Assert.Equal(
            @"other.rpf\data\a.ipl",
            DiffHandler.RelativeKey(@"other.rpf\data\a.ipl", "packed.rpf")
        );

    [Fact]
    public void RelativeKey_LeavesPathAloneWhenRootIsEmpty() =>
        Assert.Equal(@"data\a.ipl", DiffHandler.RelativeKey(@"data\a.ipl", ""));

    [Fact]
    public void RelativeKey_DifferentlyNamedArchivesProduceEqualKeys() =>
        Assert.Equal(
            DiffHandler.RelativeKey(@"left.rpf\data\a.ipl", "left.rpf"),
            DiffHandler.RelativeKey(@"right.rpf\data\a.ipl", "right.rpf")
        );

    // FindHashCandidates

    [Fact]
    public void FindHashCandidates_SameSizeAndType_IsCandidate()
    {
        HashSet<string> candidates = DiffHandler.FindHashCandidates(
            new Dictionary<string, DiffHandler.SideEntry> { ["a"] = Entry(10) },
            new Dictionary<string, DiffHandler.SideEntry> { ["a"] = Entry(10) }
        );
        Assert.Equal(["a"], candidates);
    }

    [Fact]
    public void FindHashCandidates_DifferentSize_IsNotCandidate()
    {
        HashSet<string> candidates = DiffHandler.FindHashCandidates(
            new Dictionary<string, DiffHandler.SideEntry> { ["a"] = Entry(10) },
            new Dictionary<string, DiffHandler.SideEntry> { ["a"] = Entry(11) }
        );
        Assert.Empty(candidates);
    }

    [Fact]
    public void FindHashCandidates_DifferentType_IsNotCandidate()
    {
        HashSet<string> candidates = DiffHandler.FindHashCandidates(
            new Dictionary<string, DiffHandler.SideEntry> { ["a"] = Entry(10, type: "binary") },
            new Dictionary<string, DiffHandler.SideEntry> { ["a"] = Entry(10, type: "resource") }
        );
        Assert.Empty(candidates);
    }

    [Fact]
    public void FindHashCandidates_PathOnOneSideOnly_IsNotCandidate()
    {
        HashSet<string> candidates = DiffHandler.FindHashCandidates(
            new Dictionary<string, DiffHandler.SideEntry> { ["a"] = Entry(10) },
            new Dictionary<string, DiffHandler.SideEntry> { ["b"] = Entry(10) }
        );
        Assert.Empty(candidates);
    }

    // CompareSides

    [Fact]
    public void CompareSides_PathOnlyOnLeft_IsRemoved()
    {
        Json.DiffResult result = DiffHandler.CompareSides(
            new Dictionary<string, DiffHandler.SideEntry> { ["a"] = Entry(10) },
            new Dictionary<string, DiffHandler.SideEntry>(),
            [],
            Options()
        );
        Assert.Equal("a", Assert.Single(result.Removed).Path);
        Assert.Empty(result.Added);
        Assert.Empty(result.Modified);
        Assert.Empty(result.Unchanged);
    }

    [Fact]
    public void CompareSides_PathOnlyOnRight_IsAdded()
    {
        Json.DiffResult result = DiffHandler.CompareSides(
            new Dictionary<string, DiffHandler.SideEntry>(),
            new Dictionary<string, DiffHandler.SideEntry> { ["a"] = Entry(10) },
            [],
            Options()
        );
        Assert.Equal("a", Assert.Single(result.Added).Path);
        Assert.Empty(result.Removed);
    }

    [Fact]
    public void CompareSides_MatchingHashes_IsUnchanged()
    {
        Json.DiffResult result = DiffHandler.CompareSides(
            new Dictionary<string, DiffHandler.SideEntry> { ["a"] = Entry(10, "ABCD") },
            new Dictionary<string, DiffHandler.SideEntry> { ["a"] = Entry(10, "ABCD") },
            [],
            Options()
        );
        Assert.Equal("a", Assert.Single(result.Unchanged).Path);
        Assert.Empty(result.Modified);
    }

    [Fact]
    public void CompareSides_DifferentHashes_IsModified()
    {
        Json.DiffResult result = DiffHandler.CompareSides(
            new Dictionary<string, DiffHandler.SideEntry> { ["a"] = Entry(10, "ABCD") },
            new Dictionary<string, DiffHandler.SideEntry> { ["a"] = Entry(10, "DCBA") },
            [],
            Options()
        );
        Assert.Equal("a", Assert.Single(result.Modified).Path);
        Assert.Empty(result.Unchanged);
    }

    [Fact]
    public void CompareSides_DifferentSize_IsModified_WithBothSizes()
    {
        Json.DiffResult result = DiffHandler.CompareSides(
            new Dictionary<string, DiffHandler.SideEntry> { ["a"] = Entry(10) },
            new Dictionary<string, DiffHandler.SideEntry> { ["a"] = Entry(20) },
            [],
            Options()
        );
        Json.DiffEntry entry = Assert.Single(result.Modified);
        Assert.Equal(10, entry.LeftSize);
        Assert.Equal(20, entry.RightSize);
    }

    [Fact]
    public void CompareSides_UnhashedEntry_IsModified_NotUnchanged()
    {
        // An entry whose content could not be read is never reported as identical.
        Json.DiffResult result = DiffHandler.CompareSides(
            new Dictionary<string, DiffHandler.SideEntry> { ["a"] = Entry(10) },
            new Dictionary<string, DiffHandler.SideEntry> { ["a"] = Entry(10, "ABCD") },
            [],
            Options()
        );
        _ = Assert.Single(result.Modified);
        Assert.Empty(result.Unchanged);
    }

    [Fact]
    public void CompareSides_SortsEachCategoryByPath()
    {
        Json.DiffResult result = DiffHandler.CompareSides(
            new Dictionary<string, DiffHandler.SideEntry>
            {
                ["c"] = Entry(1),
                ["a"] = Entry(1),
                ["b"] = Entry(1),
            },
            new Dictionary<string, DiffHandler.SideEntry>(),
            [],
            Options()
        );
        Assert.Equal(["a", "b", "c"], result.Removed.Select(e => e.Path));
    }

    [Fact]
    public void CompareSides_SummaryMatchesCategoryCounts()
    {
        Json.DiffResult result = DiffHandler.CompareSides(
            new Dictionary<string, DiffHandler.SideEntry>
            {
                ["same"] = Entry(1, "AA"),
                ["changed"] = Entry(1, "AA"),
                ["gone"] = Entry(1),
            },
            new Dictionary<string, DiffHandler.SideEntry>
            {
                ["same"] = Entry(1, "AA"),
                ["changed"] = Entry(1, "BB"),
                ["new"] = Entry(1),
            },
            [],
            Options()
        );
        Assert.Equal(1, result.Summary.AddedCount);
        Assert.Equal(1, result.Summary.RemovedCount);
        Assert.Equal(1, result.Summary.ModifiedCount);
        Assert.Equal(1, result.Summary.UnchangedCount);
    }

    [Fact]
    public void CompareSides_ErrorMessages_MarkResultUnsuccessful()
    {
        Json.DiffResult result = DiffHandler.CompareSides(
            new Dictionary<string, DiffHandler.SideEntry>(),
            new Dictionary<string, DiffHandler.SideEntry>(),
            ["Failed to extract left entry: a"],
            Options()
        );
        Assert.False(result.Success);
        _ = Assert.Single(result.ErrorMessages);
    }
}

[Collection("ConsoleOutput")]
public sealed class DiffHandlerExecuteTests
{
    private static DiffOptions MakeOptions(string leftPath, string rightPath, bool json) =>
        new()
        {
            LeftPath = leftPath,
            RightPath = rightPath,
            LeftExePath = "/nonexistent",
            RightExePath = "/nonexistent",
            LeftGen9 = false,
            RightGen9 = false,
            Recursive = false,
            Progress = false,
            Verbose = false,
            Json = json,
            SizeFormat = SizeFormat.IEC,
            Threads = 1,
        };

    [Fact]
    public void Execute_LeftMissing_ReturnsOne()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stderr = new();
            Console.SetOut(new StringWriter());
            Console.SetError(stderr);

            int exitCode = DiffHandler.Execute(MakeOptions("/nonexistent/left.rpf", "/nonexistent/right.rpf", json: false), TestContext.Current.CancellationToken);

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
    public void Execute_LeftMissing_Json_ReturnsErrorJson()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);
            Console.SetError(new StringWriter());

            int exitCode = DiffHandler.Execute(MakeOptions("/nonexistent/left.rpf", "/nonexistent/right.rpf", json: true), TestContext.Current.CancellationToken);

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
    public void Execute_RightMissing_WithExistingLeft_ReturnsOne()
    {
        string dir = Path.Combine(Path.GetTempPath(), "cw_diff_" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(dir);
        string leftRpf = Path.Combine(dir, "left.rpf");
        File.WriteAllBytes(leftRpf, []);
        // Also need a valid exe dir
        File.WriteAllBytes(Path.Combine(dir, "GTA5.exe"), []);
        try
        {
            TextWriter origOut = Console.Out;
            TextWriter origErr = Console.Error;
            try
            {
                StringWriter stderr = new();
                Console.SetOut(new StringWriter());
                Console.SetError(stderr);

                DiffOptions options = new()
                {
                    LeftPath = leftRpf,
                    RightPath = "/nonexistent/right.rpf",
                    LeftExePath = dir,
                    RightExePath = dir,
                    LeftGen9 = false,
                    RightGen9 = false,
                    Recursive = false,
                    Progress = false,
                    Verbose = false,
                    Json = false,
                    SizeFormat = SizeFormat.IEC,
                    Threads = 1,
                };

                int exitCode = DiffHandler.Execute(options, TestContext.Current.CancellationToken);

                Assert.Equal(1, exitCode);
                Assert.Contains("RPF file not found", stderr.ToString());
            }
            finally
            {
                Console.SetOut(origOut);
                Console.SetError(origErr);
            }
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void Execute_Json_ErrorContainsExpectedFields()
    {
        TextWriter origOut = Console.Out;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);

            _ = DiffHandler.Execute(MakeOptions("/nonexistent/left.rpf", "/nonexistent/right.rpf", json: true), TestContext.Current.CancellationToken);

            string output = stdout.ToString();
            Assert.Contains("\"leftRpf\":", output);
            Assert.Contains("\"rightRpf\":", output);
        }
        finally { Console.SetOut(origOut); }
    }
}
