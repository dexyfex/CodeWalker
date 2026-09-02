using System;
using System.Globalization;
using System.Threading;

using CodeWalker.Cli.Helpers;

using Xunit;

namespace CodeWalker.Cli.Tests.Helpers;

public sealed class SizeFormatTests
{
    [Theory]
#pragma warning disable format
    // Zero bytes
    [InlineData("0 B", 0)]
    // Positive edge boundaries
    [InlineData("999 B",     999)]
    [InlineData("999 KB",    999_000)]
    [InlineData("999 MB",    999_000_000)]
    [InlineData("999 GB",    999_000_000_000)]
    [InlineData("999 TB",    999_000_000_000_000)]
    [InlineData("999 PB",    999_000_000_000_000_000)]
    [InlineData("999.99 PB", 999_990_000_000_000_000)]
    // Negative edge boundaries
    [InlineData("-999 B",     -999)]
    [InlineData("-999 KB",    -999_000)]
    [InlineData("-999 MB",    -999_000_000)]
    [InlineData("-999 GB",    -999_000_000_000)]
    [InlineData("-999 TB",    -999_000_000_000_000)]
    [InlineData("-999 PB",    -999_000_000_000_000_000)]
    [InlineData("-999.99 PB", -999_990_000_000_000_000)]
    // Positive exact boundaries
    [InlineData("1 B",     1)]
    [InlineData("1 KB",    1_000)]
    [InlineData("1 MB",    1_000_000)]
    [InlineData("1 GB",    1_000_000_000)]
    [InlineData("1 TB",    1_000_000_000_000)]
    [InlineData("1 PB",    1_000_000_000_000_000)]
    [InlineData("1000 PB", 1_000_000_000_000_000_000)]
    // Negative exact boundaries
    [InlineData("-1 B",     -1)]
    [InlineData("-1 KB",    -1_000)]
    [InlineData("-1 MB",    -1_000_000)]
    [InlineData("-1 GB",    -1_000_000_000)]
    [InlineData("-1 TB",    -1_000_000_000_000)]
    [InlineData("-1 PB",    -1_000_000_000_000_000)]
    [InlineData("-1000 PB", -1_000_000_000_000_000_000)]
    // Positive fractional values
    [InlineData("1.5 KB",  1_500)]
    [InlineData("1.5 MB",  1_500_000)]
    [InlineData("1.5 GB",  1_500_000_000)]
    [InlineData("1.5 TB",  1_500_000_000_000)]
    [InlineData("1.5 PB",  1_500_000_000_000_000)]
    [InlineData("1500 PB", 1_500_000_000_000_000_000)]
    // Negative fractional values
    [InlineData("-1.5 KB",  -1_500)]
    [InlineData("-1.5 MB",  -1_500_000)]
    [InlineData("-1.5 GB",  -1_500_000_000)]
    [InlineData("-1.5 TB",  -1_500_000_000_000)]
    [InlineData("-1.5 PB",  -1_500_000_000_000_000)]
    [InlineData("-1500 PB", -1_500_000_000_000_000_000)]
    // Extremes
    [InlineData("9223.37 PB",  long.MaxValue)]
    [InlineData("-9223.37 PB", long.MinValue)]
    // Small non-boundary values
    [InlineData("42 B",  42)]
    [InlineData("500 B", 500)]
    // Rounding (display rounds up to next whole unit)
    [InlineData("2 KB",    1_999)]
    [InlineData("1000 KB", 999_999)]
#pragma warning restore format
    public void SI_ToFormattedString_ReturnsExpectedResults(string expected, long bytes)
    {
        string result = SizeFormat.SI.ToFormattedString(bytes);
        Assert.Equal(expected, result);
    }

