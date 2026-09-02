using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

using CodeWalker.Cli.Handlers;
using CodeWalker.Cli.Helpers;
using CodeWalker.GameFiles;

using Xunit;

namespace CodeWalker.Cli.Tests.Handlers;

// ErrorResult

public sealed class TreeErrorResultTests
{
    private static TreeOptions MakeOptions(string rpfPath = "/test.rpf") =>
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
            Depth = -1,
        };

    [Fact]
    public void ErrorResult_SetsSuccessFalse()
    {
        Json.TreeResult result = TreeHandler.ErrorResult([], MakeOptions());
        Assert.False(result.Success);
    }

    [Fact]
    public void ErrorResult_PreservesErrorMessages()
    {
        string[] msgs = ["err1", "err2"];
        Json.TreeResult result = TreeHandler.ErrorResult(msgs, MakeOptions());
        Assert.Equal(msgs, result.ErrorMessages);
    }

    [Fact]
    public void ErrorResult_SetsRpfFile()
    {
        Json.TreeResult result = TreeHandler.ErrorResult([], MakeOptions("/my/test.rpf"));
        Assert.Equal("/my/test.rpf", result.RpfFile);
    }

    [Fact]
    public void ErrorResult_CountsAreZero()
    {
        Json.TreeResult result = TreeHandler.ErrorResult([], MakeOptions());
        Assert.Equal(0, result.TotalFiles);
        Assert.Equal(0, result.TotalDirs);
    }

    [Fact]
    public void ErrorResult_RootIsNull()
    {
        Json.TreeResult result = TreeHandler.ErrorResult([], MakeOptions());
        Assert.Null(result.Root);
    }
}

// PrintTree (text)

[Collection("ConsoleOutput")]
public sealed class PrintTreeTests
{
    private static readonly char[] NewLineSeparator = ['\n'];
    private static TreeOptions MakeOptions(bool verbose = false) =>
        new()
        {
            RpfPath = "/test.rpf",
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = [],
            Verbose = verbose,
            Json = false,
            Recursive = false,
            SizeFormat = SizeFormat.IEC,
            Depth = -1,
        };

    private static Json.TreeNode MakeFileNode(
        string name,
        string path = "",
        long? size = null,
        string? sizeFormatted = null,
        string? fileType = null,
        int? version = null) =>
        new()
        {
            Name = name,
            Path = path,
            Type = "file",
            Size = size,
            SizeFormatted = sizeFormatted,
            FileType = fileType,
            Version = version
        };

    private static Json.TreeNode MakeDirNode(
        string name,
        string path = "",
        IReadOnlyList<Json.TreeNode>? children = null) =>
        new()
        {
            Name = name,
            Path = path,
            Type = "dir",
            Children = children ?? []
        };

    private static (string stdout, string stderr) Capture(
        Json.TreeNode root,
        int totalFiles,
        int totalDirs,
        TreeOptions? options = null,
        CancellationToken? cancellationToken = null)
    {
        options ??= MakeOptions();
        CancellationToken ct = cancellationToken ?? TestContext.Current.CancellationToken;
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter sw = new();
            StringWriter se = new();
            Console.SetOut(sw);
            Console.SetError(se);
            TreeHandler.PrintTree(root, totalFiles, totalDirs, options, ct);
            return (sw.ToString(), se.ToString());
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void PrintTree_RootNameOnFirstLine()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/");
        (string stdout, _) = Capture(root, 0, 0);
        string firstLine = stdout.Split('\n')[0].TrimEnd('\r');
        Assert.Equal("test.rpf/", firstLine);
    }

