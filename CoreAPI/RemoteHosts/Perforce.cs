using CliWrap;
using CoreAPI.Cli;
using CoreAPI.Config;
using CoreAPI.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoreAPI.RemoteHosts
{
    [HostPriority(0)]
    class Perforce : RemoteRepoHost
    {
        public override async Task<(bool, string)> TryGetLocalPath(Uri remoteUri, RepoConfig repoConfig)
        {
            string[] requestedParts = remoteUri.PathAndQuery.Split("#")[0].Split("?")[0].Split('/');
            string[] configuredParts = new Uri(repoConfig.RemoteRootUrl).PathAndQuery.Split('/');

            int partIndex;
            for (partIndex = 0; partIndex < configuredParts.Length; partIndex++)
            {
                if (configuredParts[partIndex] != requestedParts[partIndex])
                {
                    return (false, "");
                }
            }

            if (requestedParts[partIndex++] != "files")
            {
                return (false, "");
            }

            // Note that we don't cache the stream because it can be changed.
            var stream = await GetStream(repoConfig);
            // TODO: Find out what path is used if the repo doesn't use streams.
            if (stream is null)
            {
                return (false, "");
            }
            string[] streamParts = stream[2..].Split("/");
            
            for (int streamPartIndex = 0; streamPartIndex < streamParts.Length; streamPartIndex++)
            {
                if (requestedParts[partIndex] != streamParts[streamPartIndex])
                {
                    return (false, "");
                }
                partIndex++;
            }

            return (true, Path.Combine(repoConfig.Root, string.Join('/', requestedParts.Skip(partIndex))));
        }

        public override async Task<(bool, RepoConfig?)> TryGetRepoConfig(string localRepoRoot)
        {
            if (!await IsP4Installed())
            {
                return (false, null);
            }

            var logins = await GetP4Logins();

            List<Task> getAllSwarmUrlTasks = new();

            (string swarmUrl, List<string> roots)[] swarmToClients = new (string swarmUrl, List<string> roots)[logins.Count];
            for (int i = 0; i < logins.Count; i++)
            {
                (string port, string username) = logins[i];
                string? url = await GetSwarmUrl(port, username);
                List<string> clientRoots = await GetClientRootsForServer(port, username);
                if (url is null)
                {
                    continue;
                }
                swarmToClients[i] = (url, clientRoots);
            }

            for (int i = 0; i < logins.Count; i++)
            {
                string swarmUrl = swarmToClients[i].swarmUrl;
                List<string> clients = swarmToClients[i].roots;
                string port = logins[i].port;
                string username = logins[i].username;

                foreach(string client in clients)
                {
                    if (FileUtils.IsWithinPath(client, localRepoRoot))
                    {
                        return (
                            true, 
                            new RepoConfig()
                            {
                                Root = client,
                                RemoteRootUrl = swarmUrl,
                                RemoteRepoHostKeys = new Dictionary<string, string>()
                                {
                                    ["port"] = port,
                                    ["username"] = username,
                                },
                                RawRemoteRepoHostType = nameof(Perforce)
                            });
                    }
                }
            }

            return (false, null);
        }

        public override async Task<Uri> GetRemoteLink(GetUrl request, RepoConfig repoConfig)
        {
            string actualPath = Path.GetRelativePath(repoConfig.Root, request.File);

            string? stream = await GetStream(repoConfig);

            if (stream is null)
            {
                throw new Exception("Unable to get remote links for Perforce clients that do not use streams.");
            }

            string streamDepotPath = stream.Replace("//", "/");

            Uri returnValue = new(
                Path.Combine(
                    repoConfig.RemoteRootUrl, 
                    "files", 
                    streamDepotPath.Replace("//", ""), 
                    actualPath
            ));

            if (request.StartLine is not null)
            {
                StringBuilder newUrl = new(returnValue.ToString());
                newUrl.Append("#");
                newUrl.Append(request.StartLine.Value);
                returnValue = new Uri(newUrl.ToString());
            }
            return returnValue;
        }

        private static async Task<List<string>> GetClientRootsForServer(string port, string username)
        {
            var stdOutBuffer = new StringBuilder();
            await CliWrap.Cli.Wrap("p4")
                .WithArguments($"-ztag -F %Host%,%Root% -p {port} -u {username} clients -u {username}")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();

            // Only tested on Windows.
            string expectedPrefix = $"{Environment.MachineName},";

            List<string> results = new();
            foreach (string line in stdOutBuffer.ToString().Split(Environment.NewLine))
            {
                if (line.StartsWith(expectedPrefix))
                {
                    results.Add(line[expectedPrefix.Length..]);
                }
            }

            return results;
        }

        private static async Task<string> GetConcreteStream(string streamPath, string port, string username)
        {
            // Returns the first non-virtual stream in the stream's ancestry.
            // This is useful for finding the actual file path name in the depot.
            var stdOutBuffer = new StringBuilder();
            await CliWrap.Cli.Wrap("p4")
                .WithArguments($"-p {port} -u {username} -F %type%,%title%,%parent% streams -F \"Stream={streamPath}\"")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();
            string result = stdOutBuffer.ToString().Trim();
            if (result.StartsWith("virtual,"))
            {
                return await GetConcreteStream(result.Split(",")[2], port, username);
            }
            return streamPath;
        }

        /// <summary>
        /// Returns the depot path for the given stream.
        /// </summary>
        /// <param name="repoConfig"></param>
        /// <returns></returns>
        private static async Task<string?> GetStream(RepoConfig repoConfig)
        {
            string port = repoConfig.RemoteRepoHostKeys["port"];
            string username = repoConfig.RemoteRepoHostKeys["username"];
            var stdOutBuffer = new StringBuilder();
            await CliWrap.Cli.Wrap("p4")
                .WithArguments($"-p {port} -u {username} -ztag -F %Host%,%Root%,%Stream% clients -u {username}")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();

            // MachineName only tested on Windows.
            string expectedPrefix = $"{Environment.MachineName},{repoConfig.Root},";

            List<string> results = new();
            foreach (string line in stdOutBuffer.ToString().Split(Environment.NewLine))
            {
                if (line.StartsWith(expectedPrefix))
                {
                    return await GetConcreteStream(line[expectedPrefix.Length..], port, username);
                }
            }
            return null;
        }

        private static async Task<string?> GetSwarmUrl(string port, string username)
        {
            var stdOutBuffer = new StringBuilder();
            await CliWrap.Cli.Wrap("p4")
                .WithArguments($"-p {port} -u {username} -F %serverID% serverid")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();
            string serverId = stdOutBuffer.ToString();
            stdOutBuffer.Clear();

            await CliWrap.Cli.Wrap("p4")
                .WithArguments($"-p {port} -u {username} property -l -n P4.Swarm.URL")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();

            List<string> lines = stdOutBuffer.ToString().Trim().Split(Environment.NewLine)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            if (lines.Count == 0)
            {
                return null;
            }
            if (lines.Count == 1)
            {
                return lines[0].Split(" = ")[1];
            }
            foreach (string line in lines)
            {
                if (line.StartsWith($"P4.Swarm.Url.{serverId}"))
                {
                    return lines[0].Split(" = ")[1];
                }
            }
            throw new InvalidOperationException("Unable to determine the correct Swarm URL.");
        }

        private static async Task<bool> IsP4Installed()
        {
            var stdOutBuffer = new StringBuilder();
            await CliWrap.Cli.Wrap(OperatingSystem.IsWindows() ? "where" : "which")
                .WithArguments("p4")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();
            return stdOutBuffer.ToString().Trim().Length != 0;
        }

        private static async Task<string[]> GetLocalP4Roots(string port, string user)
        {
            var stdOutBuffer = new StringBuilder();
            var result = await CliWrap.Cli.Wrap("p4")
                .WithArguments($"-p {port} -u {user} -F %domainMount% clients -u {user}")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();

            // TODO: Call the following on each, and check to make sure the hostname matches:
            //     p4 -ztag -F %Host% -c {clientName} client -o
            // filter out any entries that don't match.

            return stdOutBuffer.ToString().Trim().Split(Environment.NewLine);
        }

        private static async Task<List<(string port, string username)>> GetP4Logins()
        {
            var stdOutBuffer = new StringBuilder();
            var result = await CliWrap.Cli.Wrap("p4")
                .WithArguments("tickets")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();

            string contents = stdOutBuffer.ToString().Trim();

            if (contents.Length == 0 || result.ExitCode != 0)
            {
                return new();
            }

            List<(string port, string username)> results = new();

            // Example line:
            // perforce:1666 (username) 50E4385D7DE7B93A198D801D08EE4568
            Regex p4TicketParser = new(@"(?<port>\S+) \((?<username>\S+)\) \S+");
            foreach (string line in contents.Split(Environment.NewLine))
            {
                var match = p4TicketParser.Match(line);
                if (match.Success)
                {
                    results.Add((match.Groups["port"].Value, match.Groups["username"].Value));
                }
            }
            return results;
        }
    }
}
