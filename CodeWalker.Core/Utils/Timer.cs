using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CodeWalker.Core.Utils
{
    public class DisposableTimer : IDisposable
    {
        public static event Action<TimeSpan, string> TimerStopped;
        public readonly Stopwatch _stopwatch;

        static DisposableTimer()
        {
#if DEBUG
            TimerStopped += (timeSpan, name) => Debug.WriteLine($"{name} took {timeSpan.TotalMilliseconds} ms");
#endif
            TimerStopped += (timeSpan, name) => Console.WriteLine($"{name} took {timeSpan.TotalMilliseconds} ms");
        }

        public string Name { get; private set; }

        public DisposableTimer(string name)
        {
            _stopwatch = Stopwatch.StartNew();
            Name = name;
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            TimerStopped?.Invoke(_stopwatch.Elapsed, Name ?? string.Empty);
        }
    }
}
