using CoreAPI.RemoteHosts;
using CoreAPI.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace CoreAPI.Config
{
    public static class RepoConfigFile
    {
        public static List<RepoConfig> Read()
        {
            string config = FileUtils.ReadAllTextWait(LinkWheelConfig.TrackedReposFile);
            List<RepoConfig> repoConfigs;
            try
            {
                repoConfigs = JsonConvert.DeserializeObject<List<RepoConfig>>(config)
                    // If the file is empty, assume an empty list.
                    ?? new();
            }
            catch (JsonReaderException)
            {
                // If the file got messed up somehow, ignore its contents.
                repoConfigs = new();
            }
            return repoConfigs;
        }
    }

    public class RepoConfig
    {
        /// <summary>
        /// The local path to the root directory of this repo.
        /// </summary>
        [JsonProperty("root", Required = Required.DisallowNull)]
        public string Root { get; init; }

        /// <summary>
        /// The remote url to the root URI of this repo.
        /// </summary>
        /// <remarks>
        /// The exact URI format is defined on a per-host type basis. Ideally, this should be a URL that can be
        /// combined with the file's path relative to the repo root.
        /// 
        /// If any substantial remote repository solution doesn't allow for the local path to translate well to a
        /// remote URL, we should use the RemoteHostType to handle any API calls we need to serve a local link to the
        /// file.
        /// </remarks>
        [JsonProperty("remote_root_uri", Required = Required.DisallowNull)]
        public string RemoteRootUrl { get; init; }

        /// <summary>
        /// The class name of the remote repo host provider to use to translate the remote root url to a local file.
        /// </summary>
        [JsonProperty("remote_repo_host_type")]
        public string RawRemoteRepoHostType { get; init; }

        [JsonIgnore]
        private RemoteRepoHost remoteRepoHostType;

        /// <summary>
        /// A reference to the <see cref="RemoteRepoHost"/> responsible for 
        /// </summary>
        [JsonIgnore]
        public RemoteRepoHost RemoteRepoHostType
        {
            get
            {
                remoteRepoHostType ??= RemoteRepoHosts.All
                    .Where(host => host.GetType().Name == RawRemoteRepoHostType)
                    .FirstOrDefault();
                return remoteRepoHostType;
            }
        }

        [JsonProperty("remote_repo_host_keys")]
        public Dictionary<string, string> RemoteRepoHostKeys { get; set; }
    }
}
