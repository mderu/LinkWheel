using CommandLine;
using CoreAPI.Config;
using CoreAPI.OutputFormat;
using CoreAPI.RemoteHosts;
using CoreAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("get-roots", HelpText = HelpText)]
    public class GetRoots
    {
        public const string HelpText = "Returns all root repo directories for the given path. " +
            "There can be multiple matches (e.g., you have nested repositories, like a git " +
            "repo stored within a Perforce repo, or Git submodules). If you only want the " +
            "innermost result, use 'get-root', or use the global `--format` flag.";

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
                        return newRepoConfig;
                    }
                    return null;
                });
            var results = (await Task.WhenAll(tasks))
                .RemoveNulls()
                .OrderByDescending(result => result.Root.Length)
                .ToList();
            // Sort the results by innermost first.
            results.OrderByDescending(x => x.Root.Length);
            Dictionary<string, object> outputObjects = new() { ["results"] = results, ["givenPath"] = Path };
            if (results.Count > 0)
            {
                StringBuilder formatStringBuilder = new();
                for (int i = 0; i < results.Count; i++)
                {
                    formatStringBuilder.AppendLine("(=results[");
                    formatStringBuilder.Append(i);
                    formatStringBuilder.AppendLine("].root=)");
                    if (i != results.Count)
                    {
                        // TODO: Configuration for disabling \r's in Windows?
                        formatStringBuilder.Append(Environment.NewLine);
                    }
                }
                return new OutputData(0, outputObjects, formatStringBuilder.ToString());
            }
            else
            {
                return new OutputData(1, outputObjects, $"Unable to determine remote repo for (=givenPath=)");
            }
        }
    }
}
