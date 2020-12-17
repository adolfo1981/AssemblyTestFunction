using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Domain
{
    public static class SerializeHelper
    {
        public static MemoryStream SerializeToMemeoryStream(object item)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(item)));
        }

        public static string SerializeWithTypeName(object item)
        {
            return JsonConvert.SerializeObject(item, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }

        public static object Deserialize(string body)
        {
            return JsonConvert.DeserializeObject(body, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }

        public static object Deserialize(string body, Type type)
        {
            return JsonConvert.DeserializeObject(body, type, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }

        public static object Deserialize<T>(string body)
        {
            return JsonConvert.DeserializeObject<T>(body);
        }
    }
}
