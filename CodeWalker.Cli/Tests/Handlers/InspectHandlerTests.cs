using System;
using System.IO;

using CodeWalker.Cli.Handlers;
using CodeWalker.Cli.Helpers;

using SharpDX;

using Xunit;

namespace CodeWalker.Cli.Tests.Handlers;

public sealed class InspectHandlerTests
{
    // FormatVector3

    [Fact]
    public void FormatVector3_Zero_ReturnsFormattedZeros() =>
        Assert.Equal("0.00, 0.00, 0.00", InspectHandler.FormatVector3(Vector3.Zero));

    [Fact]
    public void FormatVector3_PositiveIntegers() =>
        Assert.Equal("1.00, 2.00, 3.00", InspectHandler.FormatVector3(new Vector3(1, 2, 3)));

    [Fact]
    public void FormatVector3_NegativeValues() =>
        Assert.Equal("-1.50, -2.75, -3.00", InspectHandler.FormatVector3(new Vector3(-1.5f, -2.75f, -3f)));

    [Fact]
    public void FormatVector3_FractionalValues_TwoDecimalPlaces()
    {
        string result = InspectHandler.FormatVector3(new Vector3(1.123f, 2.567f, 3.999f));
        // F2 rounds to 2 decimal places
        Assert.Equal("1.12, 2.57, 4.00", result);
    }

    [Fact]
    public void FormatVector3_LargeValues() =>
        Assert.Equal("1000.00, -5000.00, 9999.99",
            InspectHandler.FormatVector3(new Vector3(1000f, -5000f, 9999.99f)));

    [Fact]
    public void FormatVector3_VerySmallValues() =>
        Assert.Equal("0.01, 0.00, -0.01",
            InspectHandler.FormatVector3(new Vector3(0.01f, 0.001f, -0.01f)));

    // Additional FormatVector3 edge cases

    [Fact]
    public void FormatVector3_OneComponent() =>
        Assert.Equal("1.00, 0.00, 0.00", InspectHandler.FormatVector3(Vector3.UnitX));

    [Fact]
    public void FormatVector3_AllNegative() =>
        Assert.Equal("-1.00, -1.00, -1.00", InspectHandler.FormatVector3(new Vector3(-1, -1, -1)));
}

[Collection("ConsoleOutput")]
public sealed class InspectHandlerExecuteTests
{
    private static InspectOptions MakeOptions(string rpfPath, bool json, string filePath = "some/file.ydr") =>
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
            FilePath = filePath,
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

            int exitCode = InspectHandler.Execute(MakeOptions("/nonexistent/test.rpf", json: false), TestContext.Current.CancellationToken);

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

            int exitCode = InspectHandler.Execute(MakeOptions("/nonexistent/test.rpf", json: true), TestContext.Current.CancellationToken);

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

            _ = InspectHandler.Execute(MakeOptions("/nonexistent/test.rpf", json: true), TestContext.Current.CancellationToken);

            string output = stdout.ToString();
            Assert.Contains("\"rpfFile\":", output);
            Assert.Contains("\"path\":", output);
            Assert.Contains("\"size\": 0", output);
        }
        finally { Console.SetOut(origOut); }
    }
}
