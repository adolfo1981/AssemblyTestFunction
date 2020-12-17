using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Domain
{
    public abstract class HandlerBase
    {
        public void PlayEvents(IEnumerable<object> events)
        {
            foreach(var @event in events)
            {
                PlayEvent(@event);                
            }
        }

        public void PlayEvent(object @event)
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName == "Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null").ToArray();

                //GOOD TEST OR BAD TEST
                var goodEventTest = false;

                //PICKING RANDOM ASSEMBLY TEST
                //var random = new Random();
                //int index = 0;
                //int max = assemblies.Count() > 0 ? assemblies.Count() -1 : 0;
                //int randomIndex = random.Next(max);
                //var testAssembly = assemblies[randomIndex];

                //var execAs = Assembly.GetExecutingAssembly();
                //var areEqual = execAs.Equals(this.GetType().Assembly);
                //var newEventType = this.GetType().Assembly.GetType(@event.GetType().ToString());
                //Getting type from wrong assembly
                //var newEventType = testAssembly.GetType(@event.GetType().ToString());
                //var method = GetType().GetMethod("Handle", new[] { newEventType }); ;

                if(goodEventTest)
                {
                //GOOD EVENT TEST
                    var goodEvent = assemblies.Count() > 1 ? GetEventFromRightAssembly(@event, assemblies) : @event;
                    var method = GetType().GetMethod("Handle", new[] { goodEvent.GetType() });
                    method.Invoke(this, new[] { goodEvent });
                    }else
                {
                //BAD EVENT TEST
                    var badEvent = assemblies.Count() > 1 ? GetEventFromWrongAssembly(@event, assemblies) : @event;
                    var method = GetType().GetMethod("Handle", new[] { badEvent.GetType() });
                    method.Invoke(this, new[] { badEvent });
                }

            }
            catch (Exception ex)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var domainDllCount = AppDomain.CurrentDomain.GetAssemblies().Count(x => x.FullName == "Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
                var message = $@"Error processing {@event.GetType()} for {this.GetType()} {Environment.NewLine}
                    **** In Exception - NUMBER OF Guided DLLs Loaded: {domainDllCount} ****";
                throw new ArgumentException(message, ex);
            }
        }

        private object GetEventFromWrongAssembly(object @event,Assembly[] assemblies)
        {
            var wrongAssembly = assemblies.Where(x => !x.Equals(Assembly.GetExecutingAssembly())).FirstOrDefault();
            var wrongType = wrongAssembly.GetType(@event.ToString());
            var json = SerializeHelper.SerializeWithTypeName(@event);
            var newEvent = SerializeHelper.Deserialize(json,wrongType);
            return newEvent;
        }

        private object GetEventFromRightAssembly(object @event,Assembly[] assemblies)
        {
            var goodAssembly = assemblies.Where(x => x.Equals(Assembly.GetExecutingAssembly())).FirstOrDefault();
            var goodType = goodAssembly.GetType(@event.ToString());
            var json = SerializeHelper.SerializeWithTypeName(@event);
            var newEvent = SerializeHelper.Deserialize(json,goodType);
            return newEvent;
        }

        private Assembly GetLoadedAssembly(Assembly assembly)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(x => assembly.Equals(x)).FirstOrDefault();
        }
    }
}