    [Fact]
    public void PrintTree_SummaryOnStderr()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/");
        (_, string stderr) = Capture(root, 5, 2);
        Assert.Contains("2 directories, 5 files", stderr);
    }

    [Fact]
    public void PrintTree_SingleFile_ShowsConnector()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/", children:
        [
            MakeFileNode("data.dat")
        ]);
        (string stdout, _) = Capture(root, 1, 0);
        Assert.Contains("\u2514\u2500\u2500 data.dat", stdout);
    }

    [Fact]
    public void PrintTree_MultipleFiles_ShowsBranchAndLastConnectors()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/", children:
        [
            MakeFileNode("a.dat"),
            MakeFileNode("b.dat"),
            MakeFileNode("c.dat")
        ]);
        (string stdout, _) = Capture(root, 3, 0);
        // First two get ├──, last gets └──
        Assert.Contains("\u251c\u2500\u2500 a.dat", stdout);
        Assert.Contains("\u251c\u2500\u2500 b.dat", stdout);
        Assert.Contains("\u2514\u2500\u2500 c.dat", stdout);
    }

    [Fact]
    public void PrintTree_Directory_ShowsTrailingSlash()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/", children:
        [
            MakeDirNode("subdir")
        ]);
        (string stdout, _) = Capture(root, 0, 1);
        Assert.Contains("\u2514\u2500\u2500 subdir/", stdout);
    }

    [Fact]
    public void PrintTree_NestedStructure_ShowsIndentation()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/", children:
        [
            MakeDirNode("subdir", children:
            [
                MakeFileNode("nested.dat")
            ]),
            MakeFileNode("top.dat")
        ]);
        (string stdout, _) = Capture(root, 2, 1);
        // subdir gets ├── (not last), nested.dat gets │   └──
        Assert.Contains("\u251c\u2500\u2500 subdir/", stdout);
        Assert.Contains("\u2502   \u2514\u2500\u2500 nested.dat", stdout);
        Assert.Contains("\u2514\u2500\u2500 top.dat", stdout);
    }

    [Fact]
    public void PrintTree_LastDirectory_UsesSpacePrefix()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/", children:
        [
            MakeDirNode("lastdir", children:
            [
                MakeFileNode("child.dat")
            ])
        ]);
        (string stdout, _) = Capture(root, 1, 1);
        // lastdir is last child -> └──, its children use "    " (4 spaces) prefix
        Assert.Contains("\u2514\u2500\u2500 lastdir/", stdout);
        Assert.Contains("    \u2514\u2500\u2500 child.dat", stdout);
    }

    [Fact]
    public void PrintTree_Verbose_ShowsSizeAndType()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/", children:
        [
            MakeFileNode("model.ydr", size: 1024, sizeFormatted: "1.0 KiB", fileType: "Resource")
        ]);
        (string stdout, _) = Capture(root, 1, 0, MakeOptions(verbose: true));
        Assert.Contains("model.ydr  (1.0 KiB, Resource)", stdout);
    }

    [Fact]
    public void PrintTree_Verbose_ShowsVersion()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/", children:
        [
            MakeFileNode("model.ydr", size: 1024, sizeFormatted: "1.0 KiB", fileType: "Resource", version: 110)
        ]);
        (string stdout, _) = Capture(root, 1, 0, MakeOptions(verbose: true));
        Assert.Contains("model.ydr  (1.0 KiB, Resource v110)", stdout);
    }

    [Fact]
    public void PrintTree_NonVerbose_HidesSizeAndType()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/", children:
        [
            MakeFileNode("model.ydr", size: 1024, sizeFormatted: "1.0 KiB", fileType: "Resource")
        ]);
        (string stdout, _) = Capture(root, 1, 0, MakeOptions(verbose: false));
        Assert.Contains("model.ydr", stdout);
        Assert.DoesNotContain("1.0 KiB", stdout);
        Assert.DoesNotContain("Resource", stdout);
    }

    [Fact]
    public void PrintTree_Verbose_ArchiveDir_ShowsSizeAndType()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/", children:
        [
            new Json.TreeNode
            {
                Name = "nested.rpf",
                Path = "/test.rpf/nested.rpf",
                Type = "dir",
                Size = 2048,
                SizeFormatted = "2.0 KiB",
                FileType = "binary",
                Children = [MakeFileNode("inner.dat")]
            }
        ]);
        (string stdout, _) = Capture(root, 1, 1, MakeOptions(verbose: true));
        Assert.Contains("nested.rpf/  <2.0 KiB, binary>", stdout);
    }

    [Fact]
    public void PrintTree_NonVerbose_ArchiveDir_HidesSizeAndType()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/", children:
        [
            new Json.TreeNode
            {
                Name = "nested.rpf",
                Path = "/test.rpf/nested.rpf",
                Type = "dir",
                Size = 2048,
                SizeFormatted = "2.0 KiB",
                FileType = "binary",
                Children = [MakeFileNode("inner.dat")]
            }
        ]);
        (string stdout, _) = Capture(root, 1, 1, MakeOptions(verbose: false));
        Assert.Contains("nested.rpf/", stdout);
        Assert.DoesNotContain("2.0 KiB", stdout);
    }

    [Fact]
    public void PrintTree_EmptyRoot_ShowsOnlyRootName()
    {
        Json.TreeNode root = MakeDirNode("empty.rpf/");
        (string stdout, string stderr) = Capture(root, 0, 0);
        string firstLine = stdout.Split('\n')[0].TrimEnd('\r');
        Assert.Equal("empty.rpf/", firstLine);
        Assert.Contains("0 directories, 0 files", stderr);
    }

    [Fact]
    public void PrintTree_NullChildren_NoOutput()
    {
        Json.TreeNode root = new()
        {
            Name = "test.rpf/",
            Path = "",
            Type = "dir",
            Children = null
        };
        (string stdout, _) = Capture(root, 0, 0);
        string[] lines = stdout.Split(NewLineSeparator, StringSplitOptions.RemoveEmptyEntries);
        _ = Assert.Single(lines); // Only the root name
    }
}

