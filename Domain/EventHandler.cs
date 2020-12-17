#define TRACE
using System;
using System.Diagnostics;

namespace Domain
{
    public class EventHandler : HandlerBase, 
                    IHandle<Event1>, 
                    IHandle<Event2>
    {
        public void Handle(Event1 message)
        {
            Trace.WriteLine("Calling Handle for Event 1..");
        }

        public void Handle(Event2 message)
        {
            Trace.WriteLine("Calling Handle for Event 2..");
        }
    }
}
