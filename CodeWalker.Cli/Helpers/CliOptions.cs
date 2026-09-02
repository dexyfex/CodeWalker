using System;
using System.CommandLine;
using System.IO;

namespace CodeWalker.Cli.Helpers;

internal static class CliOptions
{
    public static Option<FileInfo> Rpf() => new("--rpf", "-r")
    {
        Description = "Path to the RPF file",
        Required = true,
    };

    public static Option<DirectoryInfo> Exe(bool required = true) => new("--exe", "-e")
    {
        Description = "Path to the GTA V installation directory (containing GTA5.exe)",
        Required = required,
    };

    public static Option<bool> Gen9() => new("--gen9", "-g")
    {
        Description = "Use GTA V Enhanced (Gen9) mode",
    };

    public static Option<string[]> Filter() => new("--filter", "-f")
    {
        Description = "Filter files by glob patterns (e.g. *.ydd); can be specified multiple times",
        AllowMultipleArgumentsPerToken = true,
    };

    public static Option<bool> Recursive() => new("--recursive", "-R")
    {
        Description = "Process nested RPF archives",
    };

    public static Option<bool> Verbose() => new("--verbose", "-v")
    {
        Description = "Show verbose output",
    };

    public static Option<bool> Json() => new("--json")
    {
        Description = "Output results in JSON format for scripting",
    };

    public static Option<bool> Si() => new("--si")
    {
        Description = "Use SI units (1000-based: KB, MB) instead of IEC (1024-based: KiB, MiB)",
    };

    public static Option<int> Threads()
    {
        Option<int> opt = new("--threads", "-t")
        {
            Description = "Number of threads for parallel processing",
            DefaultValueFactory = _ => Environment.ProcessorCount,
        };
        opt.Validators.Add(result =>
        {
            if (result.GetValue(opt) < 1)
                result.AddError("--threads must be at least 1.");
        });
        return opt;
    }

    public static Option<DirectoryInfo> OutputDir() => new("--output", "-o")
    {
        Description = "Output directory",
        // Relative so help shows "." rather than whichever directory help was run from.
        DefaultValueFactory = _ => new DirectoryInfo("."),
    };

    public static Option<bool> DryRun() => new("--dry-run", "-n")
    {
        Description = "Show what would be done without writing any files",
    };

    public static Option<bool> NoOverwrite() => new("--no-overwrite")
    {
        Description = "Skip existing output files instead of overwriting",
    };

    public static Option<bool> Progress() => new("--progress", "-P")
    {
        Description = "Show a progress bar",
    };
}
