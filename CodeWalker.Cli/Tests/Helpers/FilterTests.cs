using CodeWalker.Cli.Helpers;

using Xunit;

namespace CodeWalker.Cli.Tests.Helpers;

public sealed class FilterTests
{
    [Theory]
    // Null or empty returns empty
    [InlineData(new string[] { }, null)]
    [InlineData(new string[] { }, new string[] { })]
    // Trim and lowercase
    [InlineData(new[] { ".ydr", "foo" }, new[] { "  .YDR  ", "Foo" })]
    // Strips blank entries
    [InlineData(new[] { "a", "b" }, new[] { "a", "", "  ", "b" })]
    // Preserves wildcards and lowercases
    [InlineData(new[] { "*.ydr" }, new[] { " *.YDR " })]
    [InlineData(new[] { "model?.ydr" }, new[] { "Model?.YDR" })]
    public void Normalize_ReturnsExpectedResult(string[] expected, string[]? input)
    {
        string[] result = Filter.Normalize(input);
        Assert.Equal(expected, result);
    }

    [Theory]
#pragma warning disable format
    // No filters
    [InlineData(true, "anything.ydr")]
    [InlineData(true, "anything.ydr", null)]
    // Empty path
    [InlineData(false, "", ".ydr")]
    [InlineData(true,  "", null)]
    // Extension with dot
    [InlineData(true,  "model.ydr", ".ydr")]
    [InlineData(false, "model.ytd", ".ydr")]
    // Extension without dot
    [InlineData(true,  "model.ydr", "ydr")]
    [InlineData(false, "model.ytd", "ydr")]
    // Wildcard pattern
    [InlineData(true,  "model.ydr",     "*.ydr")]
    [InlineData(true,  "dir/model.ydr", "*.ydr")]
    [InlineData(false, "model.ytd",     "*.ydr")]
    [InlineData(false, "dir/model.ytd", "*.ydr")]
    // Path pattern
    [InlineData(true,  "vehicles/foo.ydr",   "vehicles/*.ydr")]
    [InlineData(true,  "vehicles/bar.ydr",   "vehicles/*.ydr")]
    [InlineData(false, "peds/ped.ydr",       "vehicles/*.ydr")]
    [InlineData(false, "vehicles/model.ytd", "vehicles/*.ydr")]
    // Globstar pattern
    [InlineData(true,  "x64/dlcpacks/vehicles/car.ydr", "**/vehicles/*.ydr")]
    [InlineData(true,  "vehicles/car.ydr",              "**/vehicles/*.ydr")]
    [InlineData(false, "x64/dlcpacks/vehicles/car.ytd", "**/vehicles/*.ydr")]
    [InlineData(false, "vehicles/car.ytd",              "**/vehicles/*.ydr")]
    [InlineData(false, "x64/dlcpacks/peds/foo.ydr",     "**/vehicles/*.ydr")]
    [InlineData(false, "peds/bar.ydr",                  "**/vehicles/*.ydr")]
    // Case insensitive
    [InlineData(true,  "MODEL.YDR",                     ".ydr")]
    [InlineData(true,  "MODEL.YDR",                     "ydr")]
    [InlineData(true,  "MODEL.YDR",                     "*.ydr")]
    [InlineData(true,  "X64/DLCPACKS/VEHICLES/CAR.YDR", "**/vehicles/*.ydr")]
    [InlineData(true,  "VEHICLES/FOO.YDR",              "vehicles/*.ydr")]
    [InlineData(false, "X64/DLCPACKS/VEHICLES/CAR.YTD", "**/vehicles/*.ydr")]
    [InlineData(false, "VEHICLES/FOO.YTD",              "vehicles/*.ydr")]
    [InlineData(false, "X64/DLCPACKS/PEDS/FOO.YDR",     "**/vehicles/*.ydr")]
    [InlineData(false, "PEDS/BAR.YDR",                  "**/vehicles/*.ydr")]
    // Single-char wildcard
    [InlineData(true,  "model1.ydr",  "model?.ydr")]
    [InlineData(true,  "modelA.ydr",  "model?.ydr")]
    [InlineData(false, "modelAB.ydr", "model?.ydr")]
    [InlineData(false, "model.ydr",   "model?.ydr")]
    [InlineData(true,  "a.ydr",       "?.ydr")]
    [InlineData(false, "ab.ydr",      "?.ydr")]
    // Single-char wildcard in path
    [InlineData(true,  "v1/car.ydr", "v?/*.ydr")]
    [InlineData(false, "vx/car.ytd", "v?/*.ydr")]
    // Standalone ** (no trailing /)
    [InlineData(true,  "a/b/c.ydr", "**.ydr")]
    [InlineData(true,  "c.ydr",     "**.ydr")]
    [InlineData(false, "a/b/c.ytd", "**.ydr")]
    // Path pattern matched at mid-path boundary
    [InlineData(true,  "x64/vehicles/car.ydr",    "vehicles/*.ydr")]
    [InlineData(true,  "a/b/vehicles/car.ydr",    "vehicles/*.ydr")]
    [InlineData(false, "x64/vehicles/car.ytd",    "vehicles/*.ydr")]
    [InlineData(false, "x64/notvehicles/car.ydr", "vehicles/*.ydr")]
    // Backslash normalized
    [InlineData(true,  "vehicles/car.ydr", "vehicles\\*.ydr")]
    [InlineData(false, "vehicles/car.ytd", "vehicles\\*.ydr")]
    // Multiple patterns
    [InlineData(true,  "model.ydr",        "vehicles/*.ydr", "*.ydr")]
    [InlineData(false, "model.ytd",        "vehicles/*.ydr", "*.ydr")]
    [InlineData(true,  "vehicles/car.ydr", "peds/*.ydr", "vehicles/*.ydr")]
    [InlineData(false, "vehicles/car.ytd", "peds/*.ydr", "vehicles/*.ydr")]
#pragma warning restore format
    public void Matches_ReturnsExpectedResult(bool expected, string path, params string[]? filters)
    {
        bool result = Filter.Matches(path, filters);
        Assert.Equal(expected, result);
    }
}
