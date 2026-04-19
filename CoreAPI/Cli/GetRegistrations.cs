using CommandLine;
using CoreAPI.Config;
using CoreAPI.OutputFormat;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("get-registrations", HelpText = HelpText)]
    public class GetRegistrations
    {
        public const string HelpText = "Returns all registered repo configurations.";

        public Task<OutputData> ExecuteAsync()
        {
            var configs = RepoConfigs.All();
            return Task.FromResult(new OutputData(0, new Dictionary<string, object>
            {
                { "registrations", configs }
            }));
        }
    }
}
