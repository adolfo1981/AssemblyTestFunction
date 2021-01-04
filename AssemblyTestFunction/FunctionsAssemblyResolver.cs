using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using Domain;
using System.Runtime.Loader;

namespace AssemblyTestFunction
{
public class FunctionsAssemblyResolver
{
    public static void RedirectAssembly()
    {
        var type = typeof(HandlerBase);
        //var list = AppDomain.CurrentDomain.GetAssemblies().OrderByDescending(a => a.FullName).Select(a => a.FullName).ToList();
        var list = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name == "Domain").Select(a => a.FullName).ToList();
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        //AssemblyLoadContext.Default.Resolving += Default_Resolving; ;
    }

        //private static Assembly Default_Resolving(AssemblyLoadContext arg1, AssemblyName arg2)
        //{
        //    var requestedAssembly = new AssemblyName(arg2.Name);
        //    Assembly assembly = null;
        //    AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        //    try
        //    {
        //        assembly = Assembly.Load(requestedAssembly.Name);
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        //    return assembly;
        //}

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        var requestedAssembly = new AssemblyName(args.Name);
        Assembly assembly = null;
        AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        try
        {
            assembly = Assembly.Load(requestedAssembly.Name);
        }
        catch (Exception ex)
        {
        }
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        return assembly;
    }

}
}
