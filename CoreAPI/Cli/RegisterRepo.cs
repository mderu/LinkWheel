using CommandLine;
using CoreAPI.Config;
using CoreAPI.OutputFormat;
using CoreAPI.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("register", HelpText = HelpText)]
    public class RegisterRepo
    {
        public const string HelpText = "Registers the repo the given path is a part of, if the path is valid. " +
            "Upon failure to register the repo, the return code is 1. " +
            "On success, the return code is 0 and returns a list containing the newly registered RepoConfig(s). " +
            "If already registered, this operation is effectively a no-op, but returns the same as success.";

        [Option("path", Required = true, HelpText = "Any path within the repo you wish to register.")]
        public string Path { get; set; } = "";

        public async Task<OutputData> ExecuteAsync()
        {
            OutputData result = await new GetRoot() { Path = Path }.ExecuteAsync();
            var results = (List<RepoConfig>)result.Objects["results"];
            if (results.Count >= 1)
            {
                foreach (var repoConfig in results)
                {
                    Register(repoConfig);
                }
                result.Objects["result"] = results[0];
            }
            result.Format = "(=$=)";
            return result;
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
