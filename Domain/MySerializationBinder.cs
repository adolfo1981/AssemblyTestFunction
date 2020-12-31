using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Domain
{
    public class MySerializationBinder : ISerializationBinder
    {
        public IList<Type> KnownTypes { get; set; }

        public Type BindToType(string assemblyName, string typeName)
        {
            //var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName == "Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null").ToList();
            //Assembly goodAssembly = null;
            //foreach(var ass in domainAssemblies)
            //{
            //    var areEqual = Assembly.GetExecutingAssembly().Equals(ass);
            //    if(areEqual)
            //    {
            //        goodAssembly = ass;
            //        break;
            //    }
            //}
            //var type = KnownTypes.SingleOrDefault(t => t.FullName == typeName);
            //var areEqual = type.Assembly.Equals(Assembly.GetExecutingAssembly());
            //return type;

            var type = Type.GetType(typeName);
            return type;
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }
    }
}
