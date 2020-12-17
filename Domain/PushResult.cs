using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class PushResult
    {
        public string BodyJson { get; set; }
        public IPayrollDto Result { get; set; }
        public bool LeaveEntityUnlocked { get; set; }
    }
}
