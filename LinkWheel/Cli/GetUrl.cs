﻿using CommandLine;
using LinkWheel.CodeHosts;
using LinkWheel.InternalConfig;
using LinkWheel.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkWheel.Cli
{
    [Verb("get-url")]
    class GetUrl
    {
        [Option("file", Required = true)]
        public string File { get; set; }

        public async Task<int> ExecuteAsync()
        {
            List<RepoConfig> repoConfigs = RepoConfigFile.Read();

            if (TaskUtils.Try(await RemoteRepoHosts.TryGetRemoteLinkFromPath(File, repoConfigs), out Uri remoteLink))
            {
                Console.WriteLine(remoteLink.ToString());
                return 0;
            }
            return 1;
        }
    }
}