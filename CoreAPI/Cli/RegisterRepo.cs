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
    [Verb("register", HelpText = HelpText)]
    public class RegisterRepo
    {
        [Option("path", Required = true)]
        public string Path { get; set; } = "";

        public const string HelpText = "Registers the repo the given path is a part of, if the path is valid. " +
            "Upon failure to register the repo, the return code is 1. " +
            "On success, the return code is 0 and the new RepoConfig is returned. " +
            "If already registered, this operation is effectively a no-op, but returns the same as success.";

        public async Task<int> ExecuteAsync()
        {
            var tasks = RemoteRepoHosts.All.Select(
                async (hostingSolution) =>
                {
                    if (TaskUtils.Try(await hostingSolution.TryGetRepoConfig(Path), out RepoConfig? newRepoConfig))
                    {
                        // Forgiveness: not null if the Try function return true.
                        return newRepoConfig!;
                    }
                    return null;
                });
            var results = (await Task.WhenAll(tasks)).RemoveNulls().ToList();
            if (results.Count == 1)
            {
                Register(results[0]);
                Console.WriteLine(JsonConvert.SerializeObject(results[0]));
                return 0;
            }
            else if (results.Count > 1)
            {
                throw new Exception($"Multiple matches for {Path}");
            }
            else
            {
                throw new Exception($"Unable to determine remote repo for {Path}");
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
