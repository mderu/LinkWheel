﻿using System;
using System.IO;
using System.Threading;

namespace CoreAPI.Utils
{
    public static class FileUtils
    {
        /// <summary>
        /// Waits on the givne file path to open for writing. Then, locks the file and calls the given action.
        /// The file is unlocked after the action completes.
        /// </summary>
        public static void Lock(string path, Action<FileStream> action)
        {
            var autoResetEvent = new AutoResetEvent(false);

            while (true)
            {
                try
                {
                    using var file = File.Open(
                        path, 
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.Write);
                    action(file);
                    return;
                }
                catch (IOException)
                {
                    var fileSystemWatcher =
                        new FileSystemWatcher(Path.GetDirectoryName(path))
                        {
                            // May have been fixed at some point, but we don't set a file path filter
                            // here because it may not work on Linux:
                            // https://github.com/dotnet/runtime/issues/22654
                            // Eventually we should test this on Linux. If it doesn't work, we can
                            // inherit/wrap FileSystemWatcher to have the events filter before calling
                            // the passed-in actions.
                            EnableRaisingEvents = true
                        };

                    fileSystemWatcher.Changed +=
                        (o, e) =>
                        {
                            if (ArePathsEqual(e.FullPath, path))
                            {
                                autoResetEvent.Set();
                            }
                        };

                    autoResetEvent.WaitOne();
                }
            }
        }

        /// <summary>
        /// File.ReadAllText, but waits for the file to be unlocked.
        /// </summary>
        public static string ReadAllTextWait(string path)
        {
            var autoResetEvent = new AutoResetEvent(false);

            while (true)
            {
                try
                {
                    using var file = File.Open(
                        path,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.None);
                    return new StreamReader(file).ReadToEnd();
                }
                catch (IOException)
                {
                    var fileSystemWatcher =
                        new FileSystemWatcher(Path.GetDirectoryName(path))
                        {
                            // See note in Lock() above.
                            EnableRaisingEvents = true
                        };

                    fileSystemWatcher.Changed +=
                        (o, e) =>
                        {
                            if (ArePathsEqual(e.FullPath, path))
                            {
                                autoResetEvent.Set();
                            }
                        };

                    autoResetEvent.WaitOne();
                }
            }
        }

        public static bool ArePathsEqual(string pathA, string pathB)
        {
            // TODO: Handle case sensitivity better than assuming it based on OS.
            if (OperatingSystem.IsWindows())
            {
                return string.Equals(Path.GetFullPath(pathA), Path.GetFullPath(pathB), StringComparison.InvariantCultureIgnoreCase);
            }
            else
            {
                return string.Equals(Path.GetFullPath(pathA), Path.GetFullPath(pathB), StringComparison.Ordinal);
            }
        }

        public static bool IsWithinPath(string parentPath, string potentialChildPath)
        {
            Uri parentUri = new(Path.GetFullPath(parentPath), UriKind.Absolute);
            Uri childUri = new(Path.GetFullPath(potentialChildPath), UriKind.Absolute);

            Uri relUri = parentUri.MakeRelativeUri(childUri);
            return !(relUri.IsAbsoluteUri || relUri.ToString().StartsWith(".."));
        }
    }
}
