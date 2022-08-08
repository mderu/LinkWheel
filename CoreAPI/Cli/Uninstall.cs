using CommandLine;
using CoreAPI.Installers;
using CoreAPI.OutputFormat;
using System;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("uninstall", HelpText = HelpText)]
    public class Uninstall
    {
        public const string HelpText = "Uninstalls LinkWheel, and removes it from your system path.";
        public Task<OutputData> ExecuteAsync()
        {
            if (OperatingSystem.IsWindows())
            {
                new WindowsInstaller().Uninstall();
                return Task.FromResult(new OutputData(0, new(), ""));
            }
            return Task.FromResult(new OutputData(1, new(), "Only possible to install on Windows for now."));
        }
    }
}
