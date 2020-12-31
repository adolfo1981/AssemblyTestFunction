using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public class ValidateClientCommand
    {
        public int ClientId { get; set; }
        public Guid ProspectId { get; set; }
        public Guid SagaId { get; set; }
        public int ProcessGroupNumber { get; set; }
    }
}
