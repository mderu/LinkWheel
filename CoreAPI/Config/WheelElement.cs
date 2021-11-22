using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace CoreAPI.Config
{
    public class WheelElement
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("description")]
        public string Description;

        [JsonProperty("commandline")]
        public IEnumerable<string> CommandAction;

        [JsonProperty("iconPath")]
        public string IconPath;

        [JsonProperty("iconPathSecondary")]
        public string IconPathSecondary;

        [JsonIgnore]
        public Bitmap Icon => IconLazy.Value;
        [JsonIgnore]
        public Lazy<Bitmap> IconLazy;

        [JsonIgnore]
        public Bitmap IconSecondary => IconSecondaryLazy.Value;
        [JsonIgnore]
        public Lazy<Bitmap> IconSecondaryLazy;


        public WheelElement()
        {
            IconLazy = new(() => GetBitmapFromDisk(IconPath));
            IconSecondaryLazy = new(() => GetBitmapFromDisk(IconPathSecondary));
        }

        /// <summary>
        /// Gets the bitmap from disk, or null if it does not exist.
        /// </summary>
        private static Bitmap GetBitmapFromDisk(string iconPath)
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
