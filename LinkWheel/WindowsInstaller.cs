using LinkWheel.PInvoke;
using LinkWheel.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;

namespace LinkWheel
{
    public class WindowsInstaller
    {
        public WindowsInstaller() { }

        public void EnsureInstalled()
        {
            if(!IsInstalled())
            {
                ForceInstall();
            }
        }

        public bool IsInstalled()
        {
            return Registry.CurrentUser.OpenSubKey(LinkWheelConfig.Registry.ClassKey) != null;
        }

        public void ForceInstall()
        {
            ElevateProcess(out bool isElevatedProcess);
            if (isElevatedProcess)
            {
                InterceptBrowserShellCommands();
                WriteRegistryClassKey();
            }
        }

        public void Uninstall()
        {
            ElevateProcess(out bool isElevatedProcess);
            if (isElevatedProcess)
            {
                RevertBrowserShellCommands();
                RemoveRegistryClassKey();
            }
        }

        public bool IsEnabled()
        {
            return bool.Parse((string)Registry.ClassesRoot.OpenSubKey(nameof(LinkWheel)).GetValue(LinkWheelConfig.Registry.EnabledValue, "false"));
        }

        public void Disable()
        {
            Registry.CurrentUser.OpenSubKey(LinkWheelConfig.Registry.ClassKey, true).SetValue(LinkWheelConfig.Registry.EnabledValue, "false");
        }

        public void Enable()
        {
            Registry.CurrentUser.OpenSubKey(LinkWheelConfig.Registry.ClassKey, true).SetValue(LinkWheelConfig.Registry.EnabledValue, "true");
        }

        /// <summary>
        /// If this process is elevated, returns true. Otherwise, starts and waits for an elevated
        /// process to do the install for us. This allows us to seemlessly install LinkWheel the first
        /// time we run any LinkWheel command.
        /// </summary>
        /// <returns>True if the program is elevated, false otherwise.</returns>
        private static void ElevateProcess(out bool isElevatedProcess)
        {
            try
            {
                Registry.LocalMachine.CreateSubKey(LinkWheelConfig.Registry.ClassKey, true).SetValue("", "");
                Registry.CurrentUser.CreateSubKey(LinkWheelConfig.Registry.ClassKey, true).SetValue("", "");
                isElevatedProcess = true;
            }
            catch (SecurityException)
            {
                string[] commandline = Environment.GetCommandLineArgs();
                ProcessStartInfo startInfo = new(commandline[0], string.Join(" ", commandline[1..]))
                {
                    Verb = "runas",
                    UseShellExecute = true,
                };
                Process.Start(startInfo).WaitForExit();
                isElevatedProcess = false;
            }
        }

        /// <remarks>
        /// For capturing links on Windows, we need to intercept the internet browser.
        /// 
        /// Currently, we achieve this by re-writing the shell/open/command registry key for all the user's
        /// installed internet browsers. This has the advantage of allowing the user to manage their default
        /// browser through Windows, i.e., LinkWheel will work even if the user changes their default browser.
        /// 
        /// The drawback is that if the user installs a new browser and makes it default, we won't be able to
        /// intercept it. Luckily, this can be detected without needing administrator priviledges, so we can
        /// simply re-install.
        /// 
        /// Another drawback is that it's possible the intercepted browser will reset its registry data upon
        /// updates. This is probably unlikely, but having to fight the browser in this way would be a frustrating
        /// end-user experience. If we find this to be the case for any statistically relevant browser, we should
        /// opt in to making LinkWheel a registered internet browser in Windows, and have the configuration for
        /// the default browser to be controlled by the System Tray application.
        /// </remarks>
        private static void InterceptBrowserShellCommands()
        {
            string[] args = Environment.GetCommandLineArgs();
            // Dumb hack for running while debugging. Use the EXE instead of the DLL so we don't need to kick off with
            // dotnet.exe.
            string executablePath = args[0].EndsWith(".dll") ? args[0][..^4] + ".exe" : args[0];

            var browserClasses = GetAllBrowserRegistryClassPaths();
            foreach (var browserClass in browserClasses)
            {
                // Only mess with the local user's browser paths. Don't install onto LocalMachine.
                string cmdlineRegistryPath = $@"{browserClass}\shell\open\command";
                string cmdlineRegistryUserPath = cmdlineRegistryPath.Replace("HKEY_CLASSES_ROOT", @"HKEY_CURRENT_USER\SOFTWARE\Classes");
                string prevCmdline = (string)Registry.GetValue(cmdlineRegistryPath, "", "");
                if (string.IsNullOrWhiteSpace(prevCmdline))
                {
                    continue;
                }
                string[] prevArgs = CliUtils.CommandLineToArgs(prevCmdline);
                if (prevArgs[0].EndsWith("linkWheel.exe", StringComparison.InvariantCultureIgnoreCase))
                {
                    // If a previous version of LinkWheel is already installed, update the string in case the path
                    // has changed.
                    var addedArgs = (string)Registry.CurrentUser.OpenSubKey(LinkWheelConfig.Registry.ClassKey, true).GetValue($"prev{browserClass}", "");
                    Registry.SetValue(cmdlineRegistryUserPath, "", $"\"{executablePath}\" serve --url %1 -- {addedArgs}");
                }
                else
                {
                    // Store the current value. If the browser is installed only for the current user, then we wouldn't
                    // be able to find the original commandline anywhere.
                    Registry.CurrentUser.OpenSubKey(LinkWheelConfig.Registry.ClassKey, true).SetValue($"prev{browserClass}", prevCmdline);
                    Registry.CurrentUser.OpenSubKey(LinkWheelConfig.Registry.ClassKey, true).SetValue($"prev{cmdlineRegistryPath}", prevCmdline);
                    // Set the current user's shell/open/command value. If the browser is installed at the machine level,
                    // it probably won't bother touching this key during updates/re-installs.
                    Registry.SetValue(cmdlineRegistryUserPath, "", $"\"{executablePath}\" %1 {prevCmdline}");
                }
            }
        }

