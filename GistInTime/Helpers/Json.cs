using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace GistInTime.Helpers
{
    public class Json
    {
        public static T Deserialize<T>(string json)
        {
            using (var memoryStream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                var settings = new DataContractJsonSerializerSettings();
                settings.UseSimpleDictionaryFormat = true;

                var serializer = new DataContractJsonSerializer(typeof(T), settings);

                T obj = (T)serializer.ReadObject(memoryStream);
                return obj;
            }
        }

        public static string Serialize<T>(T obj)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));

            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                byte[] json = ms.ToArray();
                return Encoding.UTF8.GetString(json, 0, json.Length);
            }
        }
    }
}
