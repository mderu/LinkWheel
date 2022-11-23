using CommandLine;
using CoreAPI.OutputFormat;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("open-in-default-browser", HelpText = HelpText)]
    public class OpenInDefaultBrowser
    {
        const string HelpText = "Opens the given --url in your default browser. Useful if you want" +
            " to open a link without recursing and opening LinkWheel again.";

        [Option("url", Required = true, HelpText = "The url to open.")]
        public string Url { get; set; } = "";

        public async Task<OutputData> ExecuteAsync()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var browserArgsOutput = await new GetBrowserArgs() { Url = Url }.ExecuteAsync();
                string[] arguments = (string[])browserArgsOutput.Objects["array"];

                ProcessStartInfo startInfo = new(arguments[0], string.Join(" ", arguments[1..]))
                {
                    UseShellExecute = true,
                };
                Process.Start(startInfo);
                return new(0, new(), "");
            }
            else
            {
                return new(1, new(), "Have not figured out the best way to do this is Linux/OSX yet.");
            }
        }
    }
}