        private static void WriteRegistryClassKey()
        {
            using var classKey = Registry.CurrentUser.OpenSubKey(LinkWheelConfig.Registry.ClassKey, true);
            var nonNullClassKey = classKey;
            if (classKey == null)
            {
                nonNullClassKey = Registry.CurrentUser.CreateSubKey(LinkWheelConfig.Registry.ClassKey, true);
            }
            nonNullClassKey.SetValue(LinkWheelConfig.Registry.EnabledValue, true);
        }

        private static void RemoveRegistryClassKey()
        {
            using var classKey = Registry.CurrentUser.OpenSubKey(LinkWheelConfig.Registry.ClassKey, true);
            var nonNullClassKey = classKey;
            if (classKey == null)
            {
                return;
            }
            nonNullClassKey.DeleteValue(LinkWheelConfig.Registry.EnabledValue);
            nonNullClassKey.DeleteValue("");
        }

        /// <remarks>
        /// Technically not a full revert, since we re-write the CURRENT_USER keys to make HKEY_CLASSES_ROOT return the
        /// same value it had before installation. A better approach would be to delete the CURRENT_USER key if it did
        /// not exist before installation. This is good enough though.
        /// </remarks>
        private static void RevertBrowserShellCommands()
        {
            string[] args = Environment.GetCommandLineArgs();
            // Dumb hack for running while debugging. Use the EXE instead of the DLL so we don't need to kick off with
            // dotnet.exe.
            string executablePath = args[0].EndsWith(".dll") ? args[0][..^4] + ".exe" : args[0];

            var browserClasses = GetAllBrowserRegistryClassPaths();
            foreach (var browserClass in browserClasses)
            {
                string cmdlineRegistryPath = $@"{browserClass}\shell\open\command";
                string prevCmdline = (string)Registry.GetValue(cmdlineRegistryPath, "", "");
                if (string.IsNullOrWhiteSpace(prevCmdline))
                {
                    continue;
                }
                string[] prevArgs = CliUtils.CommandLineToArgs(prevCmdline);
                if (prevArgs[0].EndsWith("linkWheel.exe", StringComparison.InvariantCultureIgnoreCase))
                {
                    var prevCommandline = (string)Registry.CurrentUser.OpenSubKey(LinkWheelConfig.Registry.ClassKey).GetValue($"prev{browserClass}", "");
                    Registry.SetValue(cmdlineRegistryPath, "", prevCommandline);
                }
            }
        }

