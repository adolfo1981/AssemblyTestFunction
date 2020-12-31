using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class ItemPushed : IDomainEvent
    {
        public string PerformUid { get; set; }
        //public List<ValidationError> ValidationErrors { get; set; } = new List<ValidationError>();
        public string Error { get; set; }
        public Guid SagaId { get; set; }

        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }
        public PushResult PushResult { get; set; } = new PushResult();
        public bool? BypassGuidConversion { get; set; }
        public int ClientId { get; set; }
        public ItemPushed()
        {
            Id = Guid.NewGuid();
            DateTime = DateTime.UtcNow;
        }
    }
}
