using CommandLine;
using CoreAPI.Installers;
using CoreAPI.OutputFormat;
using System;
using System.Threading.Tasks;

namespace LinkWheel.Cli
{
    [Verb("uninstall")]
    public class Uninstall
    {
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
