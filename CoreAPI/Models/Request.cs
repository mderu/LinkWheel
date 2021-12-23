using CoreAPI.Config;
using Newtonsoft.Json;
using System;

namespace CoreAPI.Models
{
    /// <summary>
    /// A class containing a remote URL and the subsequent resolved local file information.
    /// </summary>
    public class Request
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("repoConfig")]
        public RepoConfig RepoConfig { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }

        [JsonProperty("normalizedFile")]
        public string NormalizedFile
        {
            get
            {
                // If the operating system is Windows, we normalize the directory separators to be backslashes.
                // This is done so that some ancient commands in Windows (e.g., C:\Windows\explorer.exe), can
                // still operate on these files.
                //
                // If this ends up being an issue, we should move this functionality to another property,
                // e.g., fileNormalized.
                if (OperatingSystem.IsWindows())
                {
                    return File.Replace("/", "\\");
                }
                else
                {
                    return File;
                }
            }
        }
        [JsonProperty("relativePath")]
        public string RelativePath
        {
            get
            {
                Uri parentUri = new(RepoConfig.Root, UriKind.Absolute);
                Uri childUri = new(File, UriKind.Absolute);

                return parentUri.MakeRelativeUri(childUri).ToString();
            }
        }

        [JsonProperty("startLine")]
        public int? StartLine { get; set; }

        [JsonProperty("endLine")]
        public int? EndLine { get; set; }

        // Stretch goal:
        // public string? HighlightedSnippet { get; set; }

        public Request(string url, string file, RepoConfig repoConfig)
        {
            Url = url;
            File = file;
            RepoConfig = repoConfig;
        }
    }
}
