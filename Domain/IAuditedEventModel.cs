using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public interface IAuditableAggregate
    {        
        string AuditName { get; }
        string CreatedBy { get; set; }
        Guid AuditEntityId { get; }
    }

    public interface IAuditableEvent : IAuditedEventModel
    {        
        Guid Id { get; set; }
        AuditRecord CreateAuditRecord();
    }

    public interface IAuditedEventModel : IDomainEvent
    {
        Guid? UserId { get; set; }
        string UserName { get; set; }
        string UserFirstName { get; set; }
        string UserLastName { get; set; }
        bool IsPaycor { get; set; }
        string IpAddress { get; set; }
    }

    public interface IFileAuditedEventModel
    {
        Guid? FileId { get; set; }
        int? FileVersion { get; set; }
        string FileName { get; set; }
        DateTime? FileUploadDateTime { get; set; }
        string FileUploadedByUser { get; set; }
    }
}
