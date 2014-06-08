using System;
using EnvDTE80;
using Newtonsoft.Json.Converters;

namespace RgenLib.TaggedSegment {
    class CodeClassJsonConverter : CustomCreationConverter<CodeClass2>
    {
        public override CodeClass2 Create(Type objectType)
        {
            throw new NotImplementedException();
        }
        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer) {
            //not yet implemented
            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer) {
            //only write the name
            var cls = (CodeClass2)value;
            base.WriteJson(writer, cls.Name, serializer);
        }
    }
}
