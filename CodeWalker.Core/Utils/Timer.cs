using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace CodeWalker.Core.Utils
{
    public class DisposableTimer : Stopwatch, IDisposable, IResettable
    {
        public static event Action<TimeSpan, string> OnDispose;
        public Stopwatch Stopwatch => this;

        static DisposableTimer()
        {
#if DEBUG
            TimerStopped += (timeSpan, name) => Debug.WriteLine($"{name} took {timeSpan.TotalMilliseconds} ms");
#endif
            OnDispose += (timeSpan, name) => Console.WriteLine($"{name} took {timeSpan.TotalMilliseconds} ms");
        }

        public string Name { get; private set; }

        public DisposableTimer([CallerMemberName] string name = "") : base()
        {
            Start();
            Name = name;
        }

        public DisposableTimer(string name, bool start)
        {
            Name = name;
            if (start)
            {
                Start();
            }
        }

        public void Dispose()
        {
            Stop();
            OnDispose?.Invoke(Elapsed, Name ?? string.Empty);
            GC.SuppressFinalize(this);
        }

        public bool TryReset()
        {
            Reset();
            return true;
        }
    }

    public class SummableDisposableTimer : DisposableTimer
    {
        public event Action<TimeSpan, string> OnDispose;

        public SummableDisposableTimer([CallerMemberName] string name = "") : base(name)
        {
            
        }

        public void Dispose()
        {
            Stop();
        }
    }

    public class DisposableTimerSummed : IDisposable
    {
        private long _elapsed;
        public TimeSpan TimeSpan => new TimeSpan(_elapsed);

        public string Name { get; set; }

        public DisposableTimerSummed([CallerMemberName] string name = "")
        {
            Name = name;
        }

        public DisposableTimer GetTimer([CallerMemberName] string name = "")
        {
            var timer = new SummableDisposableTimer(name);
            timer.OnDispose += (time, _) =>
            {
                Interlocked.Add(ref _elapsed, time.Ticks);
            };
            return timer;
        }

        public void Dispose()
        {
            Console.WriteLine($"{Name} took {TimeSpan.TotalMilliseconds} ms");
            GC.SuppressFinalize(this);
        }
    }
}
