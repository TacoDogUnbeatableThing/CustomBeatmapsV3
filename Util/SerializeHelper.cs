using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CustomBeatmaps.Util
{
    /// <summary>
    /// Helps save/load serialized data 
    /// </summary>
    public static class SerializeHelper
    {
        private static ISerializer _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        private static IDeserializer _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        public static string SerializeYAML<T>(T data)
        {
            return _serializer.Serialize(data);
        }

        public static T DeserializeYAML<T>(string serialized)
        {
            return _deserializer.Deserialize<T>(serialized);
        }

        public static void SaveYAML<T>(string filePath, T data)
        {
            File.WriteAllText(filePath, SerializeYAML(data));
        }

        public static T LoadYAML<T>(string filePath)
        {
            return DeserializeYAML<T>(File.ReadAllText(filePath));
        }

        private static async Task<T> DeserializeJSONAsync<T>(TextReader reader)
        {
            JsonReader jreader = new JsonTextReader(reader);
            await jreader.ReadAsync();
            JsonSerializer serializer = new JsonSerializer();
            return serializer.Deserialize<T>(jreader);
        }
        public static async Task<T> DeserializeJSONAsync<T>(Stream stream) => await DeserializeJSONAsync<T>(new StreamReader(stream));
        public static async Task<T> DeserializeJSONAsync<T>(string serialized) => await DeserializeJSONAsync<T>(new StringReader(serialized));

        private static T DeserializeJSON<T>(TextReader reader)
        {
            JsonReader jreader = new JsonTextReader(reader);
            jreader.Read();
            JsonSerializer serializer = new JsonSerializer();
            return serializer.Deserialize<T>(jreader);
        }
        public static T DeserializeJSON<T>(Stream stream) => DeserializeJSON<T>(new StreamReader(stream));
        public static T DeserializeJSON<T>(string serialized) => DeserializeJSON<T>(new StringReader(serialized));

        public static string SerializeJSON<T>(T obj)
        {
            StringWriter sw = new StringWriter();
            JsonWriter jwriter = new JsonTextWriter(sw);

            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());

            serializer.Serialize(jwriter, obj);

            return sw.ToString();
        }
    }
}
