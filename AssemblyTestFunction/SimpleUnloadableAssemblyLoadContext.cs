using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace AssemblyTestFunction
{
    public class SimpleUnloadableAssemblyLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver _resolver;

        public SimpleUnloadableAssemblyLoadContext()
        : base(isCollectible: true)
        {
            var dllPath = Assembly.GetExecutingAssembly().Location;
            var dllParentPath = Path.GetDirectoryName(dllPath);
            _resolver = new AssemblyDependencyResolver(dllParentPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }
            return null;
        }
    }
}

