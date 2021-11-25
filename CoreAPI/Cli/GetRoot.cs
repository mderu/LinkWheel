using CommandLine;
using CoreAPI.Config;
using CoreAPI.RemoteHosts;
using CoreAPI.Utils;
using System;
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

        public async Task<int> ExecuteAsync()
        {
            if (TaskUtils.Try(await TryGet(), out string result))
            {
                Console.WriteLine(result);
                return 0;
            }
            Console.Error.WriteLine(result);
            return 1;
        }

        public async Task<(bool, string)> TryGet()
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
            if (results.Count == 1)
            {
                return new(true, results[0].Root);
            }
            else if (results.Count > 1)
            {
                return (false, $"Multiple matches for {Path}");
            }
            else
            {
                return (false, $"Unable to determine remote repo for {Path}");
            }
        }
    }
}
