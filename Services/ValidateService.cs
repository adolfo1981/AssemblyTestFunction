﻿#define TRACE
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

            List<object> events = new List<object>();
            var event1 = new Event1();
            var pushResult = new PushResult(){
                Result = new ClientTaxDto()
                };
            var itemPushed = new ItemPushed(){
                Error = "Test Error" 
            };
            itemPushed.PushResult = pushResult;
            event1.ItemPushed = itemPushed;
            var event2 = new Event2();
            events.Add(GetEventObject(itemPushed));
            events.Add(GetEventObject(event2));

            handler.PlayEvents(events.ToArray());

            return true;
        }

        public bool ValidatePhone(string phone)
        {
            _logger.LogInformation("PHONE VALID!");

            var handler = new Domain.EventHandler();

            List<object> events = new List<object>();
            var event1 = new Event1();
            var event2 = new Event2();
            events.Add(GetEventObject(event1));
            events.Add(GetEventObject(event2));

            handler.PlayEvents(events.ToArray());

            return true;
        }

        private object GetEventObject(IDomainEvent @event)
        {
            var json = JsonConvert.SerializeObject(@event, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
            var result = JsonConvert.DeserializeObject(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
            return result;
        }
    }
}
