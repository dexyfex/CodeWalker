using System;
using System.IO;

using CodeWalker.Cli.Handlers;
using CodeWalker.Cli.Helpers;

using Xunit;

namespace CodeWalker.Cli.Tests.Handlers;

[Collection("ConsoleOutput")]
public sealed class ExtractHandlerTests
{
    private static ExtractOptions MakeOptions(string rpfPath, bool json, bool dryRun = false) =>
        new()
        {
            RpfPath = rpfPath,
            ExePath = "/nonexistent",
            Gen9 = false,
            Filters = [],
            Verbose = false,
            Json = json,
            Recursive = false,
            Threads = 1,
            SizeFormat = SizeFormat.IEC,
            OutputPath = "/tmp/cw_extract_out",
            DryRun = dryRun,
            NoOverwrite = false,
            Progress = false,
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

            int exitCode = ExtractHandler.Execute(MakeOptions("/nonexistent/test.rpf", json: false), TestContext.Current.CancellationToken);

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

            int exitCode = ExtractHandler.Execute(MakeOptions("/nonexistent/test.rpf", json: true), TestContext.Current.CancellationToken);

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

            _ = ExtractHandler.Execute(MakeOptions("/nonexistent/test.rpf", json: true), TestContext.Current.CancellationToken);

            string output = stdout.ToString();
            Assert.Contains("\"rpfFile\":", output);
            Assert.Contains("\"totalFiles\": 0", output);
            Assert.Contains("\"extracted\": 0", output);
            Assert.Contains("\"skipped\": 0", output);
            Assert.Contains("\"errors\": 0", output);
            Assert.Contains("\"dryRun\": false", output);
        }
        finally { Console.SetOut(origOut); }
    }

    [Fact]
    public void Execute_MissingExe_WithExistingRpf_ReturnsOne()
    {
        string dir = Path.Combine(Path.GetTempPath(), "cw_ext_" + Guid.NewGuid().ToString("N"));
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

                int exitCode = ExtractHandler.Execute(MakeOptions(rpf, json: false), TestContext.Current.CancellationToken);

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

    [Fact]
    public void Execute_DryRun_Json_ErrorStillHasDryRunTrue()
    {
        TextWriter origOut = Console.Out;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);

            _ = ExtractHandler.Execute(MakeOptions("/nonexistent/test.rpf", json: true, dryRun: true), TestContext.Current.CancellationToken);

            string output = stdout.ToString();
            Assert.Contains("\"dryRun\": true", output);
        }
        finally { Console.SetOut(origOut); }
    }
}
