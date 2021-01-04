using Newtonsoft.Json;
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
        protected abstract void CallHandle(string typeName, string json);

        public void PlayEvent(object @event)
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName == "Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null").ToArray();

                //REALISTIC TEST
                //RealisticTest(assemblies, @event);
                //GOOD EVENT TEST
                //GoodEventTest(assemblies, @event);
                //BAD EVENT TEST
                BadEventTest(assemblies, @event);

                //var json = SerializeHelper.SerializeWithTypeName(@event);
                //CallHandle(@event.GetType().Name, json);0

                //var method = GetType().GetMethod("Handle", new[] { @event.GetType() });
                //method.Invoke(this, new[] { @event });
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

        private void RealisticTest(Assembly[] assemblies, object @event)
        {
            var random = new Random();
            int index = 0;
            int max = assemblies.Count() > 0 ? assemblies.Count() - 1 : 0;
            int randomIndex = random.Next(max);
            var randomAssembly = assemblies[randomIndex];
            var executingAsssembly = Assembly.GetExecutingAssembly();
            var areEqual = executingAsssembly.Equals(randomAssembly);
            Console.WriteLine($"Random Assembly and Executing Assembly Equal: {areEqual}");

            //Getting type from wrong assembly
            var newEventType = randomAssembly.GetType(@event.GetType().ToString());
            var method = GetType().GetMethod("Handle", new[] { newEventType }); 
            method.Invoke(this, new[] { @event });
        }

        private void GoodEventTest(Assembly[] assemblies, object @event)
        {
            var goodEvent = assemblies.Count() > 1 ? GetEventFromRightAssembly(@event, assemblies) : @event;
            var method = GetType().GetMethod("Handle", new[] { goodEvent.GetType() });
            method.Invoke(this, new[] { goodEvent });
            //((dynamic)this).Handle((dynamic)@event);
        }

        private void BadEventTest(Assembly[] assemblies, object @event)
        { 
            //var badEvent = assemblies.Count() > 1 ? GetEventFromWrongAssembly(@event, assemblies) : @event;
            //var method = GetType().GetMethod("Handle", new[] { badEvent.GetType() });
            var method = GetType().GetMethod("Handle", new[] { @event.GetType() });
            method.Invoke(this, new[] { @event });
        
        }

        private object GetEventFromWrongAssembly(object @event,Assembly[] assemblies)
        {
            var wrongAssembly = assemblies.Where(x => !x.Equals(Assembly.GetExecutingAssembly())).FirstOrDefault();
            var wrongType = wrongAssembly.GetType(@event.ToString());

            var json = SerializeHelper.SerializeWithTypeName(@event);
            //var newEvent = SerializeHelper.Deserialize(json,wrongType);

            MySerializationBinder sb = new MySerializationBinder();
            sb.KnownTypes = Assembly.GetExecutingAssembly().GetTypes();


            var newEvent = JsonConvert.DeserializeObject(json, wrongType, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                //SerializationBinder = sb,
                //TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full
                //TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                //Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                //{
                //    //errors.Add(args.ErrorContext.Error.Message);
                //    args.ErrorContext.Handled = true;
                //}
            });

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