// PrintJsonTree

[Collection("ConsoleOutput")]
public sealed class PrintJsonTreeTests
{
    private static TreeOptions MakeOptions(string rpfPath = "/test.rpf") =>
        new()
        {
            RpfPath = rpfPath,
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = [],
            Verbose = false,
            Json = true,
            Recursive = false,
            SizeFormat = SizeFormat.IEC,
            Depth = -1,
        };

    private static Json.TreeNode MakeFileNode(
        string name,
        string path = "",
        long? size = null,
        string? sizeFormatted = null,
        string? fileType = null,
        int? version = null) =>
        new()
        {
            Name = name,
            Path = path,
            Type = "file",
            Size = size,
            SizeFormatted = sizeFormatted,
            FileType = fileType,
            Version = version
        };

    private static Json.TreeNode MakeDirNode(
        string name,
        string path = "",
        IReadOnlyList<Json.TreeNode>? children = null) =>
        new()
        {
            Name = name,
            Path = path,
            Type = "dir",
            Children = children ?? []
        };

    private static string CaptureJson(
        Json.TreeNode root,
        int totalFiles,
        int totalDirs,
        List<string>? scanErrors = null,
        TreeOptions? options = null)
    {
        options ??= MakeOptions();
        scanErrors ??= [];
        TextWriter origOut = Console.Out;
        try
        {
            StringWriter sw = new();
            Console.SetOut(sw);
            TreeHandler.PrintJsonTree(root, totalFiles, totalDirs, scanErrors, options);
            return sw.ToString();
        }
        finally { Console.SetOut(origOut); }
    }

    [Fact]
    public void PrintJsonTree_WritesValidJson()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/");
        string json = CaptureJson(root, 0, 0);

        Json.TreeResult? parsed = JsonSerializer.Deserialize<Json.TreeResult>(
            json.Trim(), Output.JsonSerializerOptions
        );
        Assert.NotNull(parsed);
    }

    [Fact]
    public void PrintJsonTree_ContainsAllTopLevelFields()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/");
        string json = CaptureJson(root, 5, 2);

        Assert.Contains("\"success\": true", json);
        Assert.Contains("\"rpfFile\": \"/test.rpf\"", json);
        Assert.Contains("\"totalFiles\": 5", json);
        Assert.Contains("\"totalDirs\": 2", json);
        Assert.Contains("\"root\":", json);
        Assert.Contains("\"errorMessages\": []", json);
    }

    [Fact]
    public void PrintJsonTree_RootNodeHasNamePathType()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/", path: "/test.rpf");
        string json = CaptureJson(root, 0, 0);

        Assert.Contains("\"name\": \"test.rpf/\"", json);
        Assert.Contains("\"path\": \"/test.rpf\"", json);
        Assert.Contains("\"type\": \"dir\"", json);
    }

    [Fact]
    public void PrintJsonTree_FileNodeIncludesSizeFields()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/", children:
        [
            MakeFileNode("model.ydr", path: "model.ydr", size: 1024, sizeFormatted: "1.0 KiB", fileType: "Resource", version: 110)
        ]);
        string json = CaptureJson(root, 1, 0);

        Assert.Contains("\"size\": 1024", json);
        Assert.Contains("\"sizeFormatted\": \"1.0 KiB\"", json);
        Assert.Contains("\"fileType\": \"Resource\"", json);
        Assert.Contains("\"version\": 110", json);
    }

    [Fact]
    public void PrintJsonTree_FileNodeOmitsNullOptionalFields()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/", children:
        [
            MakeFileNode("data.dat", path: "data.dat")
        ]);
        string json = CaptureJson(root, 1, 0);

        // These should be omitted (JsonIgnore WhenWritingNull)
        Assert.DoesNotContain("\"size\":", json);
        Assert.DoesNotContain("\"sizeFormatted\":", json);
        Assert.DoesNotContain("\"fileType\":", json);
        Assert.DoesNotContain("\"version\":", json);
    }

    [Fact]
    public void PrintJsonTree_ScanErrors_SetsSuccessFalse()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/");
        string json = CaptureJson(root, 0, 0, scanErrors: ["scan failed"]);

        Assert.Contains("\"success\": false", json);
        Assert.Contains("scan failed", json);
    }

    [Fact]
    public void PrintJsonTree_RoundTripsCorrectly()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/", path: "/test.rpf", children:
        [
            MakeDirNode("subdir", path: "/test.rpf/subdir", children:
            [
                MakeFileNode("a.dat", path: "/test.rpf/subdir/a.dat", size: 100, sizeFormatted: "100 B", fileType: "Binary")
            ]),
            MakeFileNode("b.ydr", path: "/test.rpf/b.ydr", size: 500, sizeFormatted: "500 B", fileType: "Resource", version: 110)
        ]);
        string json = CaptureJson(root, 2, 1);

        Json.TreeResult? parsed = JsonSerializer.Deserialize<Json.TreeResult>(
            json.Trim(), Output.JsonSerializerOptions
        );
        Assert.NotNull(parsed);
        Assert.True(parsed.Success);
        Assert.Equal(2, parsed.TotalFiles);
        Assert.Equal(1, parsed.TotalDirs);
        Assert.NotNull(parsed.Root);
        Assert.Equal("test.rpf/", parsed.Root.Name);
        Assert.NotNull(parsed.Root.Children);
        Assert.Equal(2, parsed.Root.Children.Count);
    }

    [Fact]
    public void PrintJsonTree_PreservesRpfPath()
    {
        Json.TreeNode root = MakeDirNode("test.rpf/");
        string json = CaptureJson(root, 0, 0, options: MakeOptions("/custom/path.rpf"));
        Assert.Contains("\"rpfFile\": \"/custom/path.rpf\"", json);
    }
}

