using CommandLine;
using System;
using System.Threading.Tasks;

namespace LinkWheel.Cli
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
