#define TRACE
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Domain
{
    public class EventHandler : TypedEventModel, 
                    IHandle<ItemPushed>, 
                    IHandle<Event2>
    {
        public override string StreamName => throw new NotImplementedException();

        public void Handle(ItemPushed message)
        {
            Trace.WriteLine("Calling Handle for ItemPushed event..");
        }

        public void Handle(Event2 message)
        {
            Trace.WriteLine("Calling Handle for Event 2..");
        }

        protected override void CallHandle(string typeName, string json)
        {
            switch(typeName)
            {
                case nameof(ItemPushed):
                     Handle(SerializeHelper.DeserializeWithTypeName<ItemPushed>(json));
                    break;
                case nameof(Event2):
                    Handle(JsonConvert.DeserializeObject<Event2>(json));
                    break;
            }
        }
    }
}
