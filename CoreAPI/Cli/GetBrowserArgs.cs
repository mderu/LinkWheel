using CommandLine;
using CoreAPI.Config;
using CoreAPI.OutputFormat;
using CoreAPI.Utils;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("get-browser-args", HelpText = HelpText)]
    public class GetBrowserArgs
    {
        public const string HelpText = "Returns the browsers arguments, where %1 replaces the URL if no URL is provided.";

        [Option("url", HelpText = "If provided the URL to insert into the commandline, replacing any %1 within the commandline.")]
        public string? Url { get; set; }

        public async Task<OutputData> ExecuteAsync()
        {
            if (OperatingSystem.IsWindows())
            {
                string registryKey;
                if (Url is not null && Url.StartsWith("http"))
                {
                    registryKey = LinkWheelConfig.Registry.DefaultBrowserHttpKey;
                }
                else
                {
                    registryKey = LinkWheelConfig.Registry.DefaultBrowserHttpsKey;
                }
                string? classKey = (string?)Registry.GetValue(registryKey,
                    LinkWheelConfig.Registry.DefaultBrowserValue,
                    LinkWheelConfig.Registry.DefaultBrowserProgId);
                if (classKey is null)
                {
                    return new(1, new(), $"Unable to open url {Url ?? ""}: the registry key {registryKey} does not exist.");
                }

                //TO(MAYBE)DO: Could read from HKEY_CLASSES_ROOT\LinkWheel\prev{key}, but that key includes the full path
                // instead of the CLASSES_ROOT path that we use here for simplification.
                // Optionally, when we install we can also write this key with the HKEY_CLASSES_ROOT form
                string commandline = (string)(Registry.GetValue($@"HKEY_CLASSES_ROOT\{classKey}\shell\open\command", "", "") ?? "");

                if (Url is not null)
                {
                    commandline = commandline.Replace("%1", Url);
                }

                string[] arguments = CliUtils.CommandLineToArgs(commandline);
                if (arguments[0].EndsWith("linkWheel.exe", StringComparison.InvariantCultureIgnoreCase))
                {
                    int i;
                    for (i = 0; i < arguments.Length; i++)
                    {
                        if (arguments[i] == "--")
                        {
                            arguments = arguments[(i + 1)..];
                            commandline = commandline[(commandline.IndexOf(" -- ") + 4)..];
                            break;
                        }
                    }
                }
                return new(0, new() { ["array"] = arguments, ["commandline"] = commandline }, "(=commandline=)");
            }
            else
            {
                return new(1, new(), "Have not figured out the best way to do this is Linux/OSX yet.");
            }
        }
    }
}