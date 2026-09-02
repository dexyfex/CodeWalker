using System;
using System.Globalization;

namespace CodeWalker.Cli.Helpers;

/// <summary>
/// Defines size formatting options for human-readable file sizes.
/// </summary>
internal enum SizeFormat
{
    /// <summary>IEC format: 1024-based (KiB, MiB, GiB)</summary>
    IEC = 0,

    /// <summary>SI format: 1000-based (KB, MB, GB)</summary>
    SI = 1,
}

/// <summary>
/// Extension methods for SizeFormat to format byte sizes into human-readable strings.
/// </summary>
internal static class SizeFormatExtensions
{
    private static readonly string[] SiSuffixes = ["B", "KB", "MB", "GB", "TB", "PB"];
    private static readonly string[] IecSuffixes = ["B", "KiB", "MiB", "GiB", "TiB", "PiB"];

    private static double GetDivisor(this SizeFormat format) =>
        format switch
        {
            SizeFormat.SI => 1000.0,
            SizeFormat.IEC => 1024.0,
            _ => throw new ArgumentOutOfRangeException(nameof(format)),
        };

    private static string[] GetSuffixes(this SizeFormat format) =>
        format switch
        {
            SizeFormat.SI => SiSuffixes,
            SizeFormat.IEC => IecSuffixes,
            _ => throw new ArgumentOutOfRangeException(nameof(format)),
        };

    /// <summary>
    /// Formats the given byte size into a human-readable string based on the size format.
    /// Invariant, so the same archive reports the same figures on every machine and the
    /// formatted values in --json output stay stable.
    /// </summary>
    public static string ToFormattedString(this SizeFormat format, long bytes)
    {
        double divisor = format.GetDivisor();
        string[] suffixes = format.GetSuffixes();
        int i = 0;
        double size = Math.Abs((double)bytes);
        while (size >= divisor && i < suffixes.Length - 1)
        {
            size /= divisor;
            i++;
        }
        if (bytes < 0) size = -size;
        return string.Format(CultureInfo.InvariantCulture, "{0:0.##} {1}", size, suffixes[i]);
    }
}
