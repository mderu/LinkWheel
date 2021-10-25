using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkWheel.UserConfig
{
    class IdeWorkspace
    {
        [JsonProperty("fnmatch")]
        public List<string> FnMatches { get; set; } = new() { };

        [JsonProperty("command")]
        public string Command { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// A path to an image or executable. If an executable file is given, the icon it uses
        /// will be displayed instead. If this value is not supplied, the icon will be derived
        /// from the first argument of the command property.
        /// </summary>
        [JsonProperty("icon_source")]
        public string Icon { get; set; }
    }
}
