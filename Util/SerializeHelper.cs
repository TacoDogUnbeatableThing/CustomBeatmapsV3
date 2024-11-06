﻿using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// weird concurrency issues with this one
// using File = Pri.LongPath.File;
// using Path = Pri.LongPath.Path;
// using Directory = Pri.LongPath.Directory;

namespace CustomBeatmaps.Util
{
    /// <summary>
    /// Helps save/load serialized data 
    /// </summary>
    public static class SerializeHelper
    {
        
        private static object _avoidmultiwriteLock = new object();

        public static void SaveJSON<T>(string filePath, T data)
        {
            try
            {
                lock (_avoidmultiwriteLock)
                {
                    File.WriteAllText(filePath, SerializeJSON(data, true));
                }
            } catch (Exception e)
            {
                throw new Exception($"Failed to save JSON file: {filePath}", e);
            }
        }

        public static T LoadJSON<T>(string filePath)
        {
            return DeserializeJSON<T>(File.ReadAllText(filePath));
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

        public static string SerializeJSON<T>(T obj, bool prettyPrint=false)
        {
            StringWriter sw = new StringWriter();
            JsonWriter jwriter = new JsonTextWriter(sw);

            JsonSerializer serializer = new JsonSerializer();
            if (prettyPrint)
            {
                serializer.Formatting = Formatting.Indented;
            }
            serializer.Converters.Add(new JavaScriptDateTimeConverter());

            serializer.Serialize(jwriter, obj);

            return sw.ToString();
        }
    }
}
