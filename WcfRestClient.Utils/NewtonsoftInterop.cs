using System;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace WcfRestClient.Utils
{
    public static class NewtonsoftInterop
    {
        public static readonly Encoding JsonEncoding = new UTF8Encoding(false);
        private static readonly JsonSerializer Serializer;

        static NewtonsoftInterop()
        {
            Serializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.Auto,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffzzz"
            };
        }

        public static string SerializeToString<T>(T value)
        {
            var stringWriter = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture);
            using (var jsonTextWriter = new JsonTextWriter(stringWriter))
            {
                Serializer.Serialize(jsonTextWriter, value);
            }
            return stringWriter.ToString();
        }

        public static byte[] SerializeToBytes<T>(T value)
        {
            using (var ms = new MemoryStream())
            {
                SerializeToStream(value, ms);
                return ms.ToArray();
            }
        }

        public static void SerializeToStream<T>(T value, Stream stream)
        {
            using (var sw = new StreamWriter(stream, JsonEncoding))
            using (var writer = new JsonTextWriter(sw))
            {
                Serializer.Serialize(writer, value);
                sw.Flush();
            }
        }

        public static T DeserializeFromString<T>(string value)
        {
            using (var jsonTextReader = new JsonTextReader(new StringReader(value)))
            {
                return (T) Serializer.Deserialize(jsonTextReader, typeof (T));
            }
        }

        public static T DeserializeFromBytes<T>(byte[] value)
        {
            return (T) DeserializeFromBytes(value, typeof (T));
        }

        public static object DeserializeFromBytes(byte[] value, Type objectType)
        {
            using (var ms = new MemoryStream(value, false))
            {
                return DeserializeFromStream(ms, objectType);
            }
        }

        public static object DeserializeFromReader(JsonReader reader, Type objectType)
        {
            return Serializer.Deserialize(reader, objectType);
        }

        public static T DeserializeFromStream<T>(Stream stream)
        {
            return (T) DeserializeFromStream(stream, typeof (T));
        }

        private static object DeserializeFromStream(Stream stream, Type objectType)
        {
            using (var sr = new StreamReader(stream, JsonEncoding))
            using (var reader = new JsonTextReader(sr))
            {
                return DeserializeFromReader(reader, objectType);
            }
        }
    }
}
