using System;
using System.IO;
using System.Text.Json;
using System.Threading;

using CodeWalker.Cli.Handlers;
using CodeWalker.Cli.Helpers;
using CodeWalker.GameFiles;

using Xunit;

namespace CodeWalker.Cli.Tests.Handlers;

// ParseEncoding

public sealed class ParseEncodingTests
{
    [Theory]
    [InlineData("UTF-8", JenkHashInputEncoding.UTF8)]
    [InlineData("utf-8", JenkHashInputEncoding.UTF8)]
    [InlineData("Utf-8", JenkHashInputEncoding.UTF8)]
    [InlineData("ASCII", JenkHashInputEncoding.ASCII)]
    [InlineData("ascii", JenkHashInputEncoding.ASCII)]
    [InlineData("Ascii", JenkHashInputEncoding.ASCII)]
    public void ValidEncoding_ReturnsExpected(string input, JenkHashInputEncoding expected) =>
        Assert.Equal(expected, HashHandler.ParseEncoding(input));

    [Theory]
    [InlineData(JenkHashInputEncoding.UTF8, "utf-8")]
    [InlineData(JenkHashInputEncoding.ASCII, "ascii")]
    public void EncodingName_RoundTripsThroughParseEncoding(
        JenkHashInputEncoding encoding,
        string expected
    )
    {
        string name = HashHandler.EncodingName(encoding);
        Assert.Equal(expected, name);
        Assert.Equal(encoding, HashHandler.ParseEncoding(name));
    }

    [Theory]
    [InlineData("utf8")]
    [InlineData("latin-1")]
    [InlineData("")]
    [InlineData("UTF8")]
    public void InvalidEncoding_ThrowsArgumentException(string input)
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => HashHandler.ParseEncoding(input)
        );
        Assert.Contains("Unknown encoding", ex.Message);
        Assert.Contains(input, ex.Message);
    }
}

// ErrorResult

public sealed class ErrorResultTests
{
    [Fact]
    public void ErrorResult_SetsSuccessFalse()
    {
        Json.HashResult result = HashHandler.ErrorResult([]);
        Assert.False(result.Success);
        Assert.Empty(result.Hashes);
    }

    [Fact]
    public void ErrorResult_PreservesErrorMessages()
    {
        string[] msgs = ["err1", "err2"];
        Json.HashResult result = HashHandler.ErrorResult(msgs);
        Assert.Equal(msgs, result.ErrorMessages);
    }
}

// PrintHashes

[Collection("ConsoleOutput")]
public sealed class PrintHashesTests
{
    private static string Capture(string[] inputs, JenkHashInputEncoding encoding)
    {
        TextWriter orig = Console.Out;
        try
        {
            StringWriter sw = new();
            Console.SetOut(sw);
            Json.HashEntry[] hashes = HashHandler.CollectHashes(inputs, encoding, CancellationToken.None);
            HashHandler.PrintHashes(hashes, TestContext.Current.CancellationToken);
            return sw.ToString();
        }
        finally { Console.SetOut(orig); }
    }

    [Fact]
    public void SingleInput_PrintsAllFourLines()
    {
        string output = Capture(["test"], JenkHashInputEncoding.UTF8);
        Assert.Contains("Input (utf-8): test", output);
        Assert.Contains("Hash (uint):", output);
        Assert.Contains("Hash (int):", output);
        Assert.Contains("Hash (hex):", output);
    }

    [Fact]
    public void SingleInput_MatchesJenkHash()
    {
        JenkHash expected = new("test", JenkHashInputEncoding.UTF8);
        string output = Capture(["test"], JenkHashInputEncoding.UTF8);
        Assert.Contains($"Hash (uint): {expected.HashUint}", output);
        Assert.Contains($"Hash (int):  {expected.HashInt}", output);
        Assert.Contains($"Hash (hex):  {expected.HashHex}", output);
    }

    [Fact]
    public void AsciiEncoding_ShowsAsciiInHeader()
    {
        string output = Capture(["hello"], JenkHashInputEncoding.ASCII);
        Assert.Contains("Input (ascii): hello", output);
    }

    [Fact]
    public void MultipleInputs_PrintsEach()
    {
        string output = Capture(["alpha", "bravo"], JenkHashInputEncoding.UTF8);
        Assert.Contains("Input (utf-8): alpha", output);
        Assert.Contains("Input (utf-8): bravo", output);
    }

    [Fact]
    public void EmptyString_Succeeds()
    {
        string output = Capture([""], JenkHashInputEncoding.UTF8);
        Assert.Contains("Input (utf-8): ", output);
        Assert.Contains("Hash (uint):", output);
    }

    [Fact]
    public void Cancelled_ThrowsOperationCanceledException()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Json.HashEntry[] hashes = HashHandler.CollectHashes(
            ["test"],
            JenkHashInputEncoding.UTF8,
            CancellationToken.None
        );

