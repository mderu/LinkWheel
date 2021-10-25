using CommandLine;
using System;

namespace LinkWheel.Cli
{
    [Verb("uninstall")]
    class Uninstall
    {
        public int Execute()
        {
            if (OperatingSystem.IsWindows())
            {
                new WindowsInstaller().Uninstall();
                return 0;
            }
            return 1;
        }
    }
}
