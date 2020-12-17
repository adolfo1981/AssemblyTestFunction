using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class ClientTaxDto : IPayrollDto
    {
        public string Uid => Id.ToString();
        public Guid Id { get; set; }
    }
}
