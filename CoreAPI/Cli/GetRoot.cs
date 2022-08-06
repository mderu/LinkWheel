using CommandLine;
using CoreAPI.Config;
using CoreAPI.OutputFormat;
using CoreAPI.RemoteHosts;
using CoreAPI.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("get-root", HelpText = HelpText)]
    public class GetRoot
    {
        public const string HelpText = "Returns the root repo directory for the given path. " +
            "Note that if multiple matches exist (e.g., you have nested repositories, like a Git " +
            "repo stored within a Perforce repo), an error message is returned. You can still " +
            "get the available paths by using the global `--format` flag.";

        [Option("path", Required = true, 
            HelpText = "The path of a file or directory you wish to find the repo root of.")]
        public string Path { get; set; } = "";

        public async Task<OutputData> ExecuteAsync()
        {
            var tasks = RemoteRepoHosts.All.Select(
                async (hostingSolution) =>
                {
                    if (TaskUtils.Try(await hostingSolution.TryGetRepoConfig(Path), out RepoConfig? newRepoConfig))
                    {
                        // Forgiveness: Non-null when if Try passes.
                        return newRepoConfig!;
                    }
                    return null;
                });
            var results = (await Task.WhenAll(tasks)).RemoveNulls().ToList();
            // Sort the results by innermost first.
            results.OrderByDescending(x => x.Root.Length);
            Dictionary<string, object> outputObjects = new() { ["results"] = results, ["givenPath"] = Path };
            if (results.Count == 1)
            {
                return new OutputData(0, outputObjects, "(=results[0].root=)");
            }
            else if (results.Count > 1)
            {
                return new OutputData(2, outputObjects, $"Multiple matches for (=givenPath=)");
            }
            else
            {
                return new OutputData(1, outputObjects, $"Unable to determine remote repo for (=givenPath=)");
            }
        }
    }
}
