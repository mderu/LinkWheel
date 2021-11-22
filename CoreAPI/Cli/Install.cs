using CommandLine;
using CoreAPI.Installers;
using System;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("install")]
    public class Install
    {
        public Task<int> ExecuteAsync()
        {
            if (OperatingSystem.IsWindows())
            {
                new WindowsInstaller().ForceInstall();
                return Task.FromResult(0);
            }
            return Task.FromResult(1);
        }
    }
}
