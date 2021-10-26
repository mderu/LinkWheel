using CommandLine;
using Microsoft.Win32;
using System;

namespace LinkWheel.Cli
{
    [Verb("disable")]
    class Disable
    {
        public int Execute()
        {
            if (OperatingSystem.IsWindows())
            {
                Registry.CurrentUser.OpenSubKey(LinkWheelConfig.Registry.ClassKey, true)
                    .SetValue(LinkWheelConfig.Registry.EnabledValue, "false");
                return 0;
            }
            throw new NotImplementedException($"{nameof(Disable)} has only been implemented for Windows.");
        }
    }
}
