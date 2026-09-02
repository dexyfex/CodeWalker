using System;
using System.CommandLine;
using System.IO;

using CodeWalker.Cli.Helpers;

using Xunit;

namespace CodeWalker.Cli.Tests;

public sealed class CliOptionsTests
{
    [Fact]
    public void Rpf_IsRequired()
    {
        Option<FileInfo> opt = CliOptions.Rpf();
        Assert.True(opt.Required);
    }

    [Fact]
    public void Rpf_HasAlias()
    {
        Option<FileInfo> opt = CliOptions.Rpf();
        Assert.Contains("-r", opt.Aliases);
    }

    [Fact]
    public void Exe_RequiredByDefault()
    {
        Option<DirectoryInfo> opt = CliOptions.Exe();
        Assert.True(opt.Required);
    }

    [Fact]
    public void Exe_OptionalWhenSpecified()
    {
        Option<DirectoryInfo> opt = CliOptions.Exe(required: false);
        Assert.False(opt.Required);
    }

    [Fact]
    public void Exe_HasAlias()
    {
        Option<DirectoryInfo> opt = CliOptions.Exe();
        Assert.Contains("-e", opt.Aliases);
    }

    [Fact]
    public void Gen9_HasAlias()
    {
        Option<bool> opt = CliOptions.Gen9();
        Assert.Contains("-g", opt.Aliases);
    }

    [Fact]
    public void Filter_AllowsMultipleArguments()
    {
        Option<string[]> opt = CliOptions.Filter();
        Assert.True(opt.AllowMultipleArgumentsPerToken);
    }

    [Fact]
    public void Filter_HasAlias()
    {
        Option<string[]> opt = CliOptions.Filter();
        Assert.Contains("-f", opt.Aliases);
    }

    [Fact]
    public void Recursive_HasAlias()
    {
        Option<bool> opt = CliOptions.Recursive();
        Assert.Contains("-R", opt.Aliases);
    }

    [Fact]
    public void Verbose_HasAlias()
    {
        Option<bool> opt = CliOptions.Verbose();
        Assert.Contains("-v", opt.Aliases);
    }

    [Fact]
    public void Json_HasNoAlias()
    {
        Option<bool> opt = CliOptions.Json();
        // --json has no short alias
        Assert.DoesNotContain("-j", opt.Aliases);
    }

    [Fact]
    public void Threads_HasAlias()
    {
        Option<int> opt = CliOptions.Threads();
        Assert.Contains("-t", opt.Aliases);
    }

    [Fact]
    public void Threads_DefaultIsProcessorCount()
    {
        Option<int> opt = CliOptions.Threads();
        RootCommand root = [opt];
        ParseResult pr = root.Parse("");
        Assert.Equal(Environment.ProcessorCount, pr.GetValue(opt));
    }

    [Fact]
    public void Threads_RejectsZero()
    {
        Option<int> opt = CliOptions.Threads();
        RootCommand root = [opt];
        ParseResult pr = root.Parse("--threads 0");
        Assert.NotEmpty(pr.Errors);
    }

    [Fact]
    public void Threads_AcceptsOne()
    {
        Option<int> opt = CliOptions.Threads();
        RootCommand root = [opt];
        ParseResult pr = root.Parse("--threads 1");
        Assert.Empty(pr.Errors);
    }

    [Fact]
    public void OutputDir_HasAlias()
    {
        Option<DirectoryInfo> opt = CliOptions.OutputDir();
        Assert.Contains("-o", opt.Aliases);
    }

    [Fact]
    public void DryRun_HasAlias()
    {
        Option<bool> opt = CliOptions.DryRun();
        Assert.Contains("-n", opt.Aliases);
    }

    [Fact]
    public void Progress_HasAlias()
    {
        Option<bool> opt = CliOptions.Progress();
        Assert.Contains("-P", opt.Aliases);
    }

    [Fact]
    public void FactoryMethods_ReturnNewInstances()
    {
        // Each call should return a distinct instance
        Assert.NotSame(CliOptions.Rpf(), CliOptions.Rpf());
        Assert.NotSame(CliOptions.Exe(), CliOptions.Exe());
        Assert.NotSame(CliOptions.Threads(), CliOptions.Threads());
    }
}
