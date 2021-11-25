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
    [Verb("get-root")]
    public class GetRoot
    {
        [Option("path", Required = true)]
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

            Dictionary<string, object> outputObjects = new() { ["results"] = results, ["givenPath"] = Path };
            if (results.Count == 1)
            {
                return new OutputData(0, outputObjects, "(=results[0].root=)");
            }
            else if (results.Count > 1)
            {
                return new OutputData(1, outputObjects, $"Multiple matches for (=givenPath=)");
            }
            else
            {
                return new OutputData(1, outputObjects, $"Unable to determine remote repo for (=givenPath=)");
            }
        }
    }
}
