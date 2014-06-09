using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RgenLib.TaggedSegment.Json {
    public class OrderedContractResolver : DefaultContractResolver  {
        public OrderedContractResolver(Func<JsonProperty, Object>orderFunc)
        {
            OrderFunc = orderFunc;
        }

        public Func<JsonProperty, object> OrderFunc { get; set; }

        protected override System.Collections.Generic.IList<JsonProperty> CreateProperties(System.Type type, MemberSerialization memberSerialization) {
            //Template property has to be the first one
            return base.CreateProperties(type, memberSerialization).OrderBy(OrderFunc).ToList();
        }
    }
}

