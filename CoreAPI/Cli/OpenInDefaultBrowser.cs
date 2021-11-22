using CommandLine;
using CoreAPI.Config;
using CoreAPI.Installers;
using CoreAPI.Utils;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

// https://github.com/mderu/LinkWheel/blob/master/LinkWheel/Program.cs

namespace CoreAPI.Cli
{
    [Verb("open-in-default-browser")]
    public class OpenInDefaultBrowser
    {
        [Option("url", Required = true)]
        public string Url { get; set; }

        public Task<int> ExecuteAsync()
        {
            // Always ensure LinkWheel is installed before running a command. No point in making the user
            // run an install command before being able to use the executable.
            Installer.EnsureInstalled();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string registryKey;
                if (Url.StartsWith("http"))
                {
                    registryKey = LinkWheelConfig.Registry.DefaultBrowserHttpKey;
                }
                else
                {
                    registryKey = LinkWheelConfig.Registry.DefaultBrowserHttpsKey;
                }
                string classKey = (string)Registry.GetValue(registryKey, LinkWheelConfig.Registry.DefaultBrowserValue, "MSEdgeHTM");
                //TO(MAYBE)DO: Could read from HKEY_CLASSES_ROOT\LinkWheel\prev{key}, but that key includes the full path
                // instead of the CLASSES_ROOT path that we use here for simplification.
                // Optionally, when we install we can also write this key with the HKEY_CLASSES_ROOT form
                string commandline = (string)Registry.GetValue($@"HKEY_CLASSES_ROOT\{classKey}\shell\open\command", "", "");

                string[] arguments = CliUtils.CommandLineToArgs(commandline);
                if (arguments[0].EndsWith("linkWheel.exe", StringComparison.InvariantCultureIgnoreCase))
                {
                    int i;
                    for (i = 0; i < arguments.Length; i++)
                    {
                        if (arguments[i] == "%1")
                        {
                            arguments = arguments[(i + 1)..];
                            break;
                        }
                    }
                }
                string executable = arguments[0];
                ProcessStartInfo startInfo = new(executable, string.Join(" ", arguments[1..]))
                {
                    UseShellExecute = true,
                };
                Process.Start(startInfo);
                return Task.FromResult(0);
            }
            else
            {
                throw new NotImplementedException("Have not figured out the best way to do this is Linux/OSX yet.");
            }
        }
    }
}
