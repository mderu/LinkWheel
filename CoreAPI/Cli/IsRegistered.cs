using CommandLine;
using CoreAPI.Config;
using CoreAPI.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("is-registered")]
    public class IsRegistered
    {
        [Option("path", Required = true)]
        public string Path { get; set; }

        public async Task<int> ExecuteAsync()
        {
            if (TaskUtils.Try(await new GetRoot() { Path = Path }.TryGet(), out string root))
            {
                if (RepoConfigFile.Read().Where(x => FileUtils.ArePathsEqual(x.Root, root)).Any())
                {
                    return 0;
                }
            }
            return 1;
        }
    }
}
