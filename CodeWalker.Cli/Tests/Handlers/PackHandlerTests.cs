using System;
using System.IO;

using CodeWalker.Cli.Handlers;
using CodeWalker.Cli.Helpers;

using Xunit;

namespace CodeWalker.Cli.Tests.Handlers;

[Collection("ConsoleOutput")]
public sealed class PackHandlerTests
{
    private static PackOptions MakeOptions(
        string inputPath,
        string outputPath,
        bool json,
        string exePath = "/nonexistent",
        bool force = false
    ) =>
        new()
        {
            InputPath = inputPath,
            OutputPath = outputPath,
            ExePath = exePath,
            Verbose = false,
            Json = json,
            SizeFormat = SizeFormat.IEC,
            Gen9 = false,
            Force = force,
            Progress = false,
        };

    private static string CreateTempDir()
    {
        string dir = Path.Combine(Path.GetTempPath(), "cw_pack_" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(dir);
        return dir;
    }

    // Input dir missing

    [Fact]
    public void Execute_InputDirMissing_ReturnsOne()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stderr = new();
            Console.SetOut(new StringWriter());
            Console.SetError(stderr);

            int exitCode = PackHandler.Execute(MakeOptions("/nonexistent/input", "/tmp/out.rpf", json: false), TestContext.Current.CancellationToken);

            Assert.Equal(1, exitCode);
            Assert.Contains("Input directory not found", stderr.ToString());
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void Execute_InputDirMissing_Json_ReturnsErrorJson()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);
            Console.SetError(new StringWriter());

            int exitCode = PackHandler.Execute(MakeOptions("/nonexistent/input", "/tmp/out.rpf", json: true), TestContext.Current.CancellationToken);

            Assert.Equal(1, exitCode);
            string output = stdout.ToString();
            Assert.Contains("\"success\": false", output);
            Assert.Contains("Input directory not found", output);
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    // Output file exists without --force

    [Fact]
    public void Execute_OutputExists_NoForce_ReturnsOne()
    {
        string inputDir = CreateTempDir();
        string outputFile = Path.Combine(inputDir, "output.rpf");
        File.WriteAllBytes(outputFile, []);
        try
        {
            TextWriter origOut = Console.Out;
            TextWriter origErr = Console.Error;
            try
            {
                StringWriter stderr = new();
                Console.SetOut(new StringWriter());
                Console.SetError(stderr);

                int exitCode = PackHandler.Execute(
                    MakeOptions(inputDir, outputFile, json: false, force: false),
                    TestContext.Current.CancellationToken
                );

                Assert.Equal(1, exitCode);
                Assert.Contains("already exists", stderr.ToString());
                Assert.Contains("--force", stderr.ToString());
            }
            finally
            {
                Console.SetOut(origOut);
                Console.SetError(origErr);
            }
        }
        finally { Directory.Delete(inputDir, true); }
    }

    [Fact]
    public void Execute_OutputExists_NoForce_Json_ReturnsErrorJson()
    {
        string inputDir = CreateTempDir();
        string outputFile = Path.Combine(inputDir, "output.rpf");
        File.WriteAllBytes(outputFile, []);
        try
        {
            TextWriter origOut = Console.Out;
            try
            {
                StringWriter stdout = new();
                Console.SetOut(stdout);

                int exitCode = PackHandler.Execute(
                    MakeOptions(inputDir, outputFile, json: true, force: false),
                    TestContext.Current.CancellationToken
                );

                Assert.Equal(1, exitCode);
                string output = stdout.ToString();
                Assert.Contains("\"success\": false", output);
                Assert.Contains("already exists", output);
            }
            finally { Console.SetOut(origOut); }
        }
        finally { Directory.Delete(inputDir, true); }
    }

    // Missing exe

    [Fact]
    public void Execute_MissingExe_ReturnsOne()
    {
        string inputDir = CreateTempDir();
        string outputFile = Path.Combine(Path.GetTempPath(), "cw_pack_out_" + Guid.NewGuid().ToString("N") + ".rpf");
        try
        {
            TextWriter origOut = Console.Out;
            TextWriter origErr = Console.Error;
            try
            {
                StringWriter stderr = new();
                Console.SetOut(new StringWriter());
                Console.SetError(stderr);

                int exitCode = PackHandler.Execute(MakeOptions(inputDir, outputFile, json: false), TestContext.Current.CancellationToken);

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
            if (File.Exists(outputFile))
                File.Delete(outputFile);
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

            _ = PackHandler.Execute(MakeOptions("/nonexistent/input", "/tmp/out.rpf", json: true), TestContext.Current.CancellationToken);

            string output = stdout.ToString();
            Assert.Contains("\"inputDir\":", output);
            Assert.Contains("\"outputFile\":", output);
            Assert.Contains("\"totalFiles\": 0", output);
            Assert.Contains("\"totalDirs\": 0", output);
            Assert.Contains("\"totalSize\": 0", output);
            Assert.Contains("\"errors\": 0", output);
        }
        finally { Console.SetOut(origOut); }
    }
}
