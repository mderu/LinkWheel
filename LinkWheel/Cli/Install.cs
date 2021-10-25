using CommandLine;
using System;

namespace LinkWheel.Cli
{
    [Verb("install")]
    class Install
    {
        public int Execute()
        {
            if (OperatingSystem.IsWindows())
            {
                new WindowsInstaller().ForceInstall();
                return 0;
            }
            return 1;
        }
    }
}
