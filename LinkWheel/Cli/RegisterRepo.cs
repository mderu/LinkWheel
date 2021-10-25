using CommandLine;
using LinkWheel.CodeHosts;
using LinkWheel.InternalConfig;
using LinkWheel.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkWheel.Cli
{
    [Verb("open-in-default-browser")]
    class RegisterRepo
    {
        [Option("directory", Required = true)]
        public string Directory { get; set; }

        public void Execute()
        {
            foreach (var hostingSolution in RemoteRepoHosts.All)
            {
                if (hostingSolution.TryGetRootUrl(Directory, out RepoConfig newRepoConfig))
                {
                    Register(newRepoConfig);
                    return;
                }
            }
            throw new Exception($"Unable to determine remote repo for {Directory}");
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
