using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace CoreAPI.Config
{
    public class WheelElement
    {
        [JsonProperty("name", Required = Required.DisallowNull)]
        public string Name { get; set; } = "";
        [JsonProperty("description", Required = Required.DisallowNull)]
        public string Description { get; set; } = "";

        [JsonProperty("commandline", Required = Required.DisallowNull)]
        public IEnumerable<string> CommandAction { get; set; } = new List<string>();

        [JsonProperty("iconPath", Required = Required.DisallowNull)]
        public string IconPath { get; set; } = "";

        [JsonProperty("iconPathSecondary")]
        public string? IconPathSecondary { get; set; } = "";

        /// <remarks>
        /// Can be null (cases where the website is not loaded); the form protects against this.
        /// </remarks>
        [JsonIgnore]
        public Bitmap? Icon => IconLazy.Value;
        [JsonIgnore]
        public Lazy<Bitmap?> IconLazy;
        [JsonIgnore]
        public Bitmap? IconSecondary => IconSecondaryLazy.Value;
        [JsonIgnore]
        public Lazy<Bitmap?> IconSecondaryLazy;


        public WheelElement()
        {
            IconLazy = new(() => GetBitmapFromDisk(IconPath));
            IconSecondaryLazy = new(() => GetBitmapFromDisk(IconPathSecondary));
        }

        /// <summary>
        /// Gets the bitmap from disk, or null if it does not exist.
        /// </summary>
        private static Bitmap? GetBitmapFromDisk(string iconPath)
        {
            if (File.Exists(iconPath))
            {
                return new Bitmap(iconPath);
            }
            else
            {
                return null;
            }
        }
    }
}
