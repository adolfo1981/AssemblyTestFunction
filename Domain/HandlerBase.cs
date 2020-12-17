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
                //var test = GetLoadedAssembly(this.GetType().Assembly);
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName == "Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null").ToArray();
                var random = new Random();
                int index = 0;
                int max = assemblies.Count() > 0 ? assemblies.Count() -1 : 0;
                int randomIndex = random.Next(max);
                var testAssembly = assemblies[randomIndex];

                var execAs = Assembly.GetExecutingAssembly();
                var areEqual = execAs.Equals(this.GetType().Assembly);

                //var newEventType = this.GetType().Assembly.GetType(@event.GetType().ToString());
                var newEventType = testAssembly.GetType(@event.GetType().ToString());
                var method = GetType().GetMethod("Handle", new[] { newEventType }); ;
                method.Invoke(this, new[] { @event });
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

        private Assembly GetLoadedAssembly(Assembly assembly)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(x => assembly.Equals(x)).FirstOrDefault();
        }
    }
}