// PrintTreeChildren cancellation

[Collection("ConsoleOutput")]
public sealed class PrintTreeChildrenCancellationTests
{
    private static TreeOptions MakeOptions() =>
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
            Depth = -1,
        };

    [Fact]
    public void PrintTreeChildren_Cancelled_ThrowsOperationCanceledException()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Json.TreeNode node = new()
        {
            Name = "root",
            Path = "",
            Type = "dir",
            Children =
            [
                new Json.TreeNode { Name = "a.dat", Path = "", Type = "file" }
            ]
        };

        TextWriter origOut = Console.Out;
        try
        {
            Console.SetOut(new StringWriter());
            _ = Assert.Throws<OperationCanceledException>(
                () => TreeHandler.PrintTreeChildren(node, "", MakeOptions(), cts.Token)
            );
        }
        finally { Console.SetOut(origOut); }
    }

    [Fact]
    public void PrintTree_Cancelled_ThrowsOperationCanceledException()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Json.TreeNode root = new()
        {
            Name = "test.rpf/",
            Path = "",
            Type = "dir",
            Children =
            [
                new Json.TreeNode { Name = "a.dat", Path = "", Type = "file" }
            ]
        };

        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            Console.SetOut(new StringWriter());
            Console.SetError(new StringWriter());
            _ = Assert.Throws<OperationCanceledException>(
                () => TreeHandler.PrintTree(root, 1, 0, MakeOptions(), cts.Token)
            );
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }
}

// Execute (integration)

[Collection("ConsoleOutput")]
public sealed class TreeExecuteTests
{
    private static TreeOptions MakeOptions(string rpfPath, bool json, int depth = -1) =>
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
            Depth = depth,
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

            int exitCode = TreeHandler.Execute(
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

            int exitCode = TreeHandler.Execute(
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
    public void Execute_Json_ErrorContainsExpectedFields()
    {
        TextWriter origOut = Console.Out;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);

            _ = TreeHandler.Execute(
                MakeOptions("/nonexistent/test.rpf", json: true),
                TestContext.Current.CancellationToken
            );

            string output = stdout.ToString();
            Assert.Contains("\"rpfFile\":", output);
            Assert.Contains("\"totalFiles\": 0", output);
            Assert.Contains("\"totalDirs\": 0", output);
            Assert.Contains("\"root\": null", output);
        }
        finally { Console.SetOut(origOut); }
    }

