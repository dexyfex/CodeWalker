using System;
using System.IO;

using CodeWalker.Cli.Handlers;
using CodeWalker.Cli.Helpers;

using Xunit;

namespace CodeWalker.Cli.Tests.Handlers;

[Collection("ConsoleOutput")]
public sealed class Gen9HandlerTests
{
    private static Gen9Options MakeOptions(
        string inputPath,
        string outputPath,
        bool json,
        string exePath = "/nonexistent"
    ) =>
        new()
        {
            InputPath = inputPath,
            OutputPath = outputPath,
            ExePath = exePath,
            Verbose = false,
            Json = json,
            SizeFormat = SizeFormat.IEC,
            Threads = 1,
            NoRecurse = false,
            NoOverwrite = false,
            SkipUnconverted = false,
            Progress = false,
        };

    private static string CreateTempDir()
    {
        string dir = Path.Combine(Path.GetTempPath(), "cw_gen9_" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(dir);
        return dir;
    }

    // Input folder missing

    [Fact]
    public void Execute_InputFolderMissing_ReturnsOne()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stderr = new();
            Console.SetOut(new StringWriter());
            Console.SetError(stderr);

            int exitCode = Gen9Handler.Execute(MakeOptions("/nonexistent/input", "/tmp/out", json: false), TestContext.Current.CancellationToken);

            Assert.Equal(1, exitCode);
            Assert.Contains("Error:", stderr.ToString());
            Assert.Contains("Input folder not found", stderr.ToString());
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void Execute_InputFolderMissing_Json_ReturnsErrorJson()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);
            Console.SetError(new StringWriter());

            int exitCode = Gen9Handler.Execute(MakeOptions("/nonexistent/input", "/tmp/out", json: true), TestContext.Current.CancellationToken);

            Assert.Equal(1, exitCode);
            string output = stdout.ToString();
            Assert.Contains("\"success\": false", output);
            Assert.Contains("Input folder not found", output);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    // Input equals output

    [Fact]
    public void Execute_InputEqualsOutput_ReturnsOne()
    {
        string dir = CreateTempDir();
        try
        {
            TextWriter origOut = Console.Out;
            TextWriter origErr = Console.Error;
            try
            {
                StringWriter stderr = new();
                Console.SetOut(new StringWriter());
                Console.SetError(stderr);

                int exitCode = Gen9Handler.Execute(MakeOptions(dir, dir, json: false), TestContext.Current.CancellationToken);

                Assert.Equal(1, exitCode);
                Assert.Contains("must be different", stderr.ToString());
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
    public void Execute_InputEqualsOutput_Json_ReturnsErrorJson()
    {
        string dir = CreateTempDir();
        try
        {
            TextWriter origOut = Console.Out;
            try
            {
                StringWriter stdout = new();
                Console.SetOut(stdout);

                int exitCode = Gen9Handler.Execute(MakeOptions(dir, dir, json: true), TestContext.Current.CancellationToken);

                Assert.Equal(1, exitCode);
                string output = stdout.ToString();
                Assert.Contains("\"success\": false", output);
                Assert.Contains("must be different", output);
            }
            finally { Console.SetOut(origOut); }
        }
        finally { Directory.Delete(dir, true); }
    }

    // Missing exe

    [Fact]
    public void Execute_MissingExe_ReturnsOne()
    {
        string inputDir = CreateTempDir();
        string outputDir = inputDir + "_out";
        try
        {
            TextWriter origOut = Console.Out;
            TextWriter origErr = Console.Error;
            try
            {
                StringWriter stderr = new();
                Console.SetOut(new StringWriter());
                Console.SetError(stderr);

                int exitCode = Gen9Handler.Execute(MakeOptions(inputDir, outputDir, json: false), TestContext.Current.CancellationToken);

                Assert.Equal(1, exitCode);
                Assert.Contains("Error:", stderr.ToString());
            }
            finally
            {
                Console.SetOut(origOut);
                Console.SetError(origErr);
            }
        }
        finally
        {
            Directory.Delete(inputDir, true);
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir, true);
        }
    }

    // JSON error structure

    [Fact]
    public void Execute_Json_ErrorContainsExpectedFields()
    {
        TextWriter origOut = Console.Out;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);

            _ = Gen9Handler.Execute(MakeOptions("/nonexistent/input", "/tmp/out", json: true), TestContext.Current.CancellationToken);

            string output = stdout.ToString();
            Assert.Contains("\"inputFolder\":", output);
            Assert.Contains("\"outputFolder\":", output);
            Assert.Contains("\"totalFiles\": 0", output);
            Assert.Contains("\"converted\": 0", output);
            Assert.Contains("\"skipped\": 0", output);
            Assert.Contains("\"copied\": 0", output);
            Assert.Contains("\"errors\": 0", output);
        }
        finally { Console.SetOut(origOut); }
    }
}
