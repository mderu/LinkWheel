using CoreAPI.Cli;
using CoreAPI.Config;
using System;
using System.Threading.Tasks;

namespace CoreAPI.RemoteHosts
{
    // TODO: I kind of don't like this name, because Host could be a physical machine, and not a hosting solution.
    // RemoteRepoHostingSolution is a mouthful though, so ¯\_(ツ)_/¯.
    public abstract class RemoteRepoHost
    {
        /// <summary>
        /// Returns (true, A new repo config containing the root url and cleaned up repo root)
        /// if localRepo root can only be described by this type.
        /// </summary>
        /// <param name="localRepoRoot">The absolute path to the local repository.</param>
        public abstract Task<(bool, RepoConfig?)> TryGetRepoConfig(string localRepoRoot);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localFilePath">The localFilePath requested as `{localFilePath}#{lineNum}:~:text={text}`</param>
        /// <param name="repoConfig">The repo config for this file.</param>
        /// <returns></returns>
        public abstract Task<(bool, string)> TryGetLocalPath(Uri remoteUri, RepoConfig repoConfig);

        /// <summary>
        /// Returns the link to the remote URL for the given file.
        /// </summary>
        /// <param name="request">The GetUrl request</param>
        /// <param name="repoConfig"></param>
        /// <returns></returns>
        public abstract Task<Uri> GetRemoteLink(GetUrl request, RepoConfig repoConfig);
    }
}
