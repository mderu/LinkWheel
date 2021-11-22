using CoreAPI.Cli;
using CoreAPI.Config;
using CoreAPI.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CoreAPI.RemoteHosts
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
            var tasks = repoCandidates.Select(
                async (candidate) =>
                {
                    if (candidate.RemoteRepoHostType != null 
                        && TaskUtils.Try(await candidate.RemoteRepoHostType.TryGetLocalPath(url, candidate), out string resultingPath))
                    {
                        return resultingPath;
                    }
                    return null;
                });
            var results = Task.WhenAll(tasks).Result.Where(value => value != null).ToList();
            if (results.Count == 1)
            {
                localPath = results[0];
                return true;
            }
            if (results.Count > 1)
            {
                //throw new Exception();
                localPath = $"Multiple matches for {url}: {string.Join(", ", results[0])}";
                return false;
            }

            localPath = "";
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localFilePath">The localFilePath requested as `{localFilePath}#{lineNum}:~:text={text}`</param>
        /// <param name="repoCandidates">The list of all repo configs to check against.</param>
        /// <param name="remoteLink"></param>
        /// <returns></returns>
        public static async Task<(bool, Uri)> TryGetRemoteLinkFromPath(GetUrl request, List<RepoConfig> repoCandidates)
        {
            string actualPath = request.File;
            DirectoryInfo curDir;
            if (Directory.Exists(actualPath))
            {
                curDir = new DirectoryInfo(actualPath);
            }
            else
            {
                curDir = new FileInfo(actualPath).Directory;
            }

            while(curDir != null)
            {
                foreach (var candidate in repoCandidates)
                {
                    if (FileUtils.ArePathsEqual(candidate.Root, curDir.FullName))
                    {
                        return (true, await candidate.RemoteRepoHostType.GetRemoteLink(request, candidate));
                    }
                }
                curDir = curDir.Parent;
            }

            return (false, null);
        }
    }
}
