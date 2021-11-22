using CommandLine;
using CoreAPI.Config;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("enable")]
    public class Enable
    {
        public Task<int> ExecuteAsync()
        {
            if (OperatingSystem.IsWindows())
            {
                Registry.CurrentUser.OpenSubKey(LinkWheelConfig.Registry.ClassKey, true)
                    .SetValue(LinkWheelConfig.Registry.EnabledValue, "true");
                return Task.FromResult(0);
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
