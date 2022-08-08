using CoreAPI.RemoteHosts;
using CoreAPI.Utils;
using LiteDB;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace CoreAPI.Config
{
    public static class RepoConfigs
    {
        public static List<RepoConfig> All()
        {
            using LiteDatabase db = new(LinkWheelConfig.DatabaseFile);
            return db.GetCollection<RepoConfig>().Query().OrderBy(repoConfig => repoConfig.Root).ToList();
        }

        public static void Register(RepoConfig newRepoConfig)
        {
            using LiteDatabase db = new(LinkWheelConfig.DatabaseFile);
            var repoConfigCollection = db.GetCollection<RepoConfig>();
            var existingQuery = repoConfigCollection.Query()
                // ToList used so we can query true path equality
                // (FileUtils.ArePathsEqual cannot be converted to Bson query)
                .ToList()
                .Where(config => FileUtils.ArePathsEqual(config.Root, newRepoConfig.Root))
                .Select(item => item.Id);
            if (existingQuery.Any())
            {
                repoConfigCollection.Update(existingQuery.First(), newRepoConfig);
            }
            else
            {
                db.GetCollection<RepoConfig>().Insert(newRepoConfig);
            }
        }
    }

    public class RepoConfig
    {
        /// <summary>
        /// An ID used for LiteDB.
        /// </summary>
        [JsonIgnore]
        public int Id { get; set; }

        /// <summary>
        /// The local path to the root directory of this repo.
        /// </summary>
        [JsonProperty("root", Required = Required.DisallowNull)]
        public string Root { get; init; } = "";

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
        [JsonProperty("remoteRootUri", Required = Required.DisallowNull)]
        public string RemoteRootUrl { get; init; } = "";

        /// <summary>
        /// The regex that takes in a URL to match the local repo. Note that overfitting is allowed here, but is
        /// discouraged, as this regex is used to skip over potientially complex resolution logic.
        /// 
        /// Users should use <see cref="RemoteRootUrl"/> instead to get more accurate information.
        /// </summary>
        [JsonProperty("remoteRootRegex", Required = Required.DisallowNull)]
        public string RemoteRootRegex { get; init; } = "";

        /// <summary>
        /// The class name of the remote repo host provider to use to translate the remote root url to a local file.
        /// </summary>
        [JsonProperty("remoteRepoHostType", Required = Required.DisallowNull)]
        public string RawRemoteRepoHostType { get; init; } = "";

        [JsonIgnore]
        private RemoteRepoHost? remoteRepoHostType;

        /// <summary>
        /// A reference to the <see cref="RemoteRepoHost"/> responsible for 
        /// </summary>
        [JsonIgnore]
        public RemoteRepoHost RemoteRepoHostType
        {
            get
            {
                if (remoteRepoHostType is null)
                {
                    remoteRepoHostType = RemoteRepoHosts.All
                        .Where(host => host.GetType().Name == RawRemoteRepoHostType)
                        .FirstOrDefault();
                    if (remoteRepoHostType is null)
                    {
                        // TODO: An unloaded plugin or version difference can cause this error to be thrown.
                        // We should instead remove the entry from the DB and continue.
                        throw new System.InvalidOperationException(
                            $"No known remoteRepoHostType of type {RawRemoteRepoHostType}");
                    }
                }
                return remoteRepoHostType;
            }
        }

        [JsonProperty("remoteRepoHostKeys")]
        public Dictionary<string, string> RemoteRepoHostKeys { get; set; } = new Dictionary<string, string>();
    }
}
