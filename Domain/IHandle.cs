using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public interface IHandle<Event>
    {
        void Handle(Event message);
    }
}
