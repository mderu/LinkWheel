using CommandLine;
using CoreAPI.Config;
using CoreAPI.RemoteHosts;
using CoreAPI.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("register")]
    public class RegisterRepo
    {
        [Option("directory", Required = true)]
        public string Directory { get; set; }

        public async Task<int> ExecuteAsync()
        {
            var tasks = RemoteRepoHosts.All.Select(
                async (hostingSolution) =>
                {
                    if (TaskUtils.Try(await hostingSolution.TryGetRootUrl(Directory), out RepoConfig newRepoConfig))
                    {
                        return newRepoConfig;
                    }
                    return null;
                });
            var results = (await Task.WhenAll(tasks)).Where(value => value != null).ToList();
            if (results.Count == 1)
            {
                Register(results[0]);
                return 0;
            }
            else if (results.Count > 1)
            {
                throw new Exception($"Multiple matches for {Directory}");
            }
            else
            {
                throw new Exception($"Unable to determine remote repo for {Directory}");
            }
        }

        public static void Register(RepoConfig newRepoConfig)
        {
            FileUtils.Lock(LinkWheelConfig.TrackedReposFile, (filestream) =>
            {
                using StreamReader sr = new(filestream);
                using StreamWriter sw = new(filestream);

                List<RepoConfig> currentRepoConfigs;
                try
                {
                    currentRepoConfigs = JsonConvert.DeserializeObject<List<RepoConfig>>(sr.ReadToEnd())
                        ?? new();
                }
                catch (JsonReaderException)
                {
                    // If the file got messed up somehow, ignore its contents.
                    currentRepoConfigs = new();
                }

                filestream.SetLength(0);
                sw.Write(JsonConvert.SerializeObject(
                        currentRepoConfigs
                            // If the repo root is already there, remove that entry. This should make it easier for us to
                            // update RemoteRepoHosts in the future (e.g., splitting GitLab from GitHub).
                            .Where(config => !FileUtils.ArePathsEqual(config.Root, newRepoConfig.Root))
                            .Append(newRepoConfig)));
                sw.Flush();
            });
        }
    }
}
