using System;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace WcfRestClient.Helpers
{
    public static class NewtonsoftInterop
    {
        public static readonly Encoding JsonEncoding = new UTF8Encoding(false);
        private static readonly JsonSerializer _serializer;

        static NewtonsoftInterop()
        {
            _serializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.Auto,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                DateFormatString = LibraryCulture.DatetimeFormat
            };
        }

        public static string SerializeToString<T>(T value)
        {
            var stringWriter = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture);
            using (var jsonTextWriter = new JsonTextWriter(stringWriter))
            {
                _serializer.Serialize(jsonTextWriter, value);
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
                _serializer.Serialize(writer, value);
                sw.Flush();
            }
        }

        public static T DeserializeFromString<T>(string value)
        {
            using (var jsonTextReader = new JsonTextReader(new StringReader(value)))
            {
                return (T) _serializer.Deserialize(jsonTextReader, typeof (T));
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
            return _serializer.Deserialize(reader, objectType);
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
