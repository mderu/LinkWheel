using CommandLine;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using CoreAPI.Config;
using CoreAPI.RemoteHosts;
using CoreAPI.Icons;
using Newtonsoft.Json;
using CoreAPI.OutputFormat;
using CoreAPI.Models;
using Newtonsoft.Json.Linq;
using CoreAPI.Utils;
using Microsoft.Extensions.FileSystemGlobbing;

namespace CoreAPI.Cli
{
    [Verb("get-actions")]
    public class GetActions
    {
        [Option("url", Required = true)]
        public string Url { get; set; } = "";

        [Value(0)]
        public IEnumerable<string> BrowserArgs { get; set; } = new List<string>();

        public async Task<OutputData> ExecuteAsync()
        {
            List<IdelAction> actions = await Get();

            return new OutputData(0, new() { ["actions"] = actions }, "(=actions=)");
        }

        public async Task<List<IdelAction>> Get()
        {
            List<IdelAction> completedActions = new();

            if (File.Exists(LinkWheelConfig.TrackedReposFile))
            {
                List<RepoConfig> repoConfigs = RepoConfigFile.Read();

                if (RemoteRepoHosts.TryGetLocalPathFromUrl(new Uri(Url), repoConfigs, out Request? unforgivenRequest))
                {
                    // Forgiveness: if the try passes, it isn't null.
                    Request request = unforgivenRequest!;

                    string idelConfigPath = Path.Combine(request.RepoConfig.Root, ".idelconfig");
                    Dictionary<string, IdelActionDefinition> actionDefinitions = new();
                    Dictionary<string, JObject> actions = new();

                    // TODO: Repeat this for user-only idelconfigs (one in HOME and the other in as `./.user.idelconfig`.
                    string prevCwd = Directory.GetCurrentDirectory();
                    Directory.SetCurrentDirectory(request.RepoConfig.Root);
                    if (File.Exists(idelConfigPath))
                    {
                        IdelConfig? repoIdelConfig = JsonConvert.DeserializeObject<IdelConfig>(await File.ReadAllTextAsync(idelConfigPath));
                        if (repoIdelConfig?.Definitions is not null)
                        {
                            actionDefinitions.Update(repoIdelConfig.Definitions);
                        }
                        if (repoIdelConfig?.Actions is not null)
                        {
                            foreach (var nameActionPair in repoIdelConfig.Actions)
                            {
                                if (!nameActionPair.Value.ContainsKey("definition"))
                                {
                                    throw new Exception($"Action {nameActionPair.Key} does not contain the key " +
                                        $"`definition`. This value must be a string that matches a name in the " +
                                        $"`definitions` object.");
                                }
                                if (nameActionPair.Value["definition"] is null 
                                    || nameActionPair.Value["definition"]?.Type != JTokenType.String)
                                {
                                    throw new Exception($"Action {nameActionPair.Key} must be a string that matches " +
                                        $"a name in the `definitions` object.");
                                }
                                // Forgiveness: the above if statement guarantees this string isn't null.
                                if (!actionDefinitions.ContainsKey((string)nameActionPair.Value["definition"]!))
                                {
                                    throw new Exception($"Action {nameActionPair.Key} must match a name in the " +
                                        $"`definitions` object.");
                                }
                            }
                            actions.Update(repoIdelConfig.Actions);
                        }
                    }

                    foreach(var nameActionPair in actions)
                    {
                        // Forgiveness: we guaranteed this exists above.
                        IdelActionDefinition definition = actionDefinitions[(string)nameActionPair.Value["definition"]!];
                        Dictionary<string, object> formatInfo = new ()
                        {
                            ["request"] = request,
                            ["action"] = nameActionPair.Value,
                            ["name"] = nameActionPair.Key,
                            ["definition"] = definition,
                        };

                        // Hacking the output formatter to format these values for us.
                        // Here, we get FnMatches.
                        OutputFormatter formatter = new();
                        OutputData data = new(0, formatInfo);
                        string[] fnMatches = formatter.GetOutput(data, definition.FnMatches).Trim().Split(
                            new string[] { "\r\n", "\r", "\n" },
                            StringSplitOptions.None
                        );
                        Matcher matcher = new();
                        foreach (string fnmatch in fnMatches)
                        {
                            matcher.AddInclude(fnmatch.TrimStart());
                        }
                        if (!matcher.Match(request.RelativePath).HasMatches)
                        {
                            continue;
                        }

                        int priority = definition.Priority;
                        string command;
                        if (OperatingSystem.IsWindows() && definition.BatchCommand != null)
                        {
                            command = formatter.GetOutput(data, definition.BatchCommand);
                        }
                        else if (definition.BashCommand != null)
                        {
                            command = formatter.GetOutput(data, definition.BatchCommand);
                        }
                        else
                        {
                            throw new ArgumentException(
                                $"Either `batchCommand` or `batchCommand` must be " +
                                $"specified for action {nameActionPair.Key}.");
                        }
                        string title = formatter.GetOutput(data, definition.Title);
                        string description = formatter.GetOutput(data, definition.Description);
                        IconResult icon = new(null, "");
                        IconResult iconSecondary = new(null, "");
                        if (definition.Icon is not null)
                        {
                            icon = IconUtils.FetchIcon(formatter.GetOutput(data, definition.Icon));
                        }
                        if (definition.IconSecondary is not null)
                        {
                            iconSecondary = IconUtils.FetchIcon(formatter.GetOutput(data, definition.IconSecondary));
                        }
                        completedActions.Add(new IdelAction(
                            priority: priority,
                            command: command,
                            title: title,
                            description: description,
                            iconSource: icon.Path,
                            iconSecondarySource: iconSecondary.Path
                        )
                        {
                            Icon = icon.Icon,
                            IconSecondary = iconSecondary.Icon,
                            CommandWorkingDirectory = request.RepoConfig.Root,
                        });
                    }
                    Directory.SetCurrentDirectory(prevCwd);
                }
            }

            IconResult browserIcon = IconUtils.DefaultBrowserIcon;
            // Special case pass-through websites: don't bother trying to grab icons if they are unrelated to your repos.
            // We do this so we don't make all non-repo links slower to open (e.g., YouTube, Drive, Facebook, Amazon, etc).
            // The case where this is particularly bad is where the website linked to is slow or dead, and it eventually
            // gives up on finding an icon.
            if (completedActions.Count == 0)
            {
                if (IconUtils.TryGetCachedWebsiteIconPath(new Uri(Url), out string? localCachePath))
                {
                    completedActions.Add(new IdelAction(
                        priority: -100,
                        command: CliUtils.JoinToCommandLine(BrowserArgs),
                        title: "Open in Browser",
                        description: $"Opens {Url} in your default browser.",
                        iconSource: localCachePath,
                        iconSecondarySource: browserIcon.Path
                    ));
                }
                else
                {
                    completedActions.Add(new IdelAction(
                        priority: -100,
                        command: CliUtils.JoinToCommandLine(BrowserArgs),
                        title: "Open in Browser",
                        description: $"Opens {Url} in your default browser.",
                        iconSource: "",
                        iconSecondarySource: browserIcon.Path
                    ));
                }
            }
            else
            {
                IconResult urlIcon = IconUtils.GetIconForUrl(Url);
                completedActions.Add(new IdelAction(
                    priority: -100,
                    command: CliUtils.JoinToCommandLine(BrowserArgs),
                    title: "Open in Browser",
                    description: $"Opens {Url} in your default browser.",
                    iconSource: urlIcon.Path,
                    iconSecondarySource: browserIcon.Path
                )
                {
                    Icon = urlIcon.Icon,
                    IconSecondary = browserIcon.Icon,
                });
            }

            return completedActions;
        }

        private static bool IsEnabled()
        {
            if (OperatingSystem.IsWindows())
            {
                RegistryKey? registryKey = Registry.ClassesRoot.OpenSubKey(nameof(LinkWheel));
                return bool.Parse((string?)registryKey?.GetValue(LinkWheelConfig.Registry.EnabledValue, "false")
                    ?? "false");
            }
            else
            {
                throw new NotImplementedException("No support for non Windows yet");
            }
        }
    }
}
