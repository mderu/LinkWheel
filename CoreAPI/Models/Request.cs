using Newtonsoft.Json;
using System;

namespace CoreAPI.Models
{
    public class Request
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonIgnore]
        private string filePath = "";

        [JsonProperty("file")]
        public string File 
        {
            get
            {
                return filePath;
            }
            set
            {
                // If the operating system is Windows, we normalize the directory separators to be backslashes.
                // This is done so that some ancient commands in Windows (e.g., C:\Windows\explorer.exe), can
                // still operate on these files.
                //
                // If this ends up being an issue, we should move this functionality to another property,
                // e.g., fileNormalized.
                if (OperatingSystem.IsWindows())
                {
                    filePath = value.Replace("/", "\\");
                }
                else
                {
                    filePath = value;
                }
            }
        }
        
        [JsonProperty("startLine")]
        public int? StartLine { get; set; }

        [JsonProperty("endLine")]
        public int? EndLine { get; set; }

        // Stretch goal:
        // public string? HighlightedSnippet { get; set; }

        public Request(string url, string file)
        {
            Url = url;
            File = file;
        }
    }
}
