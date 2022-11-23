using CommandLine;
using CoreAPI.OutputFormat;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("get-registration", HelpText = HelpText)]
    public class GetRegistration
    {
        public const string HelpText =
            "If the given path is registered, writes the registered RepoConfig and return code 0. " +
            "Otherwise, returns 1.";

        [Option("path", Required = true, HelpText = "The path (or file) to check whether or not it is registered.")]
        public string Path { get; set; } = "";

        public async Task<OutputData> ExecuteAsync()
        {
            // TODO: Can probably delete either this or `get-root`.
            var result = await new GetRoot() { Path = Path }.ExecuteAsync();
            result.Format = "(=results[0]=)";
            return result;
        }
    }
}
