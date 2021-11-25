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

namespace CoreAPI.Cli
{
    [Verb("get-actions")]
    public class GetActions
    {
        [Option("url", Required = true)]
        public string Url { get; set; } = "";

        [Value(0)]
        public IEnumerable<string> BrowserArgs { get; set; } = new List<string>();

        public async Task<int> ExecuteAsync()
        {
            List<WheelElement> elements = await Get();
            Console.WriteLine(JsonConvert.SerializeObject(elements));
            return 0;
        }

        public async Task<List<WheelElement>> Get()
        {
            List<WheelElement> elements = new();

            if (File.Exists(LinkWheelConfig.TrackedReposFile))
            {
                List<RepoConfig> repoConfigs = RepoConfigFile.Read();
                
                if (OperatingSystem.IsWindows())
                {
                    if (RemoteRepoHosts.TryGetLocalPathFromUrl(new Uri(Url), repoConfigs, out string path))
                    {
                        // TODO: Read the .idelconfig file and populate these. Here's some okay defaults
                        // in the meantime.

                        IconResult iconPath = IconUtils.GetIconForFile(path);

                        elements.Add(new WheelElement()
                        {
                            Name = "Open in Editor",
                            Description = $"Opens {path} in your default editor.",
                            CommandAction = new string[] { path },
                            IconPath = iconPath.Path,
                            IconLazy = new(() => iconPath.Icon),
                        });

                        IconResult dirIconPath = IconUtils.GetIconForFile(@"C:\Windows\explorer.exe");
                        elements.Add(new WheelElement()
                        {
                            Name = "Show in Explorer",
                            Description = $"Shows {path} in your file explorer.",
                            // The "/select" argument requires backslashes. That comma is intentional. See
                            //   https://stackoverflow.com/questions/13680415/how-to-open-explorer-with-a-specific-file-selected
                            //   https://ss64.com/nt/explorer.html
                            CommandAction = new string[] { @"C:\Windows\explorer.exe", $"/select,\"{path.Replace("/", "\\")}\"" },
                            IconPath = dirIconPath.Path,
                            IconLazy = new(() => dirIconPath.Icon),
                        });
                    }
                }
                else
                {
                    throw new NotImplementedException("Cannot get actions for non-Windows systems yet.");
                }
            }

            IconResult browserIcon = IconUtils.DefaultBrowserIcon;

            if (elements.Count == 0)
            {
                if (IconUtils.TryGetWebsiteIconPath(new Uri(Url), out string? localCachePath))
                {
                    elements.Add(new WheelElement()
                    {
                        Name = "Open in Browser",
                        Description = $"Opens {Url} in your default browser.",
                        CommandAction = BrowserArgs,
                        IconPath = localCachePath,
                        IconLazy = new(() => new((Bitmap)Image.FromFile(localCachePath))),
                        IconPathSecondary = browserIcon.Path,
                        IconSecondaryLazy = new(() => browserIcon.Icon),
                    });
                }
                else
                {
                    elements.Add(new WheelElement()
                    {
                        Name = "Open in Browser",
                        Description = $"Opens {Url} in your default browser.",
                        CommandAction = BrowserArgs,
                        IconPath = "",
                        IconLazy = new(() => null),
                        IconPathSecondary = browserIcon.Path,
                        IconSecondaryLazy = new(() => browserIcon.Icon),
                    });
                }
            }
            else
            {
                IconResult urlIcon = IconUtils.GetIconForUrl(Url);
                elements.Add(new WheelElement()
                {
                    Name = "Open in Browser",
                    Description = $"Opens {Url} in your default browser.",
                    CommandAction = BrowserArgs,
                    IconPath = urlIcon.Path,
                    IconLazy = new(() => urlIcon.Icon),
                    IconPathSecondary = browserIcon.Path,
                    IconSecondaryLazy = new(() => browserIcon.Icon),
                });
            }

            return elements;
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
