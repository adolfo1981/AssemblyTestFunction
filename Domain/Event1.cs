using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class Event1 : IDomainEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public string Name = "Event1";
        public PushResult PushResult { get; set; } = new PushResult();
    }
}
