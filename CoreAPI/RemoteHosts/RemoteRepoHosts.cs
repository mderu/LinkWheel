using CoreAPI.Cli;
using CoreAPI.Config;
using CoreAPI.Models;
using CoreAPI.Utils;
using Newtonsoft.Json;
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
            // Forgiveness: we know the type exists in an assembly.
            foreach (Type type in Assembly.GetAssembly(typeof(RemoteRepoHost))!.GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(RemoteRepoHost))))
            {
                HostPriorityAttribute? attribute = type.GetCustomAttribute<HostPriorityAttribute>();
                if (attribute is null)
                {
                    throw new Exception(
                        $"All inheritors of {nameof(RemoteRepoHost)} must have a {nameof(HostPriorityAttribute)}.");
                }
                priorities.Add(attribute.Priority);
                var instance = (RemoteRepoHost?)Activator.CreateInstance(type);
                if (instance is null)
                {
                    throw new InvalidOperationException($"{type.FullName} needs to be instantiatable.");
                }
                objects.Add(instance);
            }
            return objects
                .Zip(priorities, (remoteRepoHost, priority) => (remoteRepoHost, priority))
                .OrderByDescending(pair => pair.priority)
                .Select(pair => pair.remoteRepoHost);
        });

        public static bool TryGetLocalPathFromUrl(Uri url, List<RepoConfig> repoCandidates, out Request? request)
        {
            var tasks = repoCandidates.Select(
                async (candidate) =>
                {
                    if (candidate.RemoteRepoHostType != null 
                        && TaskUtils.Try(await candidate.RemoteRepoHostType.TryGetLocalPath(url, candidate), out Request? resultingPath))
                    {
                        return resultingPath;
                    }
                    return null;
                });
            var results = Task.WhenAll(tasks).Result.RemoveNulls().ToList();
            if (results.Count == 1)
            {
                request = results[0];
                return true;
            }
            if (results.Count > 1)
            {
                // TO(MAYBE)DO: Seems a bit strange to throw an exception for this. We should probably output this to the
                // console somehow in a parseable way.
                throw new Exception($"Multiple matches for {url}: {string.Join(", ", JsonConvert.SerializeObject(results))}");
            }

            request = null;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localFilePath">The localFilePath requested as `{localFilePath}#{lineNum}:~:text={text}`</param>
        /// <param name="repoCandidates">The list of all repo configs to check against.</param>
        /// <param name="remoteLink"></param>
        /// <returns></returns>
        public static async Task<(bool, RepoConfig?, Uri?)> TryGetRemoteLinkFromPath(GetUrl request, List<RepoConfig> repoCandidates)
        {
            string actualPath = request.File;
            DirectoryInfo? curDir;
            if (File.Exists(actualPath))
            {
                // Forgiveness: Files always have a directory they reside in.
                curDir = new FileInfo(actualPath).Directory!;
            }
            else
            {
                curDir = new DirectoryInfo(actualPath);
            }

            while(curDir != null)
            {
                foreach (var candidate in repoCandidates)
                {
                    if (FileUtils.ArePathsEqual(candidate.Root, curDir.FullName))
                    {
                        return (true, candidate, await candidate.RemoteRepoHostType.GetRemoteLink(request, candidate));
                    }
                }
                curDir = curDir.Parent;
            }

            return (false, null, null);
        }
    }
}
