using LinkWheel.InternalConfig;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkWheel.CodeHosts
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
        public abstract Task<(bool, RepoConfig)> TryGetRootUrl(string localRepoRoot);

        /// <summary>
        /// Returns (true, The absolute path to the local file correlated with the remoteUri)
        /// if an absolute path to a local file matches the remoteUri given.
        /// </summary>
        /// <param name="remoteUri">The URL to correlate to a local path.</param>
        /// <param name="repoConfig">The <see cref="RepoConfig"/> this URL is associated with.</param>
        /// <param name="localPath">The absolute path to the local file correlated with the remoteUri.</param>
        public abstract Task<(bool, string)> TryGetLocalPath(Uri remoteUri, RepoConfig repoConfig);

        /// <summary>
        /// Returns the link to the remote URL for the given file.
        /// </summary>
        /// <param name="localFilePath">{filepath}#{lineNumber}:~:text={text}</param>
        /// <param name="repoConfig"></param>
        /// <returns></returns>
        public abstract Task<Uri> GetRemoteLink(string localFilePath, RepoConfig repoConfig);
    }
}
