using EnvDTE80;
using Newtonsoft.Json;

namespace RgenLib.TaggedSegment {
    
    public class TriggerInfo {
        public TriggerTypes Type { get; set; }

        [JsonConverter(typeof(CodeClassJsonConverter))]
        public CodeClass2 TriggeringBaseClass {get;set;}

    }
}
