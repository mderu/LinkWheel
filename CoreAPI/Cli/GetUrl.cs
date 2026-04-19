using CommandLine;
using CoreAPI.Config;
using CoreAPI.OutputFormat;
using CoreAPI.RemoteHosts;
using CoreAPI.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("get-url", HelpText = GetUrlHelpText)]
    public class GetUrl
    {
        public const string GetUrlHelpText = "Returns the URL for a given local --file, with optional --start-line" +
            "and --end-line.";

        public const string FileHelpText = "The file to get the URL for.";
        [Option("file", Required = true, HelpText = FileHelpText)]
        public string File { get; set; } = "";

        [Option("start-line")]
        public int? StartLine { get; set; }

        [Option("end-line")]
        public int? EndLine { get; set; }

        public const string RegisterHelpText =
            "Whether to attempt to register the repo if the given file is not already part of a registered repo.";
        [Option("register", Default = false, HelpText = RegisterHelpText)]
        public bool Register { get; set; }

        public async Task<OutputData> ExecuteAsync()
        {
            List<RepoConfig> repoConfigs = RepoConfigs.All();

            if (TaskUtils.Try(await RemoteRepoHosts.TryGetRemoteLinkFromPath(this, repoConfigs), out RepoConfig? repoConfig, out Uri? remoteLink))
            {
                return new(0, new() { ["repoConfig"] = repoConfig, ["url"] = remoteLink.ToString() }, "(=url=)");
            }

            if (Register)
            {
                OutputData registerResult = await new RegisterRepo() { Path = File }.ExecuteAsync();
                var registeredRepoConfig = (RepoConfig)registerResult.Objects["result"];
                if (TaskUtils.Try(await RemoteRepoHosts.TryGetRemoteLinkFromPath(this, new List<RepoConfig>() { registeredRepoConfig }), out repoConfig, out remoteLink))
                {
                    return new(0, new() { ["repoConfig"] = repoConfig, ["url"] = remoteLink.ToString() }, "(=url=)");
                }
            }

            return new(1, new(), "");
        }
    }
}
