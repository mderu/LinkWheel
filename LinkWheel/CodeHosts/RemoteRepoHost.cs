using LinkWheel.InternalConfig;
using System;
using System.Collections.Generic;

namespace LinkWheel.CodeHosts
{
    // TODO: I kind of don't like this name, because Host could be a physical machine, and not a hosting solution.
    // RemoteRepoHostingSolution is a mouthful though, so ¯\_(ツ)_/¯.
    public abstract class RemoteRepoHost
    {
        /// <summary>
        /// Returns true if localRepo root can only be described by this type.
        /// </summary>
        /// <param name="localRepoRoot">The absolute path to the local repository.</param>
        /// <param name="remoteRepoUri">The remote repo URI to associate this repo with.</param>
        public abstract bool TryGetRootUrl(string localRepoRoot, out string remoteRepoUri);

        /// <summary>
        /// Returns true if an absolute path to a local file matches the remoteUri given.
        /// </summary>
        /// <param name="remoteUri">The URL to correlate to a local path.</param>
        /// <param name="repoConfig">The <see cref="RepoConfig"/> this URL is associated with.</param>
        /// <param name="localPath">The absolute path to the local file correlated with the remoteUri.</param>
        public abstract bool TryGetLocalPath(Uri remoteUri, RepoConfig repoConfig, out string localPath);
    }
}
