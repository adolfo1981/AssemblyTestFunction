using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class AuditRecord : TableEntity
    {
        public AuditRecord()
        {

        }

        public AuditRecord(Guid entityId, DateTime eventDateTime, Guid id, string propertyName)
        {
            PartitionKey = entityId.ToString();
            EntityId = entityId;
            RowKey = $"{eventDateTime.Ticks}-{id}-{propertyName}";
            EventId = id;
        }

		public Guid? EntityId {get; set; }
        public Guid ProspectId { get; set; }
        public Guid EventId { get; set; }
        public string EntityType { get; set; }

        /// <summary>
        /// Human readable Id (Code, Tax, etc)
        /// </summary>
        public string AuditName { get; set; }

        public string Message { get; set; }
        public bool IncludePropertyDifferences { get; set; }
        public string PreviousValue { get; set; }
        public string NextValue { get; set; }

        // info
        public DateTime EventDateTime { get; set; }
        public Guid? FileId { get; set; }
        public int? FileVersion { get; set; }
        public string FileName { get; set; }
        public DateTime? FileUploadDateTime { get; set; }
        public Guid? UserId { get; set; }
        public string UserName { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public bool IsPaycor { get; set; }
        public string IpAddress { get; set; }
        public int Version { get; set; }
        public string PropertyName { get; set; }

        public string EDTCode { get; set; }

        public string PayRateSequence { get; set; }
        public string DDTypePlus4 { get; set; }
        public string CreatedBy { get; set; }

        //Details stored as JSON
        public string Details { get; set; }
    }
}
