using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CodeWalker.Core.Utils
{
    public class DisposableTimer : IDisposable
    {
        public static event Action<TimeSpan, string> TimerStopped;
        public Stopwatch Stopwatch { get; init; }

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
            Stopwatch = Stopwatch.StartNew();
            Name = name;
        }
        public DisposableTimer(string name, bool start)
        {
            Name = name;
            if (start)
            {
                Stopwatch = Stopwatch.StartNew();
            } else
            {
                Stopwatch = new Stopwatch();
            }
        }

        public void Dispose()
        {
            Stopwatch.Stop();
            TimerStopped?.Invoke(Stopwatch.Elapsed, Name ?? string.Empty);
        }
    }
}
