using CommandLine;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreAPI.Config;
using System.Linq;
using CoreAPI.Cli;

namespace LinkWheel.Cli
{
    /// <summary>
    /// A wrapper that allows LinkWheel's GUI to call this function and get a List<WheelElement>
    /// instead of outputting JSON.
    /// </summary>
    [Verb("serve")]
    public class Serve
    {
        [Option("url", Required = true)]
        public string Url { get; set; } = "";

        [Value(0)]
        public IEnumerable<string> BrowserArgs { get; set; } = new List<string>();

        public async Task<List<WheelElement>> ExecuteAsync()
        {
            // TODO: I have no idea why "serve" gets picked up here, but in get-actions the verb doesn't,
            // but I really don't have the time to deal with this right now. This is a hack to remove the
            // verb from the list of arguments.
            var browserArgs = BrowserArgs;
            if (browserArgs.First() == "serve")
            {
                browserArgs = browserArgs.Skip(1);
            }
            return await new GetActions()
            {
                Url = Url, 
                BrowserArgs = browserArgs
            }.Get();
        }
    }
}
