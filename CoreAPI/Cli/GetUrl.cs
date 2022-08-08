using CommandLine;
using CoreAPI.Config;
using CoreAPI.OutputFormat;
using CoreAPI.RemoteHosts;
using CoreAPI.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("get-url", HelpText = HelpText)]
    public class GetUrl
    {
        public const string HelpText = "Returns the URL for a given local --file, with optional --start-line" +
            "and --end-line.";

        [Option("file", Required = true)]
        public string File { get; set; } = "";

        [Option("start-line")]
        public int? StartLine { get; set; }

        [Option("end-line")]
        public int? EndLine { get; set; }

        public async Task<OutputData> ExecuteAsync()
        {
            List<RepoConfig> repoConfigs = RepoConfigs.All();

            if (TaskUtils.Try(await RemoteRepoHosts.TryGetRemoteLinkFromPath(this, repoConfigs), out RepoConfig? repoConfig, out Uri ? remoteLink))
            {
                // Forgiveness: above try passes, so both can be forgiven.
                return new(0, new() { ["repoConfig"] = repoConfig!, ["url"] = remoteLink!.ToString() }, "(=url=)");
            }
            return new(1, new(), "");
        }
    }
}
