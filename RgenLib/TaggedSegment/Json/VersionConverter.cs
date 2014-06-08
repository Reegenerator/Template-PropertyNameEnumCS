using System;
using Newtonsoft.Json;
using RgenLib.Extensions;

namespace RgenLib.TaggedSegment.Json {
    class VersionConverter :JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var ver = (Version) value;
            writer.WriteRawValue(ver.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Debug.DebugHere();
            Version ver;
            Version.TryParse(reader.Value.ToString(), out ver);
            return ver;
        }

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Version));
        }
    }
}
