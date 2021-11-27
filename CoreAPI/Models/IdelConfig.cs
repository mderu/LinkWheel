using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CoreAPI.Models
{
    class IdelConfig
    {
        [JsonProperty("definitions")]
        public Dictionary<string, IdelActionDefinition>? Definitions { get; set; }
        
        [JsonProperty("actions")]
        public Dictionary<string, JObject>? Actions { get; set; }
    }
}