        TextWriter orig = Console.Out;
        try
        {
            Console.SetOut(new StringWriter());
            _ = Assert.Throws<OperationCanceledException>(
                () => HashHandler.PrintHashes(hashes, cts.Token)
            );
        }
        finally { Console.SetOut(orig); }
    }
}

// PrintJsonHashes

[Collection("ConsoleOutput")]
public sealed class PrintJsonHashesTests
{
    private static string Capture(string[] inputs, JenkHashInputEncoding encoding)
    {
        TextWriter orig = Console.Out;
        try
        {
            StringWriter sw = new();
            Console.SetOut(sw);
            Json.HashEntry[] hashes = HashHandler.CollectHashes(inputs, encoding, CancellationToken.None);
            HashHandler.PrintJsonHashes(hashes);
            return sw.ToString();
        }
        finally { Console.SetOut(orig); }
    }

    [Fact]
    public void SingleInput_WritesValidJson()
    {
        Json.HashResult? result = JsonSerializer.Deserialize<Json.HashResult>(
            Capture(["test"], JenkHashInputEncoding.UTF8).Trim(),
            Output.JsonSerializerOptions
        );
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.ErrorMessages);
        _ = Assert.Single(result.Hashes);
    }

    [Fact]
    public void SingleInput_MatchesJenkHash()
    {
        JenkHash expected = new("test", JenkHashInputEncoding.UTF8);
        Json.HashResult? result = JsonSerializer.Deserialize<Json.HashResult>(
            Capture(["test"], JenkHashInputEncoding.UTF8).Trim(),
            Output.JsonSerializerOptions
        );
        Assert.NotNull(result);

        Json.HashEntry entry = result.Hashes[0];
        Assert.Equal("test", entry.Input);
        Assert.Equal(expected.HashUint, entry.Hash);
        Assert.Equal(expected.HashInt, entry.HashSigned);
        Assert.Equal(expected.HashHex, entry.HashHex);
        Assert.Equal("utf-8", entry.Encoding);
    }

    [Fact]
    public void AsciiEncoding_SetsEncodingField()
    {
        Json.HashResult? result = JsonSerializer.Deserialize<Json.HashResult>(
            Capture(["hello"], JenkHashInputEncoding.ASCII).Trim(),
            Output.JsonSerializerOptions
        );
        Assert.NotNull(result);
        Assert.Equal("ascii", result.Hashes[0].Encoding);
    }

    [Fact]
    public void MultipleInputs_ReturnsAll()
    {
        Json.HashResult? result = JsonSerializer.Deserialize<Json.HashResult>(
            Capture(["alpha", "bravo"], JenkHashInputEncoding.UTF8).Trim(),
            Output.JsonSerializerOptions
        );
        Assert.NotNull(result);
        Assert.Equal(2, result.Hashes.Count);
        Assert.Equal("alpha", result.Hashes[0].Input);
        Assert.Equal("bravo", result.Hashes[1].Input);
    }
}

// CollectHashes

public sealed class CollectHashesTests
{
    [Fact]
    public void SingleInput_ReturnsOneEntry()
    {
        Json.HashEntry[] entries = HashHandler.CollectHashes(
            ["vehicle"],
            JenkHashInputEncoding.UTF8,
            TestContext.Current.CancellationToken
        );
        _ = Assert.Single(entries);
    }

    [Fact]
    public void SingleInput_MatchesJenkHash()
    {
        JenkHash expected = new("vehicle", JenkHashInputEncoding.UTF8);
        Json.HashEntry[] entries = HashHandler.CollectHashes(
            ["vehicle"],
            JenkHashInputEncoding.UTF8,
            TestContext.Current.CancellationToken
        );

        Json.HashEntry entry = entries[0];
        Assert.Equal("vehicle", entry.Input);
        Assert.Equal(expected.HashUint, entry.Hash);
        Assert.Equal(expected.HashInt, entry.HashSigned);
        Assert.Equal(expected.HashHex, entry.HashHex);
        Assert.Equal("utf-8", entry.Encoding);
    }

    [Fact]
    public void AsciiEncoding_SetsEncodingField()
    {
        Json.HashEntry[] entries = HashHandler.CollectHashes(
            ["test"],
            JenkHashInputEncoding.ASCII,
            TestContext.Current.CancellationToken
        );
        Assert.Equal("ascii", entries[0].Encoding);
    }

    [Fact]
    public void MultipleInputs_ReturnsAll()
    {
        Json.HashEntry[] entries = HashHandler.CollectHashes(
            ["one", "two", "three"],
            JenkHashInputEncoding.UTF8,
            TestContext.Current.CancellationToken
        );
        Assert.Equal(3, entries.Length);
        Assert.Equal("one", entries[0].Input);
        Assert.Equal("two", entries[1].Input);
        Assert.Equal("three", entries[2].Input);
    }

