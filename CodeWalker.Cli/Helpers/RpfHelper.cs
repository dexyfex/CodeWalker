using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CodeWalker.GameFiles;

namespace CodeWalker.Cli.Helpers;

internal static class RpfHelper
{
    /// <summary>
    /// Validates that the GTA V executable exists in the given directory.
    /// Returns null on success, or an error message on failure.
    /// </summary>
    public static string? ValidateExe(string exePath, bool gen9)
    {
        string exeFile = gen9 ? "GTA5_Enhanced.exe" : "GTA5.exe";
        if (!File.Exists(Path.Combine(exePath, exeFile)))
            return $"{exeFile} not found in: {exePath}";

        return null;
    }

    /// <summary>
    /// Validates that the RPF file and GTA V executable exist.
    /// Returns null on success, or an error message on failure.
    /// </summary>
    public static string? ValidateInputs(string rpfPath, string exePath, bool gen9)
    {
        if (!File.Exists(rpfPath))
            return $"RPF file not found: {rpfPath}";

        return ValidateExe(exePath, gen9);
    }

    /// <summary>
    /// Validates the GTA V exe, loads encryption keys, and prints status to stderr.
    /// For commands that have no --rpf (e.g. gen9, pack).
    /// Returns an error message on failure, or null on success.
    /// </summary>
    public static string? ValidateExeAndLoadKeys(string exePath, bool gen9, bool json)
    {
        string? error = ValidateExe(exePath, gen9);
        if (error != null)
            return error;

        if (!json)
            Console.Error.WriteLine("Loading encryption keys...");
        LoadKeys(exePath, gen9);

        return null;
    }

    /// <summary>
    /// Loads GTA V encryption keys and selects the resource layout for the target generation.
    /// <see cref="RpfManager.IsGen9"/> is a process-wide switch that every resource reader
    /// consults, so it must be set before any resource file is parsed.
    /// </summary>
    public static void LoadKeys(string exePath, bool gen9)
    {
        RpfManager.IsGen9 = gen9;
        GTA5Keys.LoadFromPath(exePath, gen9);
    }

    /// <summary>
    /// Validates inputs, loads encryption keys, and prints status to stderr.
    /// Returns an error message on failure, or null on success.
    /// </summary>
    public static string? ValidateAndLoadKeys(string rpfPath, string exePath, bool gen9, bool json)
    {
        string? error = ValidateInputs(rpfPath, exePath, gen9);
        if (error != null)
            return error;

        if (!json)
            Console.Error.WriteLine("Loading encryption keys...");
        LoadKeys(exePath, gen9);

        return null;
    }

    /// <summary>
    /// Opens an RPF file with standard verbose/json output handling.
    /// </summary>
    public static RpfFile OpenRpf(
        string rpfPath,
        bool verbose,
        bool json,
        List<string> errorMessages
    )
    {
        if (!json)
            Console.Error.WriteLine($"Opening RPF: {rpfPath}");

        string rpfName = Path.GetFileName(rpfPath);
        RpfFile rpf = new(rpfPath, rpfName);
        rpf.ScanStructure(
            status =>
            {
                if (verbose && !json)
                    Console.Error.WriteLine(status);
            },
            error =>
            {
                if (!json)
                    Console.Error.WriteLine($"Error: {error}");
                errorMessages.Add(error);
            }
        );

        if (!json)
        {
            Console.Error.WriteLine(
                $"Found {rpf.GrandTotalFileCount} files in {rpf.GrandTotalRpfCount} archive(s)"
            );
        }

        return rpf;
    }

    /// <summary>
    /// Recursively collects file entries from an RPF archive, applying glob filters.
    /// </summary>
    public static List<(RpfFile rpf, RpfFileEntry entry)> CollectFiles(
        RpfFile rpf,
        string[]? filters,
        bool recursive
    )
    {
        List<(RpfFile, RpfFileEntry)> files = [];
        CollectFilesRecursive(rpf, filters, recursive, files);
        return files;
    }

    private static void CollectFilesRecursive(
        RpfFile rpf,
        string[]? filters,
        bool recursive,
        List<(RpfFile, RpfFileEntry)> files
    )
    {
        if (rpf.AllEntries != null)
        {
            files.AddRange(
                rpf.AllEntries
                    .OfType<RpfFileEntry>()
                    .Where(fe =>
                        (!recursive || !fe.NameLower.EndsWith(".rpf", StringComparison.Ordinal))
                        && Filter.Matches(fe.Path, filters))
                    .Select(fe => (rpf, fe))
            );
        }

        if (recursive && rpf.Children != null)
        {
            foreach (RpfFile child in rpf.Children)
                CollectFilesRecursive(child, filters, recursive, files);
        }
    }

    /// <summary>
    /// Returns the file type string for a given RPF file entry.
    /// </summary>
    public static string GetFileType(RpfFileEntry fileEntry) => fileEntry switch
    {
        RpfResourceFileEntry => "resource",
        RpfBinaryFileEntry => "binary",
        _ => "unknown",
    };
}
