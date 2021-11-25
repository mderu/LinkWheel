using CommandLine;
using CoreAPI.Config;
using CoreAPI.Utils;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("get-registration", HelpText = HelpText)]
    public class GetRegistration
    {
        [Option("path", Required = true)]
        public string Path { get; set; }

        public const string HelpText = 
            "If the given path is registered, writes the registered RepoConfig and return code 0. " +
            "Otherwise, returns 1.";

        public async Task<int> ExecuteAsync()
        {
            if (TaskUtils.Try(await new GetRoot() { Path = Path }.TryGet(), out string root))
            {
                var results = RepoConfigFile.Read().Where(x => FileUtils.ArePathsEqual(x.Root, root));
                if (results.Any())
                {
                    Console.WriteLine(JsonConvert.SerializeObject(results.First()));
                    return 0;
                }
            }
            return 1;
        }
    }
}
