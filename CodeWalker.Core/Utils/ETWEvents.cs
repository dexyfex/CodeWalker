using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Core.Utils;

[EventSource(Name = "CodeWalker-Diagnostics", Guid = "911cf260-f98c-5a05-7f16-f11db360be50")]
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


    private ETWEvents(bool throwOnEventWriteErrors) : base(throwOnEventWriteErrors)
    { }

    [Event(1, Message = "Starting up.", Keywords = Keywords.Performance, Level = EventLevel.Informational)]
    public void Startup() {
        WriteEvent(1);
    }

    [Event(2, Keywords = Keywords.Performance | Keywords.StateChanges, Level = EventLevel.Verbose)]
    public void CreatingFormStart(string form) { WriteEvent(2, form); }
    [Event(3, Keywords = Keywords.Performance | Keywords.StateChanges, Level = EventLevel.Verbose)]
    public void CreatingFormStop() { WriteEvent(3); }

    [Event(4, Keywords = Keywords.Performance | Keywords.StateChanges, Level = EventLevel.Verbose)]
    public void LoadingForm(string form) { WriteEvent(4, form); }

    [Event(5, Keywords = Keywords.Performance | Keywords.StateChanges, Level = EventLevel.Verbose)]
    public void RefreshingMainTreeViewStart(string path) { WriteEvent(5, path); }
    [Event(6, Keywords = Keywords.Performance | Keywords.StateChanges, Level = EventLevel.Verbose)]
    public void RefreshingMainTreeViewStop() { WriteEvent(6); }

    [Event(7, Keywords = Keywords.Performance | Keywords.StateChanges, Level = EventLevel.Verbose)]
    public void RefreshMainTreeViewStart() { WriteEvent(7); }
    [Event(8, Keywords = Keywords.Performance | Keywords.StateChanges, Level = EventLevel.Verbose)]
    public void RefreshMainTreeViewStop() { WriteEvent(8); }

    [Event(9, Keywords = Keywords.Performance | Keywords.StateChanges, Level = EventLevel.Verbose)]
    public void InitFileCacheStart() {
        WriteEvent(9);
    }
    [Event(10, Keywords = Keywords.Performance | Keywords.StateChanges, Level = EventLevel.Verbose)]
    public void InitFileCacheStop() { WriteEvent(10); }



    public static readonly ETWEvents Log = new ETWEvents(true);
}
