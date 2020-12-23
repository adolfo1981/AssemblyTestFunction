#define TRACE
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace AssemblyTestFunction
{
    public static class AssemblyLoadingHelper
    {
 
        ///<summary>
        /// Reads the "BindingRedirecs" field from the app settings and applies the redirection on the
        /// specified assemblies
        /// </summary>
        private static ILogger _logger;
        public static void ConfigureAssemblyLoading(ILogger logger)
        {
            _logger = logger;
            AppDomain currentDomain = AppDomain.CurrentDomain;
            AssemblyLoadContext.Default.Resolving += Default_Resolving;
            currentDomain.AssemblyLoad += new AssemblyLoadEventHandler(MyAssemblyLoadEventHandler);
            currentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            //PrintLoadedAssemblies(currentDomain);
            // Lists all five assemblies
        }

        private static Assembly Default_Resolving(AssemblyLoadContext context, AssemblyName name)
        {
            if (name.Name == "Domain")
            {
                var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName == "Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" &&
                x.Equals(Assembly.GetExecutingAssembly())).ToList();
                return domainAssemblies.FirstOrDefault();
            }
            return null;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            int x = 10;
            return null;
        }

        static void PrintLoadedAssemblies(AppDomain domain) {
          _logger.LogInformation("LOADED ASSEMBLIES:");
          foreach (Assembly a in domain.GetAssemblies()) {
             Trace.WriteLine(a.FullName);
          }
       }

       static void MyAssemblyLoadEventHandler(object sender, AssemblyLoadEventArgs args) {
          var assemblies = AppDomain.CurrentDomain.GetAssemblies();
          var domainDllCount = assemblies.Count(x => x.FullName == "Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
          if(args.LoadedAssembly.FullName == "Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")
          {
            _logger.LogInformation($"DOMAIN ASSEMBLY LOADED: {args.LoadedAssembly.FullName}");
          }
          _logger.LogInformation($"DOMAIN ASSEMBLIES LOADED: {domainDllCount}");
       }
    }
}
