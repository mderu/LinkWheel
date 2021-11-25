using CommandLine;
using CoreAPI.Config;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("disable")]
    public class Disable
    {
        public Task<int> ExecuteAsync()
        {
            if (OperatingSystem.IsWindows())
            {
                RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(LinkWheelConfig.Registry.ClassKey, true);
                if (registryKey is not null)
                {
                    registryKey.SetValue(LinkWheelConfig.Registry.EnabledValue, "false");
                    return Task.FromResult(0);
                }
                return Task.FromResult(1);
            }
            throw new NotImplementedException($"{nameof(Disable)} has only been implemented for Windows.");
        }
    }
}
