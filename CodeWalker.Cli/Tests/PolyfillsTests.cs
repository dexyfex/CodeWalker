using System;

using Xunit;

namespace CodeWalker.Cli.Tests;

/// <summary>
/// Fuzz-tests every polyfill in <see cref="StringExtensions"/> against the built-in
/// .NET implementation. Each method is called with a large combinatorial corpus of
/// hand-picked edge cases plus deterministic random strings, and the result is compared
/// against the equivalent built-in method or string-based overload.
/// </summary>
public sealed class StringExtensionsFuzzTests
{
    private static readonly StringComparison[] AllComparisons =
    [
        StringComparison.CurrentCulture,
        StringComparison.CurrentCultureIgnoreCase,
        StringComparison.InvariantCulture,
        StringComparison.InvariantCultureIgnoreCase,
        StringComparison.Ordinal,
        StringComparison.OrdinalIgnoreCase,
    ];

    private static readonly string[] Corpus = BuildCorpus();

    private static string[] BuildCorpus()
    {
        string[] handPicked =
        [
            "", " ", "a", "A", "ab", "AB", "abc", "ABC",
            "hello", "HELLO", "Hello", "Hello World", "hello world",
            "  spaces  ", "tab\there", "new\nline",
            "straße", "STRASSE", "Straße", "café", "CAFÉ",
            "résumé", "naïve", "日本語",
            "a\u00ADb", "a\u00ADbcd", "xa\u00ADbc", "a\0b", "he\u00ADllo",
            "abc123!@#", "path/to/file.txt", @"C:\Windows\System32",
            "\0null\0", "🎮🎲🎯", new string('x', 200),
            "aaa", "aaA", "AaA",
        ];

        // 100 deterministic random strings for breadth
        Random rng = new(42);
        const string alphabet = "aAbBcC xXyYzZ\t\n\0éß";
        string[] random = new string[100];
        for (int i = 0; i < random.Length; i++)
        {
            char[] buf = new char[rng.Next(0, 30)];
            for (int j = 0; j < buf.Length; j++)
                buf[j] = alphabet[rng.Next(alphabet.Length)];
            random[i] = new string(buf);
        }

        string[] result = new string[handPicked.Length + random.Length];
        handPicked.CopyTo(result, 0);
        random.CopyTo(result, handPicked.Length);
        return result;
    }

    private static readonly char[] Chars =
    [
        'a', 'A', 'z', 'Z', ' ', '\t', '\n', '\0',
        '/', '\\', '.', '!', 'é', 'ß', 'ñ', '日', 'x', 'X',
    ];

    private static readonly string[] SearchStrings =
    [
        "a", "A", "hello", "HELLO", "llo", "World", "world",
        "ab", "abc", "straße", "STRASSE", "ß", "SS", "café", "xyz", " ",
        "/", "\\", "\0", "🎮", "xx",
    ];

    // Contains(string, StringComparison)
    // Polyfill wraps IndexOf; built-in is the native implementation.

    [Fact]
    public void Contains_String_Comparison_MatchesBuiltIn()
    {
        foreach (string s in Corpus)
            foreach (string sub in SearchStrings)
                foreach (StringComparison cmp in AllComparisons)
                    AssertBool(
                        s.Contains(sub, cmp),
                        StringExtensions.Contains(s, sub, cmp),
                        $"Contains(\"{Esc(s)}\", \"{Esc(sub)}\", {cmp})");
    }

    // Contains(char)
    // Built-in string.Contains(char) exists on .NET 5+.

    [Fact]
    public void Contains_Char_MatchesBuiltIn()
    {
        foreach (string s in Corpus)
            foreach (char c in Chars)
                AssertBool(
                    s.Contains(c),
                    StringExtensions.Contains(s, c),
                    $"Contains(\"{Esc(s)}\", '{c}')");
    }

    // Contains(char, StringComparison)
    // No built-in char overload; verify against string-based Contains.

