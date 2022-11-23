using CoreAPI.Config;
using CoreAPI.RemoteHosts;
using CoreAPI.Icons;
using CoreAPI.OutputFormat;
using CoreAPI.Models;
using CoreAPI.Utils;
using CommandLine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using System;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Linq;
using Microsoft.Win32;

namespace CoreAPI.Cli
{
    [Verb("get-actions", HelpText = HelpText)]
    public class GetActions
    {
        [Option("url", Required = true)]
        public string Url { get; set; } = "";

        public const string HelpText = "Returns a JSON object containing the actions that are " +
            "defined within all relevant .idelconfig files. If you would like to receive a " +
            "specific value or object, consider using the global `--format` flag along with " +
            "JSONPath string.";

        [Value(0, HelpText = "The exact arguments to open this URL in your browser. If unset " +
            "set, the arguments will be fetched from the operating system's defaults.")]
        public IEnumerable<string> BrowserArgs { get; set; } = new List<string>();

        public async Task<OutputData> ExecuteAsync()
        {
            List<IdelAction> actions = await Get();

            return new OutputData(0, new() { ["actions"] = actions }, "(=actions=)");
        }

        private void FormatEntries(JObject jObject, OutputData outputData)
        {
            OutputFormatter formatter = new();
            foreach (var kvp in jObject)
            {
                if (kvp.Value is JObject childObject)
                {
                    FormatEntries(childObject, outputData);
                }
                else if (kvp.Value?.Type == JTokenType.String)
                {
                    jObject[kvp.Key] = formatter.GetOutput(outputData, (string?)kvp.Value);
                }
            }
        }

        private async Task<List<IdelAction>> GetActionsForFile(string filePath, Request request, Dictionary<string, IdelActionDefinition> actionDefinitions)
        {
            string idelConfigPath = filePath;
            Dictionary<string, JObject> actions = new();
            List<IdelAction> completedActions = new();

            string prevWd = Directory.GetCurrentDirectory();
            // Forgiveness: filePath is always a file.
            string currentWd = Path.GetDirectoryName(filePath)!;
            Directory.SetCurrentDirectory(currentWd);
            if (File.Exists(idelConfigPath))
            {
                IdelConfig? repoIdelConfig = JsonConvert.DeserializeObject<IdelConfig>(
                    await File.ReadAllTextAsync(idelConfigPath));
                if (repoIdelConfig?.Definitions is not null)
                {
                    // Add the file path as the action source.
                    foreach (IdelActionDefinition value in repoIdelConfig.Definitions.Values)
                    {
                        value.ActionSourceFile = filePath;
                    }
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

            foreach (var nameActionPair in actions)
            {
                // Forgiveness: we guaranteed this exists above.
                IdelActionDefinition definition = actionDefinitions[(string)nameActionPair.Value["definition"]!];


                OutputFormatter formatter = new();
                Dictionary<string, object> formatInfo = new()
                {
                    ["request"] = request,
                    ["name"] = nameActionPair.Key,
                    ["definition"] = definition,
                };
                OutputData data = new(0, formatInfo);
                FormatEntries(nameActionPair.Value, data);

                formatInfo["action"] = nameActionPair.Value;

                // Hacking the output formatter to format these values for us.
                // Here, we get FnMatches.
                string[] fnMatches = formatter.GetOutput(data, definition.FnMatches).Trim().Split(
                    new string[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None
                );
                Matcher matcher = new();
                foreach (string fnmatch in fnMatches)
                {
                    matcher.AddInclude(fnmatch.TrimStart());
                }
                if (!matcher.Match(request.RepoConfig.Root, request.File).HasMatches)
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
                ActionSource source = new()
                {
                    File = filePath,
                    Name = nameActionPair.Key
                };
                completedActions.Add(new IdelAction(
                    priority: priority,
                    command: command,
                    title: title,
                    description: description,
                    iconSource: icon.Path,
                    iconSecondarySource: iconSecondary.Path,
                    source: source
                )
                {
                    Icon = icon.Icon,
                    IconSecondary = iconSecondary.Icon,
                    CommandWorkingDirectory = currentWd,
                });
            }
            Directory.SetCurrentDirectory(prevWd);
            return completedActions;
        }

        public async Task<List<IdelAction>> Get()
        {
            List<IdelAction> completedActions = new();
            List<RepoConfig> repoConfigs = RepoConfigs.All();

            if (RemoteRepoHosts.TryGetLocalPathFromUrl(new Uri(Url), repoConfigs, out Request? request))
            {
                string globalIdelConfigPath = Path.Combine(LinkWheelConfig.DataDirectory, ".idelconfig");
                string repoIdelConfigPath = Path.Combine(request.RepoConfig.Root, ".idelconfig");
                string userIdelConfigPath = Path.Combine(request.RepoConfig.Root, ".user.idelconfig");

                Dictionary<string, IdelActionDefinition> culumulativeActionDefinitions = new();
                Dictionary<string, IdelActionDefinition> tempDefinitions = new();

                completedActions.AddRange(await GetActionsForFile(globalIdelConfigPath, request, culumulativeActionDefinitions));
                completedActions.AddRange(await GetActionsForFile(repoIdelConfigPath, request, tempDefinitions));
                culumulativeActionDefinitions.Update(tempDefinitions);
                completedActions.AddRange(await GetActionsForFile(userIdelConfigPath, request, culumulativeActionDefinitions));
                completedActions = completedActions.GroupBy(action => action.Source?.Name ?? "").Select(list => list.Last()).ToList();
            }

            IconResult browserIcon = IconUtils.DefaultBrowserIcon;

            // If BrowserArgs were not passed, fill them in from the OS-defined values.
            if (!BrowserArgs.Any())
            {
                BrowserArgs = (string[])(await new GetBrowserArgs() { Url = Url }.ExecuteAsync()).Objects["array"];
            }

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
                RegistryKey? registryKey = Registry.ClassesRoot.OpenSubKey(LinkWheelConfig.ApplicationName);
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
