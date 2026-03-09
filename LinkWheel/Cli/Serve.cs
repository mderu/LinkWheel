using CommandLine;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreAPI.Cli;
using CoreAPI.Models;

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
        public IEnumerable<string> BrowserArgs { get; set; } = [];

        public async Task<List<IdelAction>> ExecuteAsync()
        {
            var browserArgs = BrowserArgs;
            return await new GetActions()
            {
                Url = Url,
                BrowserArgs = browserArgs
            }.Get();
        }
    }
}
