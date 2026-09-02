using System;
using System.IO;

using CodeWalker.Cli.Handlers;
using CodeWalker.Cli.Helpers;

using Xunit;

namespace CodeWalker.Cli.Tests.Handlers;

[Collection("ConsoleOutput")]
public sealed class ValidateHandlerTests
{
    private static ValidateOptions MakeOptions(string rpfPath, bool json) =>
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

            int exitCode = ValidateHandler.Execute(MakeOptions("/nonexistent/test.rpf", json: false), TestContext.Current.CancellationToken);

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

            int exitCode = ValidateHandler.Execute(MakeOptions("/nonexistent/test.rpf", json: true), TestContext.Current.CancellationToken);

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

            _ = ValidateHandler.Execute(MakeOptions("/nonexistent/test.rpf", json: true), TestContext.Current.CancellationToken);

            string output = stdout.ToString();
            Assert.Contains("\"rpfFile\":", output);
            Assert.Contains("\"totalFiles\": 0", output);
            Assert.Contains("\"valid\": 0", output);
            Assert.Contains("\"warnings\": 0", output);
            Assert.Contains("\"errors\": 0", output);
            Assert.Contains("\"skipped\": 0", output);
        }
        finally { Console.SetOut(origOut); }
    }

    [Fact]
    public void Execute_MissingExe_WithExistingRpf_ReturnsOne()
    {
        string dir = Path.Combine(Path.GetTempPath(), "cw_val_" + Guid.NewGuid().ToString("N"));
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

                int exitCode = ValidateHandler.Execute(MakeOptions(rpf, json: false), TestContext.Current.CancellationToken);

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
