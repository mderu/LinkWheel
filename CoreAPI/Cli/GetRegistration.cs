using CommandLine;
using CoreAPI.Config;
using CoreAPI.OutputFormat;
using CoreAPI.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("get-registration", HelpText = HelpText)]
    public class GetRegistration
    {
        [Option("path", Required = true)]
        public string Path { get; set; } = "";

        public const string HelpText = 
            "If the given path is registered, writes the registered RepoConfig and return code 0. " +
            "Otherwise, returns 1.";

        public async Task<OutputData> ExecuteAsync()
        {
            // TODO: Can probably delete either this or `get-root`.
            var result = await new GetRoot() { Path = Path }.ExecuteAsync();
            result.Format = "(=results[0]=)";
            return result;
        }
    }
}
