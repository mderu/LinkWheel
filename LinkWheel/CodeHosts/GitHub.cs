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
        public override Task<(bool, string)> TryGetLocalPath(Uri remoteUri, RepoConfig repoConfig)
        {
            // TODO: Ideally we'll want to open a file to a specific line in the future (i.e., read the # or ? in the
            // remoteUri). We'll have to change the return type of this function to a more descriptive object.
            string[] requestedParts = remoteUri.PathAndQuery.Split("?")[0].Split("#")[0].Split("/");
            string[] configuredParts = new Uri(repoConfig.RemoteRootUrl).PathAndQuery.Split("/");

            for (int i = 0; i < configuredParts.Length; i++)
            {
                if (configuredParts[i] != requestedParts[i])
                {
                    return Task.FromResult((false, ""));
                }
            }

            // Verify that the URL kind of looks like a GitHub-hosted project URL.
            if (!(requestedParts[3] == "blob" || requestedParts[3] == "tree"))
            {
                return Task.FromResult((false, ""));
            }

            // Magic number explanation:
            // The 5 skipped parts are:
            //   1) empty string (paths are rooted)
            //   2) username
            //   3) repo name
            //   4) blob or tree
            //   5) commit hash or branch name
            return Task.FromResult((true,
                Path.Combine(
                    repoConfig.Root,
                    string.Join("/", requestedParts.Skip(5)))));
        }

        public override async Task<(bool, RepoConfig)> TryGetRootUrl(string localRepoRoot)
        {
            // TODO: The logic here will match pretty much any Git repo. We need to see if there's a way
            // we can differentiate GitHub from other git hosting solutions.

            // TODO: asyncify stuff.
            if (!await IsGitInstalled())
            {
                return (false, null);
            }

            // For now, we assume the user didn't super configure their git repo to use something
            // other than "origin". Definitely something we'll have to revisit, but it works for
            // the majority of git users.
            (bool isValid, string remoteOrigin) = await GetRemoteOriginUrl(localRepoRoot);
            if (isValid)
            {
                return (
                    true,
                    new RepoConfig()
                    {
                        // Trims ".git"
                        RemoteRootUrl = remoteOrigin[..^4],
                        Root = (await GetRepoRoot(localRepoRoot)).root,
                        RawRemoteRepoHostType = nameof(GitHub),
                    }
                );
            }
            return (false, null);
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
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();

            return (result.ExitCode == 0, stdOutBuffer.ToString().Trim());
        }

        private static async Task<bool> IsGitInstalled()
        {
            var stdOutBuffer = new StringBuilder();
            // I think `which` is more common on Unix/Linux.
            await CliWrap.Cli.Wrap(OperatingSystem.IsWindows() ? "where" : "which")
                .WithArguments("git")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();
            return stdOutBuffer.ToString().Trim().Length != 0;
        }

        public static async Task<(bool success, string root)> GetRepoRoot(string localPath)
        {
            var stdOutBuffer = new StringBuilder();
            var result = await CliWrap.Cli.Wrap("git")
                .WithArguments("rev-parse --show-toplevel")
                .WithWorkingDirectory(Path.GetDirectoryName(localPath))
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();

            return (success: result.ExitCode == 0, root: stdOutBuffer.ToString().Trim());
        }
    }
}