    [Fact]
    public void DifferentInputs_ProduceDifferentHashes()
    {
        Json.HashEntry[] entries = HashHandler.CollectHashes(
            ["alpha", "beta"],
            JenkHashInputEncoding.UTF8,
            TestContext.Current.CancellationToken
        );
        Assert.NotEqual(entries[0].Hash, entries[1].Hash);
    }

    [Fact]
    public void SameInput_ProducesSameHash()
    {
        Json.HashEntry[] a = HashHandler.CollectHashes(
            ["deterministic"],
            JenkHashInputEncoding.UTF8,
            TestContext.Current.CancellationToken
        );
        Json.HashEntry[] b = HashHandler.CollectHashes(
            ["deterministic"],
            JenkHashInputEncoding.UTF8,
            TestContext.Current.CancellationToken
        );
        Assert.Equal(a[0].Hash, b[0].Hash);
    }

    [Fact]
    public void Cancelled_ThrowsOperationCanceledException()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();
        _ = Assert.Throws<OperationCanceledException>(
            () => HashHandler.CollectHashes(["test"],
                JenkHashInputEncoding.UTF8,
                cts.Token
            )
        );
    }
}

// Execute (integration)

[Collection("ConsoleOutput")]
public sealed class HashExecuteTests
{
    private static HashOptions MakeOptions(
        string[] inputs,
        string encoding = HashOptions.DefaultEncoding,
        bool json = false
    ) => new() { Inputs = inputs, Encoding = encoding, Json = json };

    [Fact]
    public void Text_ReturnsZero()
    {
        TextWriter orig = Console.Out;
        try
        {
            Console.SetOut(new StringWriter());
            Assert.Equal(0, HashHandler.Execute(
                MakeOptions(["test"]),
                TestContext.Current.CancellationToken
            ));
        }
        finally { Console.SetOut(orig); }
    }

    [Fact]
    public void Json_WritesValidJson()
    {
        TextWriter orig = Console.Out;
        try
        {
            StringWriter sw = new();
            Console.SetOut(sw);
            Assert.Equal(0, HashHandler.Execute(
                MakeOptions(["test"], json: true),
                TestContext.Current.CancellationToken
            ));

            Json.HashResult? result = JsonSerializer.Deserialize<Json.HashResult>(
                sw.ToString().Trim(), Output.JsonSerializerOptions
            );
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.ErrorMessages);
            _ = Assert.Single(result.Hashes);
        }
        finally { Console.SetOut(orig); }
    }

    [Fact]
    public void InvalidEncoding_ReturnsOne()
    {
        TextWriter origOut = Console.Out;
        TextWriter origErr = Console.Error;
        try
        {
            Console.SetOut(new StringWriter());
            StringWriter stderr = new();
            Console.SetError(stderr);

            int exitCode = HashHandler.Execute(
                MakeOptions(["test"], encoding: "bad"),
                TestContext.Current.CancellationToken
            );

            Assert.Equal(1, exitCode);
            Assert.Contains("Unknown encoding", stderr.ToString());
        }
        finally
        {
            Console.SetOut(origOut);
            Console.SetError(origErr);
        }
    }

    [Fact]
    public void Json_InvalidEncoding_ReturnsJsonError()
    {
        TextWriter origOut = Console.Out;
        try
        {
            StringWriter stdout = new();
            Console.SetOut(stdout);

            int exitCode = HashHandler.Execute(
                MakeOptions(["test"], encoding: "bad", json: true),
                TestContext.Current.CancellationToken
            );

            Assert.Equal(1, exitCode);
            string output = stdout.ToString();
            Assert.Contains("\"success\": false", output);
            Assert.Contains("Unknown encoding", output);
        }
        finally { Console.SetOut(origOut); }
    }

    [Fact]
    public void DefaultEncoding_IsUtf8() =>
        Assert.Equal("utf-8", HashOptions.DefaultEncoding);

    [Fact]
    public void Text_Cancelled_ThrowsOperationCanceledException()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        TextWriter orig = Console.Out;
        try
        {
            Console.SetOut(new StringWriter());
            _ = Assert.Throws<OperationCanceledException>(
                () => HashHandler.Execute(
                    MakeOptions(["test"]),
                    cts.Token
                )
            );
        }
        finally { Console.SetOut(orig); }
    }

    [Fact]
    public void Json_Cancelled_ThrowsOperationCanceledException()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        TextWriter orig = Console.Out;
        try
        {
            Console.SetOut(new StringWriter());
            _ = Assert.Throws<OperationCanceledException>(
                () => HashHandler.Execute(
                    MakeOptions(["test"], json: true),
                    cts.Token
                )
            );
        }
        finally { Console.SetOut(orig); }
    }
}
