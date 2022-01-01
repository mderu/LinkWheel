using CommandLine;
using CoreAPI.Config;
using CoreAPI.OutputFormat;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("enable", HelpText = HelpText)]
    public class Enable
    {
        public const string HelpText = "Enables LinkWheel after it has been disabled. See `disable`.";
        public Task<OutputData> ExecuteAsync()
        {
            if (OperatingSystem.IsWindows())
            {
                RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(LinkWheelConfig.Registry.ClassKey, true);
                if (registryKey is not null)
                {
                    registryKey.SetValue(LinkWheelConfig.Registry.EnabledValue, "true");
                    return Task.FromResult(new OutputData(0, new() { ["installed"] = true, ["enabled"] = true }, ""));
                }
                return Task.FromResult(new OutputData(1, new() { ["installed"] = false, ["enabled"] = false }, ""));
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
