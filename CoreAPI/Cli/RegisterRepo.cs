using CommandLine;
using CoreAPI.Config;
using CoreAPI.OutputFormat;
using System.Collections.Generic;
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
                    RepoConfigs.Register(repoConfig);
                }
                result.Objects["result"] = results[0];
            }
            result.Format = "(=$=)";
            return result;
        }
    }
}