    [Fact]
    public void Contains_Char_Comparison_MatchesStringOverload()
    {
        foreach (string s in Corpus)
            foreach (char c in Chars)
                foreach (StringComparison cmp in AllComparisons)
                    AssertBool(
                        s.Contains(c.ToString(), cmp),
                        StringExtensions.Contains(s, c, cmp),
                        $"Contains(\"{Esc(s)}\", '{c}', {cmp})");
    }

    // StartsWith(char)
    // Verify against string-based StartsWith with Ordinal comparison.

    [Fact]
    public void StartsWith_Char_MatchesStringOverload()
    {
        foreach (string s in Corpus)
            foreach (char c in Chars)
                AssertBool(
                    s.StartsWith(c.ToString(), StringComparison.Ordinal),
                    StringExtensions.StartsWith(s, c),
                    $"StartsWith(\"{Esc(s)}\", '{c}')");
    }

    // EndsWith(char)
    // Verify against string-based EndsWith with Ordinal comparison.

    [Fact]
    public void EndsWith_Char_MatchesStringOverload()
    {
        foreach (string s in Corpus)
            foreach (char c in Chars)
                AssertBool(
                    s.EndsWith(c.ToString(), StringComparison.Ordinal),
                    StringExtensions.EndsWith(s, c),
                    $"EndsWith(\"{Esc(s)}\", '{c}')");
    }

    // Replace(string, string?, StringComparison)
    // Ordinal path delegates to built-in; non-Ordinal uses ReplaceCore.

    [Fact]
    public void Replace_MatchesBuiltIn()
    {
        string[] oldValues = ["a", "A", "hello", "HELLO", "llo", "straße", "SS", "ß", " ", "xx"];
        string?[] newValues = [null, "", "X", "YY", "replaced"];

        foreach (string s in Corpus)
            foreach (string old in oldValues)
                foreach (string? @new in newValues)
                    foreach (StringComparison cmp in AllComparisons)
                        AssertString(
                            s.Replace(old, @new, cmp),
                            StringExtensions.Replace(s, old, @new, cmp),
                            $"Replace(\"{Esc(s)}\", \"{Esc(old)}\", \"{Esc(@new)}\", {cmp})");
    }

    // Helpers

    private static void AssertBool(bool expected, bool actual, string label) =>
        Assert.True(expected == actual, $"{label}: expected={expected} actual={actual}");

    private static void AssertString(string expected, string actual, string label) =>
        Assert.True(string.Equals(expected, actual, StringComparison.Ordinal),
            $"{label}: expected=\"{Esc(expected)}\" actual=\"{Esc(actual)}\"");

    private static string Esc(string? s) =>
        s?.Replace("\0", "\\0", StringComparison.Ordinal)
         .Replace("\n", "\\n", StringComparison.Ordinal)
         .Replace("\r", "\\r", StringComparison.Ordinal)
         .Replace("\t", "\\t", StringComparison.Ordinal)
         .Replace("\u00AD", "\\u00AD", StringComparison.Ordinal) ?? "(null)";
}

public sealed class StringExtensionsUnitTests
{
    [Fact]
    public void Replace_NullOldValue_Throws()
    {
        _ = Assert.Throws<ArgumentNullException>(() =>
            StringExtensions.Replace("input", null!, "new", StringComparison.Ordinal));
    }

    [Fact]
    public void Replace_EmptyOldValue_Throws()
    {
        _ = Assert.Throws<ArgumentException>(() =>
            StringExtensions.Replace("input", "", "new", StringComparison.Ordinal));
    }

    [Fact]
    public void Replace_UnsupportedComparison_Throws()
    {
        _ = Assert.Throws<ArgumentException>(() =>
            StringExtensions.Replace("input", "in", "new", (StringComparison)999));
    }

    [Fact]
    public void Replace_NullNewValue_DoesNotThrow()
    {
        string result = StringExtensions.Replace("input", "in", null, StringComparison.Ordinal);
        Assert.Equal("input".Replace("in", null), result);
    }
}
