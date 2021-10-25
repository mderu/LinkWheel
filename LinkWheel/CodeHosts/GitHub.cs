using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliWrap;
using LinkWheel.InternalConfig;

namespace LinkWheel.CodeHosts
{
    [HostPriority(0)]
    class GitHub : RemoteRepoHost
    {
        public override bool TryGetLocalPath(Uri remoteUri, RepoConfig repoConfig, out string localPath)
        {
            // TODO: Ideally we'll want to open a file to a specific line in the future (i.e., read the # or ? in the
            // remoteUri). We'll have to change the return type of this function to a more descriptive object.
            string[] requestedParts = remoteUri.PathAndQuery.Split("?")[0].Split("#")[0].Split("/");
            string[] configuredParts = new Uri(repoConfig.RemoteRootUrl).PathAndQuery.Split("/");


            for (int i = 0; i < configuredParts.Length; i++)
            {
                if (configuredParts[i] != requestedParts[i])
                {
                    localPath = "";
                    return false;
                }
            }

            // Verify that the URL kind of looks like a GitHub-hosted project URL.
            if (!(requestedParts[3] == "blob" || requestedParts[3] == "tree"))
            {
                localPath = "";
                return false;
            }

            // Magic number explanation:
            // The 5 skipped parts are:
            //   1) empty string (paths are rooted)
            //   2) username
            //   3) repo name
            //   4) blob or tree
            //   5) commit hash or branch name
            localPath = Path.Combine(
                repoConfig.Root,
                string.Join("/", requestedParts.Skip(5)));
            return true;
        }

        public override bool TryGetRootUrl(string localRepoRoot, out string remoteRepoUri)
        {
            // For now, we assume the user didn't super configure their git repo to use something
            // other than "origin". Definitely something we'll have to revisit, but it works for
            // the majority of git users.
            //
            // TODO: asyncify stuff.
            if (!Task.Run(() => IsGitInstalled()).Result)
            {
                remoteRepoUri = "";
                return false;
            }
            (bool isValid, string remoteOrigin) = Task.Run(() => GetRemoteOriginUrl(localRepoRoot)).Result;
            if (isValid)
            {
                // Trims ".git"
                remoteRepoUri = remoteOrigin[..^4];
                return true;
            }
            remoteRepoUri = "";
            return false;
        }

        /// <summary>
        /// Returns the configured origin remote URL.
        /// </summary>
        /// <param name="workingDirectory"></param>
        /// <returns></returns>
        public static async Task<(bool, string)> GetRemoteOriginUrl(string workingDirectory)
        {
            var stdOutBuffer = new StringBuilder();
            var result = await CliWrap.Cli.Wrap("git")
                .WithArguments("config --get remote.origin.url")
                .WithWorkingDirectory(workingDirectory)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .ExecuteAsync()
                // TODO: Remove this after making stuff async
                .ConfigureAwait(false);

            return (result.ExitCode == 0, stdOutBuffer.ToString().Trim());
        }

        private static async Task<bool> IsGitInstalled()
        {
            var stdOutBuffer = new StringBuilder();
            // I think which is more common on 
            await CliWrap.Cli.Wrap(OperatingSystem.IsWindows() ? "where" : "which")
                .WithArguments("git")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .ExecuteAsync()
                // TODO: Remove this after making stuff async
                .ConfigureAwait(false);
            return stdOutBuffer.ToString().Trim().Length != 0;
        }
    }
}