    [Fact]
    public void Execute_MissingExe_WithExistingRpf_ReturnsOne()
    {
        string dir = Path.Combine(Path.GetTempPath(), "cw_tree_" + Guid.NewGuid().ToString("N"));
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

                TreeOptions options = MakeOptions(rpf, json: false);
                int exitCode = TreeHandler.Execute(options, TestContext.Current.CancellationToken);

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

// CollectChildren

public sealed class CollectChildrenTests
{
    private static TreeOptions MakeOptions(
        bool recursive = false,
        string[]? filters = null) =>
        new()
        {
            RpfPath = "/test.rpf",
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = filters ?? [],
            Verbose = false,
            Json = false,
            Recursive = recursive,
            SizeFormat = SizeFormat.IEC,
            Depth = -1,
        };

    private static RpfFile MakeRpf(List<RpfFile>? children = null)
    {
        RpfFile rpf = new("test.rpf", "/test.rpf", 0)
        {
            Children = children
        };
        return rpf;
    }

    [Fact]
    public void EmptyDirectory_ReturnsEmptyList()
    {
        RpfDirectoryEntry dir = new()
        {
            Name = "root",
            NameLower = "root",
            Path = "/root",
            Directories = null!,
            Files = null!
        };

        List<ChildItem> items = TreeHandler.CollectChildren(dir, MakeRpf(), MakeOptions());

        Assert.Empty(items);
    }

    [Fact]
    public void EmptyDirectory_WithEmptyLists_ReturnsEmptyList()
    {
        RpfDirectoryEntry dir = new() { Name = "root", NameLower = "root", Path = "/root" };

        List<ChildItem> items = TreeHandler.CollectChildren(dir, MakeRpf(), MakeOptions());

        Assert.Empty(items);
    }

    [Fact]
    public void Subdirectories_ReturnedAsDirItems()
    {
        RpfDirectoryEntry sub1 = new() { Name = "sub1", NameLower = "sub1", Path = "/root/sub1" };
        RpfDirectoryEntry sub2 = new() { Name = "sub2", NameLower = "sub2", Path = "/root/sub2" };
        RpfDirectoryEntry dir = new()
        {
            Name = "root",
            NameLower = "root",
            Path = "/root",
            Directories = [sub1, sub2]
        };

        List<ChildItem> items = TreeHandler.CollectChildren(dir, MakeRpf(), MakeOptions());

        Assert.Equal(2, items.Count);
        Assert.All(items, i => Assert.True(i.IsDir));
        Assert.Equal("sub1", items[0].Name);
        Assert.Equal("sub2", items[1].Name);
        Assert.All(items, i => Assert.Null(i.ChildRpf));
    }

    [Fact]
    public void Files_ReturnedAsFileItems()
    {
        RpfBinaryFileEntry f1 = new() { Name = "a.dat", NameLower = "a.dat", Path = "/root/a.dat" };
        RpfBinaryFileEntry f2 = new() { Name = "b.dat", NameLower = "b.dat", Path = "/root/b.dat" };
        RpfDirectoryEntry dir = new()
        {
            Name = "root",
            NameLower = "root",
            Path = "/root",
            Files = [f1, f2]
        };

        List<ChildItem> items = TreeHandler.CollectChildren(dir, MakeRpf(), MakeOptions());

        Assert.Equal(2, items.Count);
        Assert.All(items, i => Assert.False(i.IsDir));
        Assert.Equal("a.dat", items[0].Name);
        Assert.Equal("b.dat", items[1].Name);
    }

    [Fact]
    public void NonRecursive_RpfFilesListedAsFiles()
    {
        RpfBinaryFileEntry rpfFile = new()
        {
            Name = "nested.rpf",
            NameLower = "nested.rpf",
            Path = "/root/nested.rpf"
        };
        RpfDirectoryEntry childRoot = new() { Name = "nested", NameLower = "nested", Path = "/nested" };
        RpfFile childRpf = new("nested.rpf", "/nested.rpf", 0) { Root = childRoot };

        RpfDirectoryEntry dir = new()
        {
            Name = "root",
            NameLower = "root",
            Path = "/root",
            Files = [rpfFile]
        };

        List<ChildItem> items = TreeHandler.CollectChildren(
            dir, MakeRpf(children: [childRpf]), MakeOptions(recursive: false));

        _ = Assert.Single(items);
        Assert.False(items[0].IsDir);
        Assert.Equal("nested.rpf", items[0].Name);
    }

    [Fact]
    public void Recursive_RpfFilesExpandedAsDirectories()
    {
        RpfBinaryFileEntry rpfFile = new()
        {
            Name = "nested.rpf",
            NameLower = "nested.rpf",
            Path = "/root/nested.rpf"
        };
        RpfDirectoryEntry childRoot = new() { Name = "nested", NameLower = "nested", Path = "/nested" };
        RpfFile childRpf = new("nested.rpf", "/nested.rpf", 0) { Root = childRoot };

        RpfDirectoryEntry dir = new()
        {
            Name = "root",
            NameLower = "root",
            Path = "/root",
            Files = [rpfFile]
        };

        List<ChildItem> items = TreeHandler.CollectChildren(
            dir, MakeRpf(children: [childRpf]), MakeOptions(recursive: true));

        // Expanded as dir + not duplicated as file
        _ = Assert.Single(items);
        Assert.True(items[0].IsDir);
        Assert.Equal("nested.rpf", items[0].Name);
        Assert.Same(childRpf, items[0].ChildRpf);
        Assert.Same(childRoot, items[0].Entry);
        Assert.Same(rpfFile, items[0].ArchiveEntry);
    }

    [Fact]
    public void Recursive_RpfWithNullRoot_NotExpanded()
    {
        RpfBinaryFileEntry rpfFile = new()
        {
            Name = "broken.rpf",
            NameLower = "broken.rpf",
            Path = "/root/broken.rpf"
        };
        RpfFile childRpf = new("broken.rpf", "/broken.rpf", 0);

        RpfDirectoryEntry dir = new()
        {
            Name = "root",
            NameLower = "root",
            Path = "/root",
            Files = [rpfFile]
        };

        List<ChildItem> items = TreeHandler.CollectChildren(
            dir, MakeRpf(children: [childRpf]), MakeOptions(recursive: true));

        // Not expanded (null Root), listed as file instead
        _ = Assert.Single(items);
        Assert.False(items[0].IsDir);
        Assert.Equal("broken.rpf", items[0].Name);
    }

    [Fact]
    public void FilterMatching_OnlyMatchingFilesIncluded()
    {
        RpfBinaryFileEntry ydr = new() { Name = "model.ydr", NameLower = "model.ydr", Path = "/root/model.ydr" };
        RpfBinaryFileEntry dat = new() { Name = "data.dat", NameLower = "data.dat", Path = "/root/data.dat" };
        RpfDirectoryEntry dir = new()
        {
            Name = "root",
            NameLower = "root",
            Path = "/root",
            Files = [ydr, dat]
        };

        List<ChildItem> items = TreeHandler.CollectChildren(
            dir, MakeRpf(), MakeOptions(filters: ["*.ydr"]));

        _ = Assert.Single(items);
        Assert.Equal("model.ydr", items[0].Name);
    }

    [Fact]
    public void DirsAndFiles_OrderedCorrectly()
    {
        RpfDirectoryEntry sub = new() { Name = "subdir", NameLower = "subdir", Path = "/root/subdir" };
        RpfBinaryFileEntry file = new() { Name = "data.dat", NameLower = "data.dat", Path = "/root/data.dat" };
        RpfDirectoryEntry dir = new()
        {
            Name = "root",
            NameLower = "root",
            Path = "/root",
            Directories = [sub],
            Files = [file]
        };

        List<ChildItem> items = TreeHandler.CollectChildren(dir, MakeRpf(), MakeOptions());

        Assert.Equal(2, items.Count);
        Assert.True(items[0].IsDir);   // dirs first
        Assert.False(items[1].IsDir);  // then files
    }
}

// BuildTreeNode

public sealed class BuildTreeNodeTests
{
    private static TreeOptions MakeOptions(
        int depth = -1,
        string[]? filters = null) =>
        new()
        {
            RpfPath = "/test.rpf",
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = filters ?? [],
            Verbose = false,
            Json = false,
            Recursive = false,
            SizeFormat = SizeFormat.IEC,
            Depth = depth,
        };

    private static RpfFile MakeRpf() => new("test.rpf", "/test.rpf", 0);

    [Fact]
    public void EmptyDirectory_ReturnsDirNodeWithEmptyChildren()
    {
        RpfDirectoryEntry dir = new() { Name = "root", NameLower = "root", Path = "/root" };
        int totalFiles = 0, totalDirs = 0;

        Json.TreeNode node = TreeHandler.BuildTreeNode(
            dir, MakeRpf(), MakeOptions(), 0, ref totalFiles, ref totalDirs,
            TestContext.Current.CancellationToken);

        Assert.Equal("root", node.Name);
        Assert.Equal("dir", node.Type);
        Assert.NotNull(node.Children);
        Assert.Empty(node.Children);
        Assert.Equal(0, totalFiles);
        Assert.Equal(0, totalDirs);
    }

    [Fact]
    public void FlatFiles_CorrectTotalFilesCount()
    {
        RpfBinaryFileEntry f1 = new()
        {
            Name = "a.dat",
            NameLower = "a.dat",
            Path = "/root/a.dat",
            FileSize = 100
        };
        RpfBinaryFileEntry f2 = new()
        {
            Name = "b.dat",
            NameLower = "b.dat",
            Path = "/root/b.dat",
            FileSize = 200
        };
        RpfDirectoryEntry dir = new()
        {
            Name = "root",
            NameLower = "root",
            Path = "/root",
            Files = [f1, f2]
        };
        int totalFiles = 0, totalDirs = 0;

        Json.TreeNode node = TreeHandler.BuildTreeNode(
            dir, MakeRpf(), MakeOptions(), 0, ref totalFiles, ref totalDirs,
            TestContext.Current.CancellationToken);

        Assert.Equal(2, totalFiles);
        Assert.Equal(0, totalDirs);
        Assert.NotNull(node.Children);
        Assert.Equal(2, node.Children.Count);
        Assert.All(node.Children, c => Assert.Equal("file", c.Type));
    }

    [Fact]
    public void FlatFiles_NodeHasSizeAndType()
    {
        RpfBinaryFileEntry f = new()
        {
            Name = "data.dat",
            NameLower = "data.dat",
            Path = "/root/data.dat",
            FileSize = 1024
        };
        RpfDirectoryEntry dir = new()
        {
            Name = "root",
            NameLower = "root",
            Path = "/root",
            Files = [f]
        };
        int totalFiles = 0, totalDirs = 0;

        Json.TreeNode node = TreeHandler.BuildTreeNode(
            dir, MakeRpf(), MakeOptions(), 0, ref totalFiles, ref totalDirs,
            TestContext.Current.CancellationToken);

        Json.TreeNode fileNode = node.Children![0];
        Assert.Equal("data.dat", fileNode.Name);
        Assert.Equal(1024, fileNode.Size);
        Assert.NotNull(fileNode.SizeFormatted);
        Assert.Equal("binary", fileNode.FileType);
    }

    [Fact]
    public void NestedDirectories_CorrectTotalDirsCount()
    {
        RpfDirectoryEntry inner = new() { Name = "inner", NameLower = "inner", Path = "/root/sub/inner" };
        RpfDirectoryEntry sub = new()
        {
            Name = "sub",
            NameLower = "sub",
            Path = "/root/sub",
            Directories = [inner]
        };

        RpfDirectoryEntry dir = new()
        {
            Name = "root",
            NameLower = "root",
            Path = "/root",
            Directories = [sub]
        };
        int totalFiles = 0, totalDirs = 0;

        Json.TreeNode node = TreeHandler.BuildTreeNode(
            dir, MakeRpf(), MakeOptions(), 0, ref totalFiles, ref totalDirs,
            TestContext.Current.CancellationToken);

        Assert.Equal(2, totalDirs);
        Assert.NotNull(node.Children);
        _ = Assert.Single(node.Children);
        Assert.Equal("sub", node.Children[0].Name);
        Assert.NotNull(node.Children[0].Children);
        Json.TreeNode innerNode = Assert.Single(node.Children[0].Children!);
        Assert.Equal("inner", innerNode.Name);
    }

    [Fact]
    public void DepthZero_NoChildrenCollected()
    {
        RpfBinaryFileEntry f = new()
        {
            Name = "data.dat",
            NameLower = "data.dat",
            Path = "/root/data.dat",
            FileSize = 100
        };
        RpfDirectoryEntry dir = new()
        {
            Name = "root",
            NameLower = "root",
            Path = "/root",
            Files = [f]
        };
        int totalFiles = 0, totalDirs = 0;

        Json.TreeNode node = TreeHandler.BuildTreeNode(
            dir, MakeRpf(), MakeOptions(depth: 0), 0, ref totalFiles, ref totalDirs,
            TestContext.Current.CancellationToken);

        Assert.NotNull(node.Children);
        Assert.Empty(node.Children);
        Assert.Equal(0, totalFiles);
    }

    [Fact]
    public void DepthOne_OnlyFirstLevel()
    {
        RpfBinaryFileEntry innerFile = new()
        {
            Name = "deep.dat",
            NameLower = "deep.dat",
            Path = "/root/sub/deep.dat",
            FileSize = 50
        };
        RpfDirectoryEntry sub = new()
        {
            Name = "sub",
            NameLower = "sub",
            Path = "/root/sub",
            Files = [innerFile]
        };

        RpfBinaryFileEntry topFile = new()
        {
            Name = "top.dat",
            NameLower = "top.dat",
            Path = "/root/top.dat",
            FileSize = 100
        };
        RpfDirectoryEntry dir = new()
        {
            Name = "root",
            NameLower = "root",
            Path = "/root",
            Directories = [sub],
            Files = [topFile]
        };
        int totalFiles = 0, totalDirs = 0;

        Json.TreeNode node = TreeHandler.BuildTreeNode(
            dir, MakeRpf(), MakeOptions(depth: 1), 0, ref totalFiles, ref totalDirs,
            TestContext.Current.CancellationToken);

        Assert.Equal(1, totalFiles); // only top.dat counted
        Assert.Equal(1, totalDirs);  // sub counted as dir
        Json.TreeNode subNode = node.Children!.First(c => c.Name == "sub");
        Assert.NotNull(subNode.Children);
        Assert.Empty(subNode.Children); // depth limit prevents going deeper
    }

    [Fact]
    public void FilterWithEmptyDirs_Pruned()
    {
        RpfDirectoryEntry emptySub = new()
        {
            Name = "empty",
            NameLower = "empty",
            Path = "/root/empty"
        };
        RpfBinaryFileEntry matchFile = new()
        {
            Name = "model.ydr",
            NameLower = "model.ydr",
            Path = "/root/model.ydr",
            FileSize = 256
        };
        RpfDirectoryEntry dir = new()
        {
            Name = "root",
            NameLower = "root",
            Path = "/root",
            Directories = [emptySub],
            Files = [matchFile]
        };
        int totalFiles = 0, totalDirs = 0;

        Json.TreeNode node = TreeHandler.BuildTreeNode(
            dir, MakeRpf(), MakeOptions(filters: ["*.ydr"]), 0, ref totalFiles, ref totalDirs,
            TestContext.Current.CancellationToken);

        // Empty dir pruned, only file remains
        Assert.Equal(1, totalFiles);
        Assert.Equal(0, totalDirs);
        Assert.NotNull(node.Children);
        _ = Assert.Single(node.Children);
        Assert.Equal("model.ydr", node.Children[0].Name);
    }

    [Fact]
    public void Cancellation_ThrowsOperationCanceledException()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        RpfDirectoryEntry dir = new() { Name = "root", NameLower = "root", Path = "/root" };
        int totalFiles = 0, totalDirs = 0;

        _ = Assert.Throws<OperationCanceledException>(() =>
            TreeHandler.BuildTreeNode(
                dir, MakeRpf(), MakeOptions(), 0, ref totalFiles, ref totalDirs, cts.Token));
    }

    [Fact]
    public void ResourceFile_VersionPopulated()
    {
        // Version = (sv << 4) + gv where sv = (sysFlags >> 28) & 0xF, gv = (gfxFlags >> 28) & 0xF
        // For version 110 = 0x6E = (6 << 4) + 14: sysFlags = 6 << 28, gfxFlags = 14 << 28
        RpfResourceFileEntry rfe = new()
        {
            Name = "model.ydr",
            NameLower = "model.ydr",
            Path = "/root/model.ydr",
            FileSize = 512,
            SystemFlags = (uint)6 << 28,
            GraphicsFlags = (uint)14 << 28
        };
        RpfDirectoryEntry dir = new()
        {
            Name = "root",
            NameLower = "root",
            Path = "/root",
            Files = [rfe]
        };
        int totalFiles = 0, totalDirs = 0;

        Json.TreeNode node = TreeHandler.BuildTreeNode(
            dir, MakeRpf(), MakeOptions(), 0, ref totalFiles, ref totalDirs,
            TestContext.Current.CancellationToken);

        Json.TreeNode fileNode = node.Children![0];
        Assert.Equal("resource", fileNode.FileType);
        Assert.Equal(110, fileNode.Version);
    }

    [Fact]
    public void BinaryFile_VersionNull()
    {
        RpfBinaryFileEntry bfe = new()
        {
            Name = "data.dat",
            NameLower = "data.dat",
            Path = "/root/data.dat",
            FileSize = 256
        };
        RpfDirectoryEntry dir = new()
        {
            Name = "root",
            NameLower = "root",
            Path = "/root",
            Files = [bfe]
        };
        int totalFiles = 0, totalDirs = 0;

        Json.TreeNode node = TreeHandler.BuildTreeNode(
            dir, MakeRpf(), MakeOptions(), 0, ref totalFiles, ref totalDirs,
            TestContext.Current.CancellationToken);

        Json.TreeNode fileNode = node.Children![0];
        Assert.Equal("binary", fileNode.FileType);
        Assert.Null(fileNode.Version);
    }

    [Fact]
    public void DirName_FallsBackToRpfFilePath()
    {
        RpfDirectoryEntry dir = new() { Name = null!, NameLower = null!, Path = null! };
        RpfFile rpf = new("test.rpf", "/some/path/test.rpf", 0);
        int totalFiles = 0, totalDirs = 0;

        Json.TreeNode node = TreeHandler.BuildTreeNode(
            dir, rpf, MakeOptions(), 0, ref totalFiles, ref totalDirs,
            TestContext.Current.CancellationToken);

        Assert.Equal("test.rpf", node.Name);
        Assert.Equal("/some/path/test.rpf", node.Path);
    }
}
