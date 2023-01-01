using CliWrap;
using CoreAPI.Cli;
using CoreAPI.Config;
using CoreAPI.Models;
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
        public override async Task<(bool, Request?)> TryGetLocalPath(Uri remoteUri, RepoConfig repoConfig)
        {
            string[] requestedParts =
                remoteUri.PathAndQuery
                    .Split("#")[0]
                    .Split("?")[0]
                    // Empty entries are removed because Swarm accepts any number of slashes after "/files/"
                    .Split('/', StringSplitOptions.RemoveEmptyEntries);
            string[] configuredParts =
                new Uri(repoConfig.RemoteRootUrl).PathAndQuery
                    .Split('/', StringSplitOptions.RemoveEmptyEntries);

            int partIndex;
            for (partIndex = 0; partIndex < configuredParts.Length; partIndex++)
            {
                if (configuredParts[partIndex] != requestedParts[partIndex])
                {
                    return (false, null);
                }
            }

            // Exit early if the link does not look like a file served from Swarm.
            if (requestedParts[partIndex++] != "files")
            {
                return (false, null);
            }

            string depotPath = "//" + string.Join('/', requestedParts[partIndex..]);
            var localPath = await GetLocalPath(repoConfig, depotPath);

            if (string.IsNullOrEmpty(localPath))
            {
                return (false, null);
            }

            Request request = new(remoteUri.ToString(), localPath, repoConfig);
            // Here we assume other fragments can exist, so we look for a fragment with only numbers.
            // AFAIK, Swarm can't link to a range of line numbers.
            var lineFragment = new Regex(@"(^|&)[Ll]?(?<lineNum>\d+)(&|$)").Match(remoteUri.Fragment);

            if (lineFragment.Success)
            {
                request.StartLine = int.Parse(lineFragment.Groups["lineNum"].Value);
            }

            return (true, request);
        }

        public override async Task<(bool, RepoConfig?)> TryGetRepoConfig(string localRepoRoot)
        {
            if (!await IsP4Installed())
            {
                return (false, null);
            }

            var logins = await GetP4Logins();

            List<Task> getAllSwarmUrlTasks = new();

            for (int i = 0; i < logins.Count; i++)
            {
                (string port, string username) = logins[i];
                string? swarmUrl = await GetSwarmUrl(port, username);
                if (swarmUrl is null)
                {
                    continue;
                }
                List<(string clientName, string path)> clients = await GetClientsForServer(port, username);

                foreach((string clientName, string path) in clients)
                {
                    RepoConfig potentialConfig = new()
                    {
                        Root = path,
                        RemoteRootUrl = swarmUrl,
                        RemoteRepoHostKeys = new Dictionary<string, string>()
                        {
                            ["port"] = port,
                            ["username"] = username,
                            ["client"] = clientName,
                        },
                        RawRemoteRepoHostType = nameof(Perforce),
                        RemoteRootRegex = $"{Regex.Escape(swarmUrl)}",
                    };

                    if (await IsPathInWorkspaceView(potentialConfig, localRepoRoot))
                    {
                        return (true, potentialConfig);
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

            Uri returnValue = new(
                Path.Combine(
                    repoConfig.RemoteRootUrl,
                    "files",
                    stream.Replace("//", ""),
                    actualPath.Replace('\\', '/')
            ));

            if (request.StartLine is not null)
            {
                StringBuilder newUrl = new(returnValue.ToString());
                newUrl.Append('#');
                newUrl.Append(request.StartLine.Value);
                returnValue = new Uri(newUrl.ToString());
            }
            return returnValue;
        }

        private static async Task<List<(string clientName, string path)>> GetClientsForServer(string port, string username)
        {
            var stdOutBuffer = new StringBuilder();
            await CliWrap.Cli.Wrap("p4")
                // Separator logic: Hostnames can only contain [A-Z], [a-z], \., -
                //                  @ is a reserved character for client names.
                .WithArguments($"-ztag -F %Host%@%client%@%Root% -p {port} -u {username} clients -u {username}")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();

            // Note: I've only tested this on Windows.
            string expectedPrefix = $"{Environment.MachineName}@";

            List<(string clientName, string path)> results = new();
            foreach (string line in stdOutBuffer.ToString().Split(Environment.NewLine))
            {
                if (line.StartsWith(expectedPrefix))
                {
                    string[] parts = line[expectedPrefix.Length..].Split('@', 2);
                    results.Add((clientName: parts[0], path: parts[1]));
                }
            }

            return results;
        }

        private static async Task<bool> IsPathInWorkspaceView(RepoConfig repoConfig, string path)
        {
            string port = repoConfig.RemoteRepoHostKeys["port"];
            string username = repoConfig.RemoteRepoHostKeys["username"];
            string clientName = repoConfig.RemoteRepoHostKeys["client"];
            var stdOutBuffer = new StringBuilder();
            // Paths ending in direcotry separator characters return a null directory error.
            string trimmedPath = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            await CliWrap.Cli.Wrap("p4")
                // Dark magic explanation:
                //
                // The following command returns a non-empty line iff the path is currently in the workspace view.
                //
                // There is a branch in the logic that `where` returns, depending on if the path given is a local file,
                // or if it is "." If you pass a local file, it will return the expected data and the %clientFile%
                // variable. If you pass a valid directory, it will return "%path% - must refer to client '%client%'".
                // Note that if you pass an invalid file, you'll get the %path% variable, but not the %client% variable,
                // i.e., "Path '%path%' is not under client's root '%root%'.". Note that %client% is not returned on
                // this error, so we use that variable instead.
                //
                // Putting both of these variables together means that at least one of them will be present if the file
                // exists in the workspace view.
                .WithArguments(
                    $"-p {port} -u {username} -ztag -F %client%%clientFile% -c {clientName} where {trimmedPath}")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();
            return stdOutBuffer.ToString().Trim().Length > 0;
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
        /// Returns the client path associated with the given file, or null if the file is not within the workspace view.
        /// </summary>
        private static async Task<string?> GetLocalPath(RepoConfig repoConfig, string depotPath)
        {
            string port = repoConfig.RemoteRepoHostKeys["port"];
            string username = repoConfig.RemoteRepoHostKeys["username"];
            string client = repoConfig.RemoteRepoHostKeys["client"];
            var stdOutBuffer = new StringBuilder();
            await CliWrap.Cli.Wrap("p4")
                .WithArguments($"-p {port} -u {username} -c {client} -ztag -F %path% where {depotPath}")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();
            return stdOutBuffer.Length > 0 ? stdOutBuffer.ToString().Trim() : null;
        }

        /// <summary>
        /// Returns the depot path for the given stream.
        /// </summary>
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
            string serverId = stdOutBuffer.ToString().Trim();
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
            string? fallbackServer = null;
            foreach (string line in lines)
            {
                if (line.StartsWith("P4.Swarm.Url = ", StringComparison.OrdinalIgnoreCase))
                {
                    fallbackServer = line.Split(" = ")[1].Trim();
                    continue;
                }
                if (line.StartsWith($"P4.Swarm.Url.{serverId}", StringComparison.OrdinalIgnoreCase))
                {
                    return line.Split(" = ")[1].Trim();
                }
            }
            if (!string.IsNullOrWhiteSpace(fallbackServer))
            {
                return fallbackServer;
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
