using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security;

namespace CodeWalker.Cli.Helpers;

/// <summary>
/// Displays a console progress bar on stderr to keep stdout clean for data/JSON output.
/// </summary>
internal sealed class ProgressBar : IDisposable
{
    /// <summary>Default writer when no custom writer is provided. Uses stderr to allow console control.</summary>
    private static TextWriter DefaultWriter => Console.Error;
    /// <summary>Detect if the default console writer is redirected, in which case we disable the progress bar to avoid writing control characters to the output.</summary>
    private static bool IsDefaultWriterRedirected => Console.IsErrorRedirected;

    /// <summary>Minimum milliseconds between render updates to prevent flickering.</summary>
    private const int ThrottleMs = 50;
    /// <summary>Character width of the <c>[===&gt;   ]</c> bar portion.</summary>
    private const int BarWidth = 40;

    /// <summary>Total number of items to process.</summary>
    private readonly int _total;
    /// <summary>Output destination (stderr or a caller-supplied writer).</summary>
    private readonly TextWriter _writer;
    /// <summary>Whether this instance owns the console (true when no custom writer was provided).</summary>
    private readonly bool _ownsConsole;
    /// <summary>Terminal width used for padding and line clearing.</summary>
    private readonly int _windowWidth;
    /// <summary>Monotonic timer for throttling render updates.</summary>
    private readonly Stopwatch _throttle = new();
    /// <summary>Guards all mutable state for thread-safe updates.</summary>
    private readonly object _lock = new();
    /// <summary>Tracks whether <see cref="Dispose"/> has been called.</summary>
    private bool _disposed;

    internal int Current { get; private set; }
    internal bool Enabled { get; }

    internal void ResetThrottle()
    {
        lock (this._lock)
            this._throttle.Reset();
    }

    /// <summary>
    /// Initializes a new instance of the ProgressBar class.
    /// </summary>
    /// <param name="total">Total number of items to process.</param>
    /// <param name="enabled">Whether to enable the progress bar display.</param>
    /// <param name="writer">Optional text writer for output. When null, writes to stderr with console cursor control.</param>
    /// <param name="windowWidth">Terminal width used for padding and truncation when a custom writer is provided.</param>
    public ProgressBar(int total, bool enabled, TextWriter? writer = null, int windowWidth = 120)
    {
        this._total = total;
        this._writer = writer ?? DefaultWriter;
        this._ownsConsole = this._writer == Console.Error || this._writer == Console.Out;
        this._windowWidth = windowWidth;
        this.Enabled = enabled && total > 0 && (!this._ownsConsole || !IsDefaultWriterRedirected);
        if (this.Enabled)
        {
            if (this._ownsConsole)
            {
                try
                {
                    Console.CursorVisible = false;
                }
                catch
                {
                    // Ignore console errors (e.g. redirected output, no terminal)
                    this._ownsConsole = false;
                }
            }
            this.Render();
            this._throttle.Start();
        }
    }

    /// <summary>
    /// Updates the progress bar to the specified current value.
    /// </summary>
    /// <param name="current">Current number of items processed.</param>
    /// <param name="currentFile">Optional current file being processed.</param>
    /// <remarks>
    /// Must be called under <c>_lock</c> to ensure thread safety with Increment and Dispose.
    /// </remarks>
    private void Update(int current, string? currentFile = null)
    {
        this.Current = Math.Max(0, Math.Min(current, this._total));
        if (!this.Enabled)
            return;

        // Throttle updates to avoid flickering
        if (this._throttle is { IsRunning: true, ElapsedMilliseconds: < ThrottleMs } && current < this._total)
            return;

        this._throttle.Restart();
        this.Render(currentFile);
    }

    /// <summary>
    /// Increments the progress bar by one.
    /// </summary>
    /// <param name="currentFile">Optional current file being processed.</param>
    /// <remarks>
    /// Thread-safe: the increment and render happen atomically under a lock.
    /// </remarks>
    public void Increment(string? currentFile = null)
    {
        lock (this._lock)
        {
            if (this.Current >= this._total) return;
            this.Update(this.Current + 1, currentFile);
        }
    }

    /// <summary>
    /// Writes the progress bar line to <see cref="_writer"/>, overwriting the current console line.
    /// </summary>
    /// <param name="currentFile">Optional filename appended after the percentage stats.</param>
    private void Render(string? currentFile = null)
    {
        if (!this.Enabled || this._disposed)
            return;

        try
        {
            double percent = this._total > 0 ? (double)this.Current / this._total : 0;
            int filled = Math.Min((int)(percent * BarWidth), BarWidth);
            int winWidth = this._ownsConsole ? Console.WindowWidth : this._windowWidth;
            int maxWidth = Math.Max(1, winWidth - 1);

            // Build the full line as a single string: [====>    ] 100 % (50/100) file.ytd
            string line = filled < BarWidth
                ? $"[{new string('=', filled)}>{new string(' ', BarWidth - filled - 1)}"
                : $"[{new string('=', filled)}";

            line += string.Format(CultureInfo.InvariantCulture, "] {0,6:P0} ({1}/{2})", percent, this.Current, this._total);

            if (!string.IsNullOrEmpty(currentFile))
            {
                int maxLen = Math.Max(10, winWidth - BarWidth - 30);
                string displayFile =
                    currentFile!.Length > maxLen
                        ? $"...{currentFile[(currentFile.Length - maxLen + 3)..]}"
                        : currentFile;
                line += $" {displayFile}";
            }

            // Clamp to terminal width to prevent wrapping; pad remainder to overwrite stale characters
            if (line.Length > maxWidth)
                line = line[..maxWidth];
            else if (line.Length < maxWidth)
                line += new string(' ', maxWidth - line.Length);

            if (this._ownsConsole)
                Console.SetCursorPosition(0, Console.CursorTop);

            this._writer.Write(line);
        }
        catch (Exception ex)
            when (ex is IOException or InvalidOperationException or SecurityException)
        {
            // Ignore console errors (e.g. redirected output, no terminal)
        }
    }

    /// <summary>
    /// Disposes the progress bar, ensuring the console state is restored.
    /// </summary>
    public void Dispose()
    {
        lock (this._lock)
        {
            if (this._disposed || !this.Enabled)
                return;
            this._disposed = true;

            try
            {
                this._writer.WriteLine();
                if (this._ownsConsole)
                    Console.CursorVisible = true;
            }
            catch (Exception ex)
                when (ex is IOException or InvalidOperationException or SecurityException)
            {
                // Ignore console errors during dispose
            }
        }
    }
}
