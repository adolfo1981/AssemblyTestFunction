using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public interface IDomainEvent
    {
        Guid Id { get; set; }
        DateTime DateTime { get; set; }
    }
}
