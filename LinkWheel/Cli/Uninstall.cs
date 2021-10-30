using CommandLine;
using System;
using System.Threading.Tasks;

namespace LinkWheel.Cli
{
    [Verb("uninstall")]
    class Uninstall
    {
        public Task<int> ExecuteAsync()
        {
            if (OperatingSystem.IsWindows())
            {
                new WindowsInstaller().Uninstall();
                return Task.FromResult(0);
            }
            return Task.FromResult(1);
        }
    }
}
