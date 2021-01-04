using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class EventDataEntity
    {
        public Guid Id { get; set; }
        public string TypeFullName { get; set; }
        public string Data { get; set; }
        public DateTime DateTime { get; set; }
        public int Version { get; set; }
        public Guid ProspectId { get; set; }
    }
    
    public class EventDataEntityTable : TableEntity
    {
        public Guid Id { get; set; }
        public string TypeFullName { get; set; }
        public string Data { get; set; }
        public DateTime DateTime { get; set; }
        public int Version { get; set; }
    }
}
