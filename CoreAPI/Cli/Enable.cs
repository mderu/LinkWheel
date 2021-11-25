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
                RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(LinkWheelConfig.Registry.ClassKey, true);
                if (registryKey is not null)
                {
                    registryKey.SetValue(LinkWheelConfig.Registry.EnabledValue, "true");
                    return Task.FromResult(0);
                }
                return Task.FromResult(1);
            }
            throw new NotImplementedException($"{nameof(Enable)} has only been implemented for Windows.");
        }

        public static bool IsEnabled()
        {
            if (OperatingSystem.IsWindows())
            {
                using RegistryKey? classesRoot = Registry.ClassesRoot.OpenSubKey(nameof(LinkWheel));
                if (classesRoot is null)
                {
                    return false;
                }
                string? rawValue = (string?)classesRoot.GetValue(LinkWheelConfig.Registry.EnabledValue, "false");
                return bool.Parse(rawValue ?? "false");
            }
            throw new NotImplementedException($"{nameof(Enable)} has only been implemented for Windows.");
        }
    }
}
