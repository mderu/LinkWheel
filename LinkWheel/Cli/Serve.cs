using CommandLine;
using LinkWheel.CodeHosts;
using LinkWheel.Icons;
using LinkWheel.InternalConfig;
using LinkWheel.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LinkWheel.Cli
{
    [Verb("serve")]
    class Serve
    {
        [Option("url", Required = true)]
        public string Url { get; set; }


        [Value(0)]
        public IEnumerable<string> BrowserArgs { get; set; }

        public int Execute(Point cursorPosition)
        {
            // Always ensure LinkWheel is installed before running a command. No point in making the user
            // run an install command before being able to use the executable.
            Installer.EnsureInstalled();

            if (!IsEnabled())
            {
                CliUtils.SimpleInvoke(BrowserArgs);
                return 0;
            }

            var actions = GetActions();

            if (actions.Count == 1)
            {
                CliUtils.SimpleInvoke(actions[0].CommandAction);
            }
            else
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.Run(new Form1(cursorPosition, actions));
            }
            return 0;
        }

        private List<WheelElement> GetActions()
        {
            List<WheelElement> elements = new();

            if (File.Exists(LinkWheelConfig.TrackedReposFile))
            {
                List<RepoConfig> repoConfigs = RepoConfigFile.Read();

                if (RemoteRepoHosts.TryGetLocalPathFromUrl(new Uri(Url), repoConfigs, out string path))
                {
                    // TODO: Read the .idelconfig file and populate these. Here's some okay defaults
                    // in the meantime.

                    elements.Add(new WheelElement()
                    {
                        Name = "Open in Editor",
                        Description = $"Opens {path} in your default editor.",
                        CommandAction = new string[] { path },
                        IconFetcher = () => JumboIcons.GetJumboIcon(path)
                    });

                    string parentDirectory = Path.GetDirectoryName(path);
                    elements.Add(new WheelElement()
                    {
                        Name = "Show in Explorer",
                        Description = $"Opens {parentDirectory} in your file explorer.",
                        CommandAction = new string[] { @"C:\Windows\explorer.exe", parentDirectory },
                        IconFetcher = () => JumboIcons.GetJumboIcon(parentDirectory)
                    });
                }
            }

            elements.Add(new WheelElement()
            {
                Name = "Open in Browser",
                Description = $"Opens {Url} in your default browser.",
                CommandAction = BrowserArgs,
                IconFetcher = () => IconUtils.Compose(
                    IconUtils.RoundCorners(AppleTouchIcons.GetFromUrl(new Uri(Url))), 
                    IconUtils.DefaultBrowserIcon)
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
