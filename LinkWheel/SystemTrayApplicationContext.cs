using LinkWheel.Cli;
using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using LinkWheel.InternalConfig;
using System.Linq;

namespace LinkWheel
{
    public class SystemTrayApplicationContext : ApplicationContext
    {
        private NotifyIcon TrayIcon { get; set; }
        private HashSet<string> TrackedPaths { get; set; }
        private static string TrackedPathsFile => Path.Combine(LinkWheelConfig.CacheDirectory, "paths.txt");

        private static FileSystemWatcher ConfigDirectoryWatcher { get; set; }
        private static List<FileSystemWatcher> RepoWatchers { get; set; } = new();

        private const NotifyFilters DefaultFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

        public SystemTrayApplicationContext()
        {
            // Initialize Tray Icon
            TrayIcon = new NotifyIcon
            {
                Icon = Properties.Resources.linkWheelIcon,
                Visible = true
            };
            SetMenuStrip();
            TrackedPaths = new HashSet<string>(File.ReadAllLines(TrackedPathsFile));
            RepoWatchers = new List<FileSystemWatcher>();

            // Add the watcher for the configuration files.
            ConfigDirectoryWatcher = new(LinkWheelConfig.CacheDirectory)
            {
                NotifyFilter = DefaultFilter
            };
            ConfigDirectoryWatcher.Changed += OnRepoConfigChanged;
            ConfigDirectoryWatcher.Error += OnError;
            ConfigDirectoryWatcher.EnableRaisingEvents = true;

            RefreshRepoWatchers();
        }

        private static void RefreshRepoWatchers()
        {
            List<FileSystemWatcher> watchers = new();
            if (File.Exists(LinkWheelConfig.TrackedReposFile))
            {
                // System.Threading.Thread.Sleep(500);
                List<RepoConfig> repoConfigs = RepoConfigFile.Read();

                var repoRoots = repoConfigs
                    .Select(repoConfig => repoConfig.Root);

                foreach (string repoRoot in repoRoots)
                {
                    FileSystemWatcher watcher = new(repoRoot)
                    {
                        Filter = "*.idelconfig",
                        NotifyFilter = DefaultFilter
                    };
                    watcher.Changed += OnIdelConfigChanged;
                    watcher.Error += OnError;
                    watcher.EnableRaisingEvents = true;
                    watchers.Add(watcher);
                }
            }
            else
            {
                // If the file does not exist because the user has not registered a repo,
                // nothing will happen.
                //
                // If the file does not exist because the user deleted the config file,
                // the watchers will be disabled/removed.
            }

            var oldRepoWatchers = RepoWatchers;
            RepoWatchers = watchers;

            // Technically we can get double hits on these watchers until we dispose them,
            // but it's probably fine.
            foreach (var watcher in oldRepoWatchers)
            {
                watcher.Dispose();
            }
        }

        private static void OnIdelConfigChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed
                // Might want to end the file .idelconfig file name with .json so it can be defaulted to a JSON editor.
                // Alternatively, we can install LinkWheel as the default editor for .idelconfig files, and forward
                // the opening of these files to the default JSON editor.
                // 
                // As a bonus, the alternative above will allow us to call the RegisterRepo on the file's parent when
                // it is opened for the first time, which can potentially reduce the friction of initial setup. This
                // may confuse users though when they create the file directly in their editor.
                && Path.GetFileName(e.FullPath).Contains(".idelconfig"))
            {
                return;
            }
            // TODO: Business logic of updating triggers.
            Console.WriteLine($"Changed: {e.FullPath}");
        }

        private static void OnRepoConfigChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed
                && e.FullPath != LinkWheelConfig.TrackedReposFile)
            {
                return;
            }
            RefreshRepoWatchers();
        }

        private static void OnError(object sender, ErrorEventArgs e)
        {
            PrintException(e.GetException());
        }

        private static void PrintException(Exception ex)
        {
            if (ex != null)
            {
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine("Stacktrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                PrintException(ex.InnerException);
            }
        }

        private void SetMenuStrip()
        {
            ContextMenuStrip menuStrip = new();
            if (new WindowsInstaller().IsEnabled())
            {
                menuStrip.Items.Add("Disable", null, DisableLinkWheel);
            }
            else
            {
                menuStrip.Items.Add("Enable", null, EnableLinkWheel);
            }
            menuStrip.Items.Add("Register Repo", null, RegisterNewConfig);
            menuStrip.Items.Add("Exit", null, Exit);
            TrayIcon.ContextMenuStrip = menuStrip;
        }

        public void RegisterNewConfig(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new();
            folderDialog.ShowDialog();
            string path = folderDialog.SelectedPath;

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            new RegisterRepo(){ Directory = path }.Execute();

            if (!File.Exists(Path.Combine(path, ".idelconfig")) && !File.Exists(Path.Combine(path, ".user.idelconfig")))
            {
                return;
            }

            if (TrackedPaths.Contains(path))
            {
                return;
            }

            File.AppendAllText(TrackedPathsFile, path + Environment.NewLine);
        }

        public void DisableLinkWheel(object sender, EventArgs e)
        {
            new WindowsInstaller().Disable();
            SetMenuStrip();
        }

        public void EnableLinkWheel(object sender, EventArgs e)
        {
            new WindowsInstaller().Enable();
            SetMenuStrip();
        }

        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            TrayIcon.Visible = false;

            Application.Exit();
        }
    }
}