        private static List<string> GetAllBrowserRegistryClassPaths()
        {
            var machineInternetKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Clients\StartMenuInternet") ??
                              Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet");
            var userInternetKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\WOW6432Node\Clients\StartMenuInternet") ??
                              Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet");

            HashSet<string> browserClasses = new();
            foreach (var internetKey in new[] { machineInternetKey, userInternetKey }.Where(key => key != null))
            {
                foreach (var browserName in internetKey.GetSubKeyNames())
                {
                    // Key containing browser information
                    var browserKey = internetKey.OpenSubKey(browserName);

                    // Key containing executable path
                    using var urlAssociations = browserKey.OpenSubKey(@"Capabilities\URLAssociations");
                    if (urlAssociations == null)
                    {
                        continue;
                    }

                    foreach (var protocol in urlAssociations.GetValueNames())
                    {
                        if (string.IsNullOrWhiteSpace(protocol))
                        {
                            continue;
                        }
                        var className = (string)urlAssociations.GetValue(protocol);
                        if (string.IsNullOrWhiteSpace(className))
                        {
                            continue;
                        }
                        using var classKey = Registry.ClassesRoot.OpenSubKey(className);
                        if (classKey != null)
                        {
                            browserClasses.Add(classKey.ToString());
                        }
                    }
                }
            }

            // Check if edge is a viable option.
            using var edgeClassKey = Registry.ClassesRoot.OpenSubKey("MSEdgeHTM");
            if (edgeClassKey != null)
            {
                browserClasses.Add(edgeClassKey.ToString());
            }

            return browserClasses.ToList();
        }

        /// <remarks>
        /// This is some untested code to theoretically install LinkWheel as a browser.
        /// 
        /// We may need to implement this instead if shell/open/command is being refreshed by
        /// something else (e.g., browser updates).
        /// </remarks>
        private static void WindowsImplementBrowserProtocols()
        {
            string dllLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            // Replaces the ".dll" extension with ".exe".
            string location = dllLocation[..^4] + ".exe";

            if (Registry.LocalMachine.OpenSubKey(@$"SOFTWARE\Classes\{nameof(LinkWheel)}") == null)
            {
                using (var key = Registry.CurrentUser.CreateSubKey(@$"SOFTWARE\Classes\{nameof(LinkWheel)}"))
                {
                    key.SetValue(string.Empty, $"URL:{nameof(LinkWheel)} Protocol");
                    key.SetValue("URL Protocol", string.Empty);

                    using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
                    {
                        defaultIcon.SetValue("", $"{location},1");
                    }

                    using (var commandKey = key.CreateSubKey(@"shell\open\command"))
                    {
                        commandKey.SetValue("", "\"" + location + "\" \"%1\"");
                    }
                }
            }

            using (var key = Registry.LocalMachine.CreateSubKey(@$"SOFTWARE\Classes\LinkWheel"))
            {
                using var webBrowser = key.CreateSubKey("Capabilities");
                webBrowser.SetValue("ApplicationDescription", "More options for the same link.");
                webBrowser.SetValue("ApplicationName", "Link Wheel");
                webBrowser.SetValue("Hidden", "0");

                using var urlAssociations = webBrowser.CreateSubKey("UrlAssociations");
                urlAssociations.SetValue("http", "LinkWheel.Url.Http");
                urlAssociations.SetValue("https", "LinkWheel.Url.Https");
            }

            using (var progId = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Classes\LinkWheel.Url.Http"))
            {
                progId.SetValue("", "Link Wheel");
                progId.SetValue("AllowSilentDefaultTakeOver", "1");
                using (var defaultIcon = progId.CreateSubKey("DefaultIcon"))
                {
                    defaultIcon.SetValue("", dllLocation + ",1");
                }

                using (var commandKey = progId.CreateSubKey(@"shell\open\command"))
                {
                    commandKey.SetValue("", $"\"{location}\" \"%1\"");
                }
            }

            using (var progId = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Classes\LinkWheel.Url.Https"))
            {
                progId.SetValue("", "Link Wheel");
                progId.SetValue("AllowSilentDefaultTakeOver", "1");
                using (var defaultIcon = progId.CreateSubKey("DefaultIcon"))
                {
                    defaultIcon.SetValue("", dllLocation + ",1");
                }

                using (var commandKey = progId.CreateSubKey(@"shell\open\command"))
                {
                    commandKey.SetValue("", $"\"{location}\" \"%1\"");
                }
            }
            SHChangeNotifyWrapper.SHChangeNotify(HChangeNotifyEventID.SHCNE_ASSOCCHANGED, HChangeNotifyFlags.SHCNF_DWORD | HChangeNotifyFlags.SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
