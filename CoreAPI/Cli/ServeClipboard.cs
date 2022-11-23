using CommandLine;
using CoreAPI.Config;
using CoreAPI.OutputFormat;
using CoreAPI.Utils;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TextCopy;

namespace CoreAPI.Cli
{
    [Verb("serve-clipboard", HelpText = HelpText)]
    public class ServeClipboard
    {
        public const string HelpText = "Opens up LinkWheel using the URL stored in your clipboard (if applicable).";

        public async Task<OutputData> ExecuteAsync()
        {
            string? clipboardContents = await ClipboardService.GetTextAsync();
            if (string.IsNullOrEmpty(clipboardContents))
            {
                return new(1, new() { ["clipboard"] = clipboardContents ?? "" }, "Clipboard is empty");
            }
            Uri url;
            try
            {
                url = new Uri(clipboardContents);
            }
            catch (Exception)
            {
                return new(1, new() { ["clipboard"] = clipboardContents }, "The clipboard does not contain a Url.");
            }

            if (OperatingSystem.IsWindows())
            {
                string registryKey;
                if (clipboardContents.StartsWith("http"))
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
                    return new(1,
                        new() { ["clipboard"] = clipboardContents },
                        $"Unable to open url (=clipboard=): the registry key {registryKey} does not exist.");
                }

                string commandline = (string)(Registry.GetValue($@"HKEY_CLASSES_ROOT\{classKey}\shell\open\command", "", "") ?? "");
                commandline = commandline.Replace("%1", clipboardContents);
                string[] arguments = CliUtils.CommandLineToArgs(commandline);
                ProcessStartInfo startInfo = new(arguments[0], string.Join(" ", arguments[1..].Select(arg => arg.Contains(" ") ? $"\"{arg}\"" : arg)))
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                Process.Start(startInfo);
                return new(0, new(), "");
            }
            else
            {
                // See OpenInDefaultBrowser.
                return new(1, new(), "Have not figured out the best way to do this is Linux/OSX yet.");
            }
        }
    }
}
