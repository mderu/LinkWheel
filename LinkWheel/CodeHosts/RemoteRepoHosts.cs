using LinkWheel.InternalConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LinkWheel.CodeHosts
{
    public class RemoteRepoHosts
    {
        /// <summary>
        /// Returns all <see cref="RemoteRepoHosts"/> sorted by highest priority.
        /// </summary>
        public static IEnumerable<RemoteRepoHost> All => AllRemoteRepoHostsLazy.Value;

        private static readonly Lazy<IEnumerable<RemoteRepoHost>> AllRemoteRepoHostsLazy = new(() =>
        {
            List<RemoteRepoHost> objects = new();
            List<int> priorities = new();
            foreach (Type type in Assembly.GetAssembly(typeof(RemoteRepoHost)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(RemoteRepoHost))))
            {
                HostPriorityAttribute attribute = type.GetCustomAttribute<HostPriorityAttribute>();
                if (attribute is null)
                {
                    throw new Exception(
                        $"All inheritors of {nameof(RemoteRepoHost)} must have a {nameof(HostPriorityAttribute)}.");
                }
                priorities.Add(attribute.Priority);
                objects.Add((RemoteRepoHost)Activator.CreateInstance(type));
            }
            return objects
                .Zip(priorities, (remoteRepoHost, priority) => (remoteRepoHost, priority))
                .OrderByDescending(pair => pair.priority)
                .Select(pair => pair.remoteRepoHost);
        });

        public static bool TryGetLocalPathFromUrl(Uri url, List<RepoConfig> repoCandidates, out string localPath)
        {
            foreach (RepoConfig candidate in repoCandidates)
            {
                if (candidate.RemoteRepoHostType.TryGetLocalPath(url, candidate, out localPath))
                {
                    return true;
                }
            }
            localPath = "";
            return false;
        }
    }
}
