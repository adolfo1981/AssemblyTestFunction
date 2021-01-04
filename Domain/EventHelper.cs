using Newtonsoft.Json;
using Streamstone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Domain
{
public static class EventHelper
    {
        public static EventData Event(IDomainEvent e, Guid prospectId)
        {
            var id = Guid.NewGuid();

            var properties = new EventDataEntity
            {
                Id = id,
                TypeFullName = e.GetType().AssemblyQualifiedName,
                Data = JSON(e),
                DateTime = DateTime.UtcNow,
                ProspectId = prospectId
            };

            return new EventData(EventId.From(id), EventProperties.From(properties));
        }

        public static EventDataEntity GetEventDataEntity(IDomainEvent e, Guid prospectId)
        {
            var id = Guid.NewGuid();

            var properties = new EventDataEntity
            {
                Id = id,
                TypeFullName = e.GetType().AssemblyQualifiedName,
                Data = JSON(e),
                DateTime = DateTime.UtcNow,
                ProspectId = prospectId
            };
            return properties;
        }

        public static object GetObject(EventDataEntity @event)
        {
            try
            {
                var newObject = SerializeHelper.Deserialize(@event.Data);
                return newObject;
            }
            catch
            {
                return JsonConvert.DeserializeObject<object>(@event.Data);
            }
        }

        private static string JSON(IDomainEvent domainEvent)
        {
            return SerializeHelper.SerializeWithTypeName(domainEvent);
        }

        public static IEnumerable<object> GetObjects(EventDataEntity[] sliceEvents)
        {
            return sliceEvents.Select(GetObject);
        }

        public static Dictionary<Guid, List<string>> DeferredEventData(Dictionary<Guid, List<IAuditedEventModel>> events)
        {
            var result = new Dictionary<Guid, List<string>>();

            events.ToList().ForEach(e =>
            {
                var listOfEvents = e.Value.Select(SerializeHelper.SerializeWithTypeName);
                result.Add(e.Key, listOfEvents.ToList());
            });

            return result;
        }
        
        public static Dictionary<Guid, List<IAuditedEventModel>> GetDefferedObjects(Dictionary<Guid, List<string>> events)
        {
            var result = new Dictionary<Guid, List<IAuditedEventModel>>();

            events.ToList().ForEach(e =>
            {
                var listOfEvents = e.Value.Select(SerializeHelper.Deserialize).ToList();
                var listOfAudited = listOfEvents.Select(a => (IAuditedEventModel) a).ToList();
                result.Add(e.Key, listOfAudited);
            });

            return result;
        }
    }
}
