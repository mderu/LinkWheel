using CommandLine;
using CoreAPI.Installers;
using CoreAPI.OutputFormat;
using System;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("install", HelpText = HelpText)]
    public class Install
    {
        public const string HelpText = "Installs LinkWheel and adds it to your system path.";
        public Task<OutputData> ExecuteAsync()
        {
            if (OperatingSystem.IsWindows())
            {
                new WindowsInstaller().ForceInstall();
                return Task.FromResult(new OutputData(0, new(), ""));
            }
            return Task.FromResult(new OutputData(1, new(), "Only possible to install on Windows for now."));
        }
    }
}
