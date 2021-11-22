using CommandLine;
using CoreAPI.Config;
using CoreAPI.RemoteHosts;
using CoreAPI.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("get-url")]
    public class GetUrl
    {
        [Option("file", Required = true)]
        public string File { get; set; }

        public async Task<int> ExecuteAsync()
        {
            List<RepoConfig> repoConfigs = RepoConfigFile.Read();

            if (TaskUtils.Try(await RemoteRepoHosts.TryGetRemoteLinkFromPath(File, repoConfigs), out Uri remoteLink))
            {
                Trace.WriteLine(remoteLink.ToString());
                return 0;
            }
            return 1;
        }
    }
}