    [Theory]
#pragma warning disable format
    // Zero bytes
    [InlineData("0 B", 0)]
    // Positive edge boundaries
    [InlineData("1023 B",   1023L)]
    [InlineData("1023 KiB", 1023L * (1L << 10))]
    [InlineData("1023 MiB", 1023L * (1L << 20))]
    [InlineData("1023 GiB", 1023L * (1L << 30))]
    [InlineData("1023 TiB", 1023L * (1L << 40))]
    [InlineData("1023 PiB", 1023L * (1L << 50))]
    [InlineData("1023.99 PiB", (long)(1023.99 * (1L << 50)))]
    // Negative edge boundaries
    [InlineData("-1023 B",   -1023L)]
    [InlineData("-1023 KiB", -1023L * (1L << 10))]
    [InlineData("-1023 MiB", -1023L * (1L << 20))]
    [InlineData("-1023 GiB", -1023L * (1L << 30))]
    [InlineData("-1023 TiB", -1023L * (1L << 40))]
    [InlineData("-1023 PiB", -1023L * (1L << 50))]
    [InlineData("-1023.99 PiB", (long)(-1023.99 * (1L << 50)))]
    // Positive exact boundaries
    [InlineData("1 B",      1L)]
    [InlineData("1 KiB",    1L << 10)]
    [InlineData("1 MiB",    1L << 20)]
    [InlineData("1 GiB",    1L << 30)]
    [InlineData("1 TiB",    1L << 40)]
    [InlineData("1 PiB",    1L << 50)]
    [InlineData("1024 PiB", 1L << 60)]
    // Negative exact boundaries
    [InlineData("-1 B",      -1L)]
    [InlineData("-1 KiB",    -1L << 10)]
    [InlineData("-1 MiB",    -1L << 20)]
    [InlineData("-1 GiB",    -1L << 30)]
    [InlineData("-1 TiB",    -1L << 40)]
    [InlineData("-1 PiB",    -1L << 50)]
    [InlineData("-1024 PiB", -1L << 60)]
    // Positive fractional values
    [InlineData("1.5 KiB",  1536L)]
    [InlineData("1.5 MiB",  1536L * (1L << 10))]
    [InlineData("1.5 GiB",  1536L * (1L << 20))]
    [InlineData("1.5 TiB",  1536L * (1L << 30))]
    [InlineData("1.5 PiB",  1536L * (1L << 40))]
    [InlineData("1536 PiB", 1536L * (1L << 50))]
    // Negative fractional values
    [InlineData("-1.5 KiB",  -1536L)]
    [InlineData("-1.5 MiB",  -1536L * (1L << 10))]
    [InlineData("-1.5 GiB",  -1536L * (1L << 20))]
    [InlineData("-1.5 TiB",  -1536L * (1L << 30))]
    [InlineData("-1.5 PiB",  -1536L * (1L << 40))]
    [InlineData("-1536 PiB", -1536L * (1L << 50))]
    // Extremes
    [InlineData("8192 PiB",  long.MaxValue)]
    [InlineData("-8192 PiB", long.MinValue)]
    // Small non-boundary values
    [InlineData("42 B",  42)]
    [InlineData("500 B", 500)]
    // Rounding (display rounds up to next whole unit)
    [InlineData("2 KiB",    (1L << 10) + 1023)]
    [InlineData("1024 KiB", (1L << 20) - 1)]
#pragma warning restore format
    public void IEC_ToFormattedString_ReturnsExpectedResults(string expected, long bytes)
    {
        string result = SizeFormat.IEC.ToFormattedString(bytes);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void InvalidFormat_Throws()
    {
        const int range = 42; // Arbitrary range to test values around the defined enum members
        for (int i = -range; i <= range; i++)
        {
            if (Enum.IsDefined(typeof(SizeFormat), i))
                continue;

            SizeFormat invalid = (SizeFormat)i;
            _ = Assert.Throws<ArgumentOutOfRangeException>(() =>
                invalid.ToFormattedString(1337));
        }
    }

    [Theory]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    [InlineData("tr-TR")]
    public void ToFormattedString_IsInvariantOfCurrentCulture(string culture)
    {
        // These strings also travel inside --json as the *Formatted fields, so a comma-decimal
        // machine must not produce different output from a dot-decimal one.
        CultureInfo previous = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Assert.Equal("1.5 KiB", SizeFormat.IEC.ToFormattedString(1536));
            Assert.Equal("1.5 KB", SizeFormat.SI.ToFormattedString(1500));
        }
        finally { Thread.CurrentThread.CurrentCulture = previous; }
    }
}
