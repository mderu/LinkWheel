using Newtonsoft.Json;
using System.Drawing;

namespace CoreAPI.Models
{
    /// <summary>
    /// ActionDefinition, but everything is defined and format strings are computed.
    /// To replace <see cref="Config.WheelElement"/>.
    /// </summary>
    public class IdelAction
    {
        [JsonProperty("priority")]
        public int Priority { get; set; }
        [JsonProperty("command")]
        public string Command { get; set; }
        [JsonIgnore]
        public string? CommandWorkingDirectory { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("iconSource")]
        public string? IconSource { get; set; }
        [JsonIgnore]
        public Bitmap? Icon { get; set; }
        [JsonProperty("iconSecondarySource")]
        public string? IconSecondarySource { get; set; }
        [JsonIgnore]
        public Bitmap? IconSecondary { get; set; }

    public IdelAction(
            int priority, 
            string command, 
            string title = "", 
            string description = "", 
            string? iconSource = null, 
            string? iconSecondarySource = null)
        {
            Priority = priority;
            Command = command;
            Title = title;
            Description = description;
            IconSource = iconSource;
            IconSecondarySource = iconSecondarySource;
        }
    }
}
