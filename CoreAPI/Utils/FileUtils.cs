using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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
            if (Directory.Exists(path))
            {
                throw new InvalidOperationException("Cannot write to a directory.");
            }

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
                        // Forgiveness: Assume path is a file, or would have created a file.
                        new FileSystemWatcher(Path.GetDirectoryName(path)!)
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
            if (Directory.Exists(path))
            {
                throw new InvalidOperationException("Cannot write to a directory.");
            }

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
                        // Forgiveness: Assume path is a file, or would have created a file.
                        new FileSystemWatcher(Path.GetDirectoryName(path)!)
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
            StringComparison comparison;
            if (OperatingSystem.IsWindows())
            {
                comparison = StringComparison.InvariantCultureIgnoreCase;
            }
            else
            {
                comparison = StringComparison.Ordinal;
            }
            return string.Equals(Path.GetFullPath(pathA), Path.GetFullPath(pathB), comparison);
        }

        public static bool IsWithinPath(string parentPath, string potentialChildPath)
        {
            Uri parentUri = new(Path.GetFullPath(parentPath), UriKind.Absolute);
            Uri childUri = new(Path.GetFullPath(potentialChildPath), UriKind.Absolute);

            Uri relUri = parentUri.MakeRelativeUri(childUri);
            return !(relUri.IsAbsoluteUri || relUri.ToString().StartsWith(".."));
        }

        public static string GetFullNormalizedPath(string inPath)
        {
            if (OperatingSystem.IsWindows())
            {
                // Support special directories
                var regex = new Regex("([%][^%]+[%])");
                string newPath = regex.Replace(inPath, (match) => {
                    // get rid of %%
                    string value = match.Value[1..^1];
                    if (Enum.TryParse(value, out Environment.SpecialFolder result))
                    {
                        return Environment.GetFolderPath(result);
                    }
                    else
                    {
                        // Check if it's an env variable (e.g., LocalAppData)
                        return Environment.GetEnvironmentVariable(value) ?? "";
                    }
                });
                return Path.GetFullPath(newPath);
            }
            
            if (inPath.StartsWith("~/"))
            {
                // Not sure if this resolves inner ../'s.
                return Path.GetFullPath(Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "/", inPath[1..]));
            }
            return Path.GetFullPath(inPath);
        }

        /// <summary>
        /// Gets the full path of the given executable filename as if the user had entered this
        /// executable in a shell. So, for example, the Windows PATH environment variable will
        /// be examined. If the filename can't be found by Windows, null is returned.</summary>
        /// <param name="exeName"></param>
        /// <returns>The full path if successful, or null otherwise.</returns>
        public static bool TryGetExeOnPath(string exeName, out string? fullPath)
        {
            if (OperatingSystem.IsWindows())
            {
                if (exeName.Length >= MAX_PATH)
                {
                    throw new ArgumentException($"The executable name '{exeName}' must have less than {MAX_PATH} characters.",
                        nameof(exeName));
                }

                StringBuilder sb = new(exeName, MAX_PATH);
                fullPath = PathFindOnPath(sb, null) ? sb.ToString() : null;

                // Try to do what Win + R does:
                // https://superuser.com/questions/87372/how-does-the-windows-run-dialog-locate-executables
                //
                // Forgiveness: if this doesn't exist the user probably has bigger problems.
                RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths")!;
                string[] keyNames = regKey.GetSubKeyNames();
                string exeNameWithoutExtension = Path.GetFileNameWithoutExtension(exeName);
                string? keyName = keyNames.Where(key => Path.GetFileNameWithoutExtension(key) == exeNameWithoutExtension).FirstOrDefault();
                if (keyName is not null)
                {
                    // Forgiveness: We know the subkey exists, and we gave it a default, so it must return something.
                    fullPath = (string)regKey.OpenSubKey(keyName)!.GetValue("", "")!;
                    fullPath = fullPath.Trim('\"');
                    return true;
                }                
                return fullPath != null;
            }
            throw new NotImplementedException("TODO: Implement this with where/which in Bash");
        }

        // https://docs.microsoft.com/en-us/windows/desktop/api/shlwapi/nf-shlwapi-pathfindonpathw
        // https://www.pinvoke.net/default.aspx/shlwapi.PathFindOnPath
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        static extern bool PathFindOnPath([In, Out] StringBuilder pszFile, [In] string[]? ppszOtherDirs);

        // from MAPIWIN.h :
        private const int MAX_PATH = 260;
    }
}
