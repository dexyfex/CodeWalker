using System.IO;
using System.Threading.Tasks;

using CodeWalker.Cli.Helpers;

using Xunit;

namespace CodeWalker.Cli.Tests.Helpers;

public sealed class ProgressBarTests
{
    // Helper

    /// <summary>Increments the bar <paramref name="count"/> times, optionally passing a file on the last call.</summary>
    private static void IncrementTo(ProgressBar bar, int count, string? lastFile = null)
    {
        for (int i = 1; i < count; i++)
            bar.Increment();
        if (count > 0)
            bar.Increment(lastFile);
    }

    // Disabled-state tests

    [Fact]
    public void Constructor_disabled_when_enabled_is_false()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(100, enabled: false, sw);
        Assert.False(bar.Enabled);
        Assert.Equal("", sw.ToString());
    }

    [Fact]
    public void Constructor_disabled_when_total_is_zero()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(0, enabled: true, sw);
        Assert.False(bar.Enabled);
    }

    [Fact]
    public void Constructor_disabled_when_total_is_negative()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(-5, enabled: true, sw);
        Assert.False(bar.Enabled);
    }

    // Enabled-state tests

    [Fact]
    public void Constructor_enabled_with_custom_writer()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(100, enabled: true, sw);
        Assert.True(bar.Enabled);
    }

    [Fact]
    public void Constructor_renders_initial_zero_percent()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(100, enabled: true, sw, windowWidth: 120);
        string output = sw.ToString();
        Assert.StartsWith("[", output);
        Assert.Contains("(0/100)", output);
        Assert.Contains(">", output); // cursor indicator at start
    }

    // Render format tests

    [Fact]
    public void Render_at_50_percent_has_half_filled_bar()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(100, enabled: true, sw, windowWidth: 120);
        // Increment to 49 (throttled, no renders)
        IncrementTo(bar, 49);
        _ = sw.GetStringBuilder().Clear();
        bar.ResetThrottle();
        bar.Increment(); // 50th — renders after throttle reset
        string output = sw.ToString();
        // 50% => filled = (int)(0.5 * 40) = 20
        Assert.Contains(new string('=', 20) + ">", output);
        Assert.Contains("(50/100)", output);
    }

    [Fact]
    public void Render_at_100_percent_has_full_bar_no_cursor()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(10, enabled: true, sw, windowWidth: 120);
        _ = sw.GetStringBuilder().Clear();
        IncrementTo(bar, 10); // == total, bypasses throttle
        string output = sw.ToString();
        Assert.Contains(new string('=', 40) + "]", output);
        Assert.DoesNotContain(">", output);
        Assert.Contains("(10/10)", output);
    }

    // File name tests

    [Fact]
    public void Render_shows_current_file()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(10, enabled: true, sw, windowWidth: 120);
        _ = sw.GetStringBuilder().Clear();
        IncrementTo(bar, 10, "textures/player.ytd"); // bypasses throttle at total
        string output = sw.ToString();
        Assert.Contains("textures/player.ytd", output);
    }

    [Fact]
    public void Render_truncates_long_file_with_ellipsis()
    {
        StringWriter sw = new();
        // windowWidth=80 -> maxLen = Max(10, 80-40-30) = 10
        using ProgressBar bar = new(10, enabled: true, sw, windowWidth: 80);
        _ = sw.GetStringBuilder().Clear();
        IncrementTo(bar, 10, "very/long/path/to/some/deeply/nested/file.ytd");
        string output = sw.ToString();
        Assert.Contains("...", output);
        Assert.DoesNotContain("very/long/path", output);
    }

    [Fact]
    public void Increment_renders_file_name()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(10, enabled: true, sw, windowWidth: 120);
        _ = sw.GetStringBuilder().Clear();
        // Increment 10 times to hit total (bypasses throttle)
        for (int i = 0; i < 9; i++)
            bar.Increment();
        _ = sw.GetStringBuilder().Clear();
        bar.Increment("models/vehicle.yft");
        string output = sw.ToString();
        Assert.Contains("models/vehicle.yft", output);
        Assert.Contains("(10/10)", output);
    }

    [Fact]
    public void Render_shows_short_file_without_truncation()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(10, enabled: true, sw, windowWidth: 200);
        _ = sw.GetStringBuilder().Clear();
        IncrementTo(bar, 10, "short.ytd");
        string output = sw.ToString();
        Assert.Contains("short.ytd", output);
        Assert.DoesNotContain("...", output);
    }

    // State tracking tests

    [Fact]
    public void Increment_advances_by_one()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(100, enabled: true, sw);
        bar.Increment();
        bar.Increment();
        bar.Increment();
        Assert.Equal(3, bar.Current);
    }

    // Throttle tests

    [Fact]
    public void Throttle_skips_rapid_increments()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(100, enabled: true, sw, windowWidth: 80);
        _ = sw.GetStringBuilder().Clear();
        // Rapid increments within the 50ms throttle window - none should render
        for (int i = 1; i <= 50; i++)
            bar.Increment();
        string output = sw.ToString();
        Assert.Equal("", output);
    }

    [Fact]
    public void Throttle_bypassed_at_total()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(10, enabled: true, sw, windowWidth: 80);
        _ = sw.GetStringBuilder().Clear();
        // Increment to total always renders even within throttle window
        IncrementTo(bar, 10);
        Assert.Contains("(10/10)", sw.ToString());
    }

    [Fact]
    public void Throttle_reset_allows_render()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(100, enabled: true, sw, windowWidth: 80);
        _ = sw.GetStringBuilder().Clear();
        bar.ResetThrottle();
        bar.Increment();
        Assert.Contains("(1/100)", sw.ToString());
    }

    // Dispose tests

    [Fact]
    public void Dispose_writes_newline_when_enabled()
    {
        StringWriter sw = new();
        ProgressBar bar = new(10, enabled: true, sw, windowWidth: 80);
        bar.Dispose();
        Assert.EndsWith(sw.NewLine, sw.ToString());
    }

    [Fact]
    public void Dispose_does_not_throw_when_disabled()
    {
        ProgressBar bar = new(10, enabled: false, new StringWriter());
        bar.Dispose();
    }

    [Fact]
    public void Dispose_can_be_called_multiple_times()
    {
        ProgressBar bar = new(10, enabled: true, new StringWriter());
        bar.Dispose();
        bar.Dispose();
    }

    // Thread safety tests

    [Fact]
    public void Concurrent_increments_are_thread_safe()
    {
        const int total = 10_000;
        StringWriter sw = new();
        ProgressBar bar = new(total, enabled: true, sw);

        _ = Parallel.For(0, total, _ => bar.Increment());

        Assert.Equal(total, bar.Current);
    }

    // Disabled-state mutation tests

    [Fact]
    public void Increment_on_disabled_bar_writes_nothing()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(100, enabled: false, sw);
        bar.Increment();
        Assert.Equal("", sw.ToString());
    }

    // Clamping tests

    [Fact]
    public void Increment_clamps_current_at_total()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(3, enabled: true, sw);
        for (int i = 0; i < 10; i++)
            bar.Increment();
        Assert.Equal(3, bar.Current);
    }

    // Render exception handling tests

    [Fact]
    public void Render_swallows_IOException_from_writer()
    {
        ThrowingWriter tw = new();
        using ProgressBar bar = new(10, enabled: true, tw, windowWidth: 80);
        // Constructor render hit the throwing writer and didn't propagate
        // Further increments should also not throw
        IncrementTo(bar, 10);
    }

    private sealed class ThrowingWriter : StringWriter
    {
        public override void Write(string? value) => throw new IOException("simulated");
    }

    // Dispose idempotency tests

    [Fact]
    public void Dispose_writes_exactly_one_newline()
    {
        StringWriter sw = new();
        ProgressBar bar = new(10, enabled: true, sw, windowWidth: 80);
        _ = sw.GetStringBuilder().Clear();
        bar.Dispose();
        bar.Dispose();
        bar.Dispose();
        // Only one newline despite three Dispose calls
        Assert.Equal(sw.NewLine, sw.ToString());
    }

    // Full lifecycle test

    [Fact]
    public void Full_lifecycle_renders_progress_to_completion()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(5, enabled: true, sw, windowWidth: 120);
        for (int i = 0; i < 5; i++)
        {
            bar.ResetThrottle();
            bar.Increment($"step_{i}");
        }
        string output = sw.ToString();
        Assert.Contains("(5/5)", output);
        Assert.Equal(5, bar.Current);
    }

    // Edge case tests

    [Fact]
    public void Increment_with_empty_file_name_does_not_display_file()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(10, enabled: true, sw, windowWidth: 120);
        _ = sw.GetStringBuilder().Clear();
        IncrementTo(bar, 10, "");
        string output = sw.ToString();
        Assert.Contains("(10/10)", output);
        // Empty file name should not add extra content between stats and padding
        Assert.DoesNotContain("...", output);
    }

    [Fact]
    public void Render_at_narrow_window_truncates_to_fit()
    {
        StringWriter sw = new();
        using ProgressBar bar = new(10, enabled: true, sw, windowWidth: 30);
        _ = sw.GetStringBuilder().Clear();
        IncrementTo(bar, 10, "some/path/to/file.ytd");
        string output = sw.ToString();
        Assert.NotEmpty(output);
        // Output must be clamped to windowWidth - 1 to prevent wrapping
        Assert.True(output.Length <= 29, $"Output ({output.Length} chars) should not exceed window width - 1 (29)");
    }
}
