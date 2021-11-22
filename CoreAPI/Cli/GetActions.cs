using CommandLine;
using CoreAPI.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CoreAPI.Config;
using CoreAPI.Installers;
using CoreAPI.RemoteHosts;
using CoreAPI.Icons;
using Newtonsoft.Json;

namespace CoreAPI.Cli
{
    [Verb("get-actions")]
    public class GetActions
    {
        [Option("url", Required = true)]
        public string Url { get; set; }

        [Value(0)]
        public IEnumerable<string> BrowserArgs { get; set; }

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

                    string parentDirectory = Path.GetDirectoryName(path);
                    IconResult dirIconPath = IconUtils.GetIconForFile(parentDirectory);
                    elements.Add(new WheelElement()
                    {
                        Name = "Show in Explorer",
                        Description = $"Opens {parentDirectory} in your file explorer.",
                        CommandAction = new string[] { @"C:\Windows\explorer.exe", parentDirectory },
                        IconPath = dirIconPath.Path,
                        IconLazy = new(() => dirIconPath.Icon),
                    });
                }
            }

            IconResult urlIcon = IconUtils.GetIconForUrl(Url);
            IconResult browserIcon = IconUtils.DefaultBrowserIcon;
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

            return elements;
        }

        private static bool IsEnabled()
        {
            if (OperatingSystem.IsWindows())
            {
                return bool.Parse((string)Registry.ClassesRoot.OpenSubKey(nameof(LinkWheel)).GetValue(LinkWheelConfig.Registry.EnabledValue, "false"));
            }
            else
            {
                throw new NotImplementedException("No support for non Windows yet");
            }
        }
    }
}
