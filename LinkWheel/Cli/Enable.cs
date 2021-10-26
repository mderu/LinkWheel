using CommandLine;
using Microsoft.Win32;
using System;

namespace LinkWheel.Cli
{
    [Verb("enable")]
    public class Enable
    {
        public int Execute()
        {
            if (OperatingSystem.IsWindows())
            {
                Registry.CurrentUser.OpenSubKey(LinkWheelConfig.Registry.ClassKey, true)
                    .SetValue(LinkWheelConfig.Registry.EnabledValue, "true");
                return 0;
            }
            throw new NotImplementedException($"{nameof(Enable)} has only been implemented for Windows.");
        }

        public static bool IsEnabled()
        {
            if (OperatingSystem.IsWindows())
            {
                using RegistryKey classesRoot = Registry.ClassesRoot.OpenSubKey(nameof(LinkWheel));
                string rawValue = (string)classesRoot.GetValue(LinkWheelConfig.Registry.EnabledValue, "false");
                return bool.Parse(rawValue);
            }
            throw new NotImplementedException($"{nameof(Enable)} has only been implemented for Windows.");
        }
    }
}
