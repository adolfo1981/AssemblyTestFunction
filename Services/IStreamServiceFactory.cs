using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public interface IStreamServiceFactory
    {
        IStreamService CreateStreamService(string aggregateRoot,
            bool createStream = true);
    }
}
