using System;
using System.Collections.Generic;
using System.IO;

using CodeWalker.Cli.Helpers;
using CodeWalker.GameFiles;

using Xunit;

namespace CodeWalker.Cli.Tests;

[Collection("ConsoleOutput")]
public sealed class RpfHelperTests
{
    private static string CreateTempDir()
    {
        string dir = Path.Combine(Path.GetTempPath(), "cw_test_" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(dir);
        return dir;
    }

    // --- ValidateExe ---

    [Fact]
    public void ValidateExe_ReturnsNull_WhenExeExists()
    {
        string dir = CreateTempDir();
        try
        {
            File.WriteAllBytes(Path.Combine(dir, "GTA5.exe"), []);
            Assert.Null(RpfHelper.ValidateExe(dir, gen9: false));
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void ValidateExe_ReturnsError_WhenExeMissing()
    {
        string dir = CreateTempDir();
        try
        {
            string? error = RpfHelper.ValidateExe(dir, gen9: false);
            Assert.NotNull(error);
            Assert.Contains("GTA5.exe", error);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void ValidateExe_Gen9_ReturnsNull_WhenEnhancedExeExists()
    {
        string dir = CreateTempDir();
        try
        {
            File.WriteAllBytes(Path.Combine(dir, "GTA5_Enhanced.exe"), []);
            Assert.Null(RpfHelper.ValidateExe(dir, gen9: true));
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void ValidateExe_Gen9_ReturnsError_WhenEnhancedExeMissing()
    {
        string dir = CreateTempDir();
        try
        {
            string? error = RpfHelper.ValidateExe(dir, gen9: true);
            Assert.NotNull(error);
            Assert.Contains("GTA5_Enhanced.exe", error);
        }
        finally { Directory.Delete(dir, true); }
    }

    // --- ValidateInputs ---

    [Fact]
    public void ValidateInputs_ReturnsNull_WhenBothExist()
    {
        string dir = CreateTempDir();
        try
        {
            string rpf = Path.Combine(dir, "test.rpf");
            File.WriteAllBytes(rpf, []);
            File.WriteAllBytes(Path.Combine(dir, "GTA5.exe"), []);
            Assert.Null(RpfHelper.ValidateInputs(rpf, dir, gen9: false));
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void ValidateInputs_ReturnsError_WhenRpfMissing()
    {
        string? error = RpfHelper.ValidateInputs("/nonexistent/test.rpf", "/tmp", gen9: false);
        Assert.NotNull(error);
        Assert.Contains("RPF file not found", error);
    }

    [Fact]
    public void ValidateInputs_ReturnsError_WhenExeMissing()
    {
        string dir = CreateTempDir();
        try
        {
            string rpf = Path.Combine(dir, "test.rpf");
            File.WriteAllBytes(rpf, []);
            string? error = RpfHelper.ValidateInputs(rpf, dir, gen9: false);
            Assert.NotNull(error);
            Assert.Contains("GTA5.exe", error);
        }
        finally { Directory.Delete(dir, true); }
    }

    // --- LoadKeys ---

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void LoadKeys_SelectsResourceLayout_BeforeReadingKeys(bool gen9)
    {
        bool previous = RpfManager.IsGen9;
        string dir = CreateTempDir();
        try
        {
            RpfManager.IsGen9 = !gen9;
            // No game executable here, so key loading throws. The layout flag is still set,
            // because every resource reader consults it and it must not lag behind the keys.
            _ = Assert.Throws<FileNotFoundException>(() => RpfHelper.LoadKeys(dir, gen9));
            Assert.Equal(gen9, RpfManager.IsGen9);
        }
        finally
        {
            RpfManager.IsGen9 = previous;
            Directory.Delete(dir, true);
        }
    }

    // --- ValidateExeAndLoadKeys / ValidateAndLoadKeys early-return ---

    [Fact]
    public void ValidateExeAndLoadKeys_ReturnsError_WhenExeMissing()
    {
        string dir = CreateTempDir();
        try
        {
            string? error = RpfHelper.ValidateExeAndLoadKeys(dir, gen9: false, json: true);
            Assert.NotNull(error);
            Assert.Contains("GTA5.exe", error);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void ValidateAndLoadKeys_ReturnsError_WhenRpfMissing()
    {
        string? error = RpfHelper.ValidateAndLoadKeys(
            "/nonexistent.rpf", "/tmp", gen9: false, json: true
        );
        Assert.NotNull(error);
        Assert.Contains("RPF file not found", error);
    }

    // --- GetFileType ---

    [Fact]
    public void GetFileType_Resource() =>
        Assert.Equal("resource", RpfHelper.GetFileType(new RpfResourceFileEntry()));

    [Fact]
    public void GetFileType_Binary() =>
        Assert.Equal("binary", RpfHelper.GetFileType(new RpfBinaryFileEntry()));

    private sealed class StubFileEntry : RpfFileEntry
    {
        public override long GetFileSize() => 0;
        public override void SetFileSize(uint s) { }
        public override void Read(DataReader reader) { }
        public override void Write(DataWriter writer) { }
    }

    [Fact]
    public void GetFileType_Unknown() =>
        Assert.Equal("unknown", RpfHelper.GetFileType(new StubFileEntry()));

    // --- CollectFiles ---

    private static RpfBinaryFileEntry MakeEntry(string name, string? path = null) =>
        new() { Name = name, NameLower = name.ToLowerInvariant(), Path = path ?? name };

    [Fact]
    public void CollectFiles_NullEntries_ReturnsEmpty()
    {
        RpfFile rpf = new("test", "test.rpf", 0) { AllEntries = null };
        Assert.Empty(RpfHelper.CollectFiles(rpf, null, recursive: false));
    }

    [Fact]
    public void CollectFiles_ReturnsFileEntries()
    {
        RpfBinaryFileEntry entry = MakeEntry("test.ydr");
        RpfFile rpf = new("test", "test.rpf", 0) { AllEntries = [entry] };
        List<(RpfFile rpf, RpfFileEntry entry)> files =
            RpfHelper.CollectFiles(rpf, null, recursive: false);
        _ = Assert.Single(files);
        Assert.Same(entry, files[0].entry);
    }

    [Fact]
    public void CollectFiles_SkipsRpfEntries_WhenRecursive()
    {
        RpfBinaryFileEntry rpfEntry = MakeEntry("nested.rpf");
        RpfBinaryFileEntry fileEntry = MakeEntry("test.ydr");
        RpfFile rpf = new("test", "test.rpf", 0) { AllEntries = [rpfEntry, fileEntry] };
        List<(RpfFile rpf, RpfFileEntry entry)> files =
            RpfHelper.CollectFiles(rpf, null, recursive: true);
        _ = Assert.Single(files);
        Assert.Equal("test.ydr", files[0].entry.Name);
    }

    [Fact]
    public void CollectFiles_IncludesRpfEntries_WhenNotRecursive()
    {
        RpfBinaryFileEntry rpfEntry = MakeEntry("nested.rpf");
        RpfBinaryFileEntry fileEntry = MakeEntry("test.ydr");
        RpfFile rpf = new("test", "test.rpf", 0) { AllEntries = [rpfEntry, fileEntry] };
        List<(RpfFile rpf, RpfFileEntry entry)> files =
            RpfHelper.CollectFiles(rpf, null, recursive: false);
        Assert.Equal(2, files.Count);
        Assert.Contains(files, f => f.entry.Name == "nested.rpf");
        Assert.Contains(files, f => f.entry.Name == "test.ydr");
    }

    [Fact]
    public void CollectFiles_SkipsDirectoryEntries()
    {
        RpfDirectoryEntry dirEntry = new() { Name = "subdir", NameLower = "subdir", Path = "subdir" };
        RpfBinaryFileEntry fileEntry = MakeEntry("test.ydr");
        RpfFile rpf = new("test", "test.rpf", 0)
        {
            AllEntries = [dirEntry, fileEntry],
        };
        List<(RpfFile rpf, RpfFileEntry entry)> files =
            RpfHelper.CollectFiles(rpf, null, recursive: false);
        _ = Assert.Single(files);
    }

    [Fact]
    public void CollectFiles_AppliesFilter()
    {
        RpfBinaryFileEntry e1 = MakeEntry("test.ydr");
        RpfBinaryFileEntry e2 = MakeEntry("test.ytd");
        RpfFile rpf = new("test", "test.rpf", 0) { AllEntries = [e1, e2] };
        List<(RpfFile rpf, RpfFileEntry entry)> files =
            RpfHelper.CollectFiles(rpf, ["*.ydr"], recursive: false);
        _ = Assert.Single(files);
        Assert.Equal("test.ydr", files[0].entry.Name);
    }

    [Fact]
    public void CollectFiles_MultipleFilters()
    {
        RpfBinaryFileEntry e1 = MakeEntry("a.ydr");
        RpfBinaryFileEntry e2 = MakeEntry("b.ytd");
        RpfBinaryFileEntry e3 = MakeEntry("c.yft");
        RpfFile rpf = new("test", "test.rpf", 0) { AllEntries = [e1, e2, e3] };
        List<(RpfFile rpf, RpfFileEntry entry)> files =
            RpfHelper.CollectFiles(rpf, ["*.ydr", "*.ytd"], recursive: false);
        Assert.Equal(2, files.Count);
    }

    [Fact]
    public void CollectFiles_Recursive_WithFilter()
    {
        RpfBinaryFileEntry parentYdr = MakeEntry("a.ydr");
        RpfBinaryFileEntry parentYtd = MakeEntry("b.ytd");
        RpfBinaryFileEntry childYdr = MakeEntry("c.ydr");
        RpfBinaryFileEntry childYtd = MakeEntry("d.ytd");
        RpfFile child = new("child", "child.rpf", 0) { AllEntries = [childYdr, childYtd] };
        RpfFile parent = new("parent", "parent.rpf", 0)
        {
            AllEntries = [parentYdr, parentYtd],
            Children = [child],
        };
        List<(RpfFile rpf, RpfFileEntry entry)> files =
            RpfHelper.CollectFiles(parent, ["*.ydr"], recursive: true);
        Assert.Equal(2, files.Count);
        Assert.All(files, f => Assert.EndsWith(".ydr", f.entry.Name));
    }

    [Fact]
    public void CollectFiles_Recursive_IncludesChildren()
    {
        RpfBinaryFileEntry parentEntry = MakeEntry("a.ydr");
        RpfBinaryFileEntry childEntry = MakeEntry("b.ydr");
        RpfFile child = new("child", "child.rpf", 0) { AllEntries = [childEntry] };
        RpfFile parent = new("parent", "parent.rpf", 0)
        {
            AllEntries = [parentEntry],
            Children = [child],
        };
        Assert.Equal(2, RpfHelper.CollectFiles(parent, null, recursive: true).Count);
    }

    [Fact]
    public void CollectFiles_NonRecursive_ExcludesChildren()
    {
        RpfBinaryFileEntry parentEntry = MakeEntry("a.ydr");
        RpfBinaryFileEntry childEntry = MakeEntry("b.ydr");
        RpfFile child = new("child", "child.rpf", 0) { AllEntries = [childEntry] };
        RpfFile parent = new("parent", "parent.rpf", 0)
        {
            AllEntries = [parentEntry],
            Children = [child],
        };
        List<(RpfFile rpf, RpfFileEntry entry)> files =
            RpfHelper.CollectFiles(parent, null, recursive: false);
        _ = Assert.Single(files);
        Assert.Equal("a.ydr", files[0].entry.Name);
    }

    [Fact]
    public void CollectFiles_Recursive_ReturnsCorrectRpfRef()
    {
        RpfBinaryFileEntry childEntry = MakeEntry("b.ydr");
        RpfFile child = new("child", "child.rpf", 0) { AllEntries = [childEntry] };
        RpfFile parent = new("parent", "parent.rpf", 0)
        {
            AllEntries = [],
            Children = [child],
        };
        List<(RpfFile rpf, RpfFileEntry entry)> files =
            RpfHelper.CollectFiles(parent, null, recursive: true);
        _ = Assert.Single(files);
        Assert.Same(child, files[0].rpf);
    }

    // --- ReportError ---

    private static Json.ExportResult MakeBaseResult(string[]? errors = null) =>
        new()
        {
            Success = true,
            RpfFile = "test.rpf",
            OutputDir = "/tmp",
            Format = "xml",
            TotalFiles = 0,
            Exported = 0,
            Skipped = 0,
            Errors = 0,
            DryRun = false,
            Files = [],
            ErrorMessages = errors ?? [],
        };

    [Fact]
    public void ReportError_ReturnsOne()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            Console.SetOut(new StringWriter());
            Console.SetError(new StringWriter());
            Assert.Equal(1, Output.ReportError("err", json: false, MakeBaseResult()));
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void ReportError_Json_WritesToStdout()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter sw = new();
            Console.SetOut(sw);
            Console.SetError(new StringWriter());
            _ = Output.ReportError("test error", json: true, MakeBaseResult());
            string output = sw.ToString();
            Assert.Contains("\"success\": false", output);
            Assert.Contains("test error", output);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void ReportError_Json_PreservesExistingErrors()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter sw = new();
            Console.SetOut(sw);
            Console.SetError(new StringWriter());
            _ = Output.ReportError("new error", json: true, MakeBaseResult(["old error"]));
            string output = sw.ToString();
            Assert.Contains("old error", output);
            Assert.Contains("new error", output);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void ReportError_Text_WritesToStderr()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            StringWriter stderr = new();
            Console.SetOut(stdout);
            Console.SetError(stderr);
            _ = Output.ReportError("test error", json: false, MakeBaseResult());
            Assert.Contains("Error: test error", stderr.ToString());
            Assert.Equal("", stdout.ToString());
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void ReportError_Text_IncludesStackTrace()
    {
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stderr = new();
            Console.SetError(stderr);
            _ = Output.ReportError("err", json: false, MakeBaseResult(), "at Foo.Bar()");
            Assert.Contains("at Foo.Bar()", stderr.ToString());
        }
        finally { Console.SetError(origErr); }
    }

    [Fact]
    public void ReportError_Text_OmitsStackTrace_WhenNull()
    {
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stderr = new();
            Console.SetError(stderr);
            _ = Output.ReportError("err", json: false, MakeBaseResult());
            Assert.DoesNotContain("at ", stderr.ToString());
        }
        finally { Console.SetError(origErr); }
    }
}
