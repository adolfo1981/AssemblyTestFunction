#define TRACE
using Domain;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Services
{
    public class ValidateService : IValidateService
    {
        private ILogger _logger;
        public ValidateService(ILogger<ValidateService> logger)
        {
            _logger = logger;
        }

        public bool ValidateName(string name)
        {
            _logger.LogInformation("NAME VALID!");

            var handler = new Domain.EventHandler();
            handler.Logger = _logger;

            List<object> events = new List<object>();
            var event1 = new Event1();
            var pushResult = new PushResult(){
                Result = new ClientTaxDto()
                };
            var itemPushed = new ItemPushed(){
                Error = "Test Error" 
            };

            var ede1 = EventHelper.GetEventDataEntity(itemPushed,Guid.Empty);

            itemPushed.PushResult = pushResult;
            event1.ItemPushed = itemPushed;
            var event2 = new Event2();
            var ede2 = EventHelper.GetEventDataEntity(event2, Guid.Empty);


            events.Add(EventHelper.GetObject(ede1));
            events.Add(EventHelper.GetObject(ede2));

            handler.PlayEvents(events.ToArray());

            return true;
        }

        public bool ValidatePhone(string phone)
        {
            _logger.LogInformation("PHONE VALID!");

            var handler = new Domain.EventHandler();
            handler.Logger = _logger;

            List<object> events = new List<object>();
            var event1 = new Event1();
            var ede1 = EventHelper.GetEventDataEntity(event1,Guid.Empty);
            var event2 = new Event2();
            var ede2 = EventHelper.GetEventDataEntity(event2,Guid.Empty);
            events.Add(EventHelper.GetObject(ede1));
            events.Add(EventHelper.GetObject(ede2));

            handler.PlayEvents(events.ToArray());

            return true;
        }
    }
}
