using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Core.Utils;

[EventSource(Name = "CodeWalker-Diagnostics")]
public class ETWEvents : EventSource
{
    public static class Keywords
    {
        public const EventKeywords ComponentLifespan = (EventKeywords)1;
        public const EventKeywords StateChanges = (EventKeywords)(1 << 1);
        public const EventKeywords Performance = (EventKeywords)(1 << 2);
        public const EventKeywords DumpState = (EventKeywords)(1 << 3);
        public const EventKeywords StateTracking = (EventKeywords)(1 << 4);
    }
    internal static class DebugCounters
    {

    }


    public ETWEvents(bool throwOnEventWriteErrors) : base(throwOnEventWriteErrors)
    { }

    [Event(1, Message = "Starting up.", Keywords = Keywords.Performance, Level = EventLevel.Informational)]
    public void Startup() {
        WriteEvent(1);
    }

    [Event(2, Message = "Creating form {0}", Keywords = Keywords.Performance | Keywords.StateChanges, Level = EventLevel.Verbose)]
    public void CreatingForm(string form) { WriteEvent(2, form); }

    [Event(3, Message = "Loading form {0}", Keywords = Keywords.Performance | Keywords.StateChanges, Level = EventLevel.Verbose)]
    public void LoadingForm(string form) { WriteEvent(3, form); }

    public static readonly ETWEvents Log = new ETWEvents(true);
}
