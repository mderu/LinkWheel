﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CliWrap;
using CoreAPI.Cli;
using CoreAPI.Config;
using CoreAPI.Models;
using CoreAPI.Utils;

namespace CoreAPI.RemoteHosts
{
    [HostPriority(0)]
    class GitHub : RemoteRepoHost
    {
        public override Task<(bool, Request?)> TryGetLocalPath(Uri remoteUri, RepoConfig repoConfig)
        {
            string[] requestedParts = remoteUri.PathAndQuery.Split('?')[0].Split('#')[0].Split('/');
            string[] configuredParts = new Uri(repoConfig.RemoteRootUrl).PathAndQuery.Split('/');

            for (int i = 0; i < configuredParts.Length; i++)
            {
                if (configuredParts[i] != requestedParts[i])
                {
                    return Task.FromResult<(bool, Request?)>((false, null));
                }
            }

            // Exit early if the URL does not look like a file hosted on GitHub or GitHub enterprise (i.e., link to a non-file).
            if (requestedParts.Length < 4 || !(requestedParts[3] == "blob" || requestedParts[3] == "tree"))
            {
                return Task.FromResult<(bool, Request?)>((false, null));
            }

            // Magic number explanation:
            // The 5 skipped parts are:
            //   1) empty string (paths are rooted)
            //   2) username or organization
            //   3) repo name
            //   4) 'blob' or 'tree'
            //   5) commit hash or branch name
            string filePath = Path.Combine(
                    repoConfig.Root,
                    string.Join("/", requestedParts.Skip(5)));

            Request request = new(remoteUri.ToString(), filePath, repoConfig);
            var lineParts = new Regex(@"L(?<startLine>\d+)(-L(?<endLine>\d+))?").Match(remoteUri.Fragment);

            if (lineParts.Groups.TryGetValue("startLine", out Group? startLine))
            {
                request.StartLine = int.Parse(startLine.Value);
            }
            if (lineParts.Groups.TryGetValue("endLine", out Group? endLine) && !string.IsNullOrWhiteSpace(endLine.Value))
            {
                request.EndLine = int.Parse(endLine.Value);
            }

            return Task.FromResult<(bool, Request?)>((true, request));
        }

        public override async Task<(bool, RepoConfig?)> TryGetRepoConfig(string pathInRepo)
        {
            // TODO: The logic here will match pretty much any Git repo. We need to see if there's a way
            // we can differentiate GitHub from other git hosting solutions.

            if (!await IsGitInstalled())
            {
                return (false, null);
            }

            // For now, we assume the user didn't super configure their git repo to use something
            // other than "origin". Definitely something we'll have to revisit, but it works for
            // the majority of git users.
            (bool isValid, string remoteOrigin) = await GetRemoteOriginUrl(pathInRepo);
            if (isValid)
            {
                return (
                    true,
                    new RepoConfig()
                    {
                        // Trims ".git"
                        RemoteRootUrl = remoteOrigin[..^4],
                        Root = (await GetRepoRoot(pathInRepo)).root,
                        RawRemoteRepoHostType = nameof(GitHub),
                        RemoteRootRegex = $"{Regex.Escape(remoteOrigin[..^4])}",
                    }
                );
            }
            return (false, null);
        }

        public override async Task<Uri> GetRemoteLink(GetUrl request, RepoConfig repoConfig)
        {
            string fullPath = request.File;
            string relativePath = Path.GetRelativePath(repoConfig.Root, fullPath);

            // Note that even if we get this wrong, GitHub will compensate.
            string blobOrTree = "blob";
            bool isDirectory = Directory.Exists(relativePath);
            if (isDirectory)
            {
                blobOrTree = "tree";
            }

            string remoteBranch;
            // Forgiveness: we know fullPath is not a directory, so it must have a parent directory.
            string localPathDirectory = isDirectory ? fullPath : Path.GetDirectoryName(fullPath)!;
            if (TaskUtils.Try(await GetRemoteBranch(localPathDirectory), out remoteBranch))
            {

            }
            else
            {
                remoteBranch = await GetMostRecentOriginCommitHash(localPathDirectory);
            }

            Uri returnValue = new(Path.Combine(
                repoConfig.RemoteRootUrl,
                blobOrTree,
                remoteBranch,
                relativePath));

            if (request.StartLine is not null)
            {
                StringBuilder newUrl = new(returnValue.ToString());
                newUrl.Append("#L");
                newUrl.Append(request.StartLine.Value);
                if (request.EndLine is not null)
                {
                    newUrl.Append("-L");
                    newUrl.Append(request.EndLine.Value);
                }
                returnValue = new Uri(newUrl.ToString());
            }
            return returnValue;
        }

        public static async Task<(bool, string)> GetRemoteBranch(string directory)
        {
            var stdOutBuffer = new StringBuilder();
            var result = await CliWrap.Cli.Wrap("git")
                .WithArguments("rev-parse --abbrev-ref --symbolic-full-name \"@{u}\"")
                .WithWorkingDirectory(directory)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();

            return (result.ExitCode == 0, result.ExitCode == 0 ? stdOutBuffer.ToString().Trim().Split("/")[1] : "");
        }

        // Notes on origin vs upstream: https://stackoverflow.com/a/9257901/6876989
        public static async Task<string> GetMostRecentOriginCommitHash(string directory)
        {
            var stdOutBuffer = new StringBuilder();
            // TODO: Handle the error caused by nothing being upstream.
            await CliWrap.Cli.Wrap("git")
                .WithArguments("rev-list origin -n 1")
                .WithWorkingDirectory(directory)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .ExecuteAsync();

            return stdOutBuffer.ToString().Trim();
        }

        /// <summary>
        /// Returns the configured origin remote URL.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<(bool, string)> GetRemoteOriginUrl(string path)
        {
            if (!Directory.Exists(path))
            {
                if (File.Exists(path))
                {
                    // Forgiveness: we know Path is a file, so it must have a directory.
                    path = new FileInfo(path).Directory!.FullName;
                }
                else
                {
                    return new(false, $"No such file or directory: {path}");
                }
            }
            var stdOutBuffer = new StringBuilder();
            var result = await CliWrap.Cli.Wrap("git")
                .WithArguments("config --get remote.origin.url")
                .WithWorkingDirectory(path)
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

            if (File.Exists(localPath))
            {
                // Forgiveness: must have a directory if it exists as a file.
                localPath = Path.GetDirectoryName(localPath)!;
            }

            var result = await CliWrap.Cli.Wrap("git")
                .WithArguments("rev-parse --show-toplevel")
                .WithWorkingDirectory(localPath)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();

            return (success: result.ExitCode == 0, root: stdOutBuffer.ToString().Trim());
        }
    }
}
