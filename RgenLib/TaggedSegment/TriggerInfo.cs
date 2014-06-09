using EnvDTE80;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RgenLib.TaggedSegment {
    
    public class TriggerInfo {
        public TriggerInfo() { }
        public TriggerInfo(TriggerTypes type)
        {
            Type = type;
        }
        [JsonConverter(typeof(StringEnumConverter))]
        public TriggerTypes Type { get; set; }

        [JsonConverter(typeof(CodeClassJsonConverter))]
        public CodeClass2 TriggeringBaseClass {get;set;}

    }
}
