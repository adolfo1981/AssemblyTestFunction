using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class Event2 : IDomainEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public string Name = "Event2";
    }
}
