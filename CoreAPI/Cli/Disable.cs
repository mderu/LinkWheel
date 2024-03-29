﻿using CommandLine;
using CoreAPI.Config;
using CoreAPI.OutputFormat;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace CoreAPI.Cli
{
    [Verb("disable", HelpText = HelpText)]
    public class Disable
    {
        public const string HelpText = "Disables LinkWheel from intercepting links, but otherwise stays installed."
            + " Useful for checking whether or not a program is trying to open a link, but LinkWheel is crashing.";

        public Task<OutputData> ExecuteAsync()
        {
            if (OperatingSystem.IsWindows())
            {
                RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(LinkWheelConfig.Registry.ClassKey, true);
                if (registryKey is not null)
                {
                    registryKey.SetValue(LinkWheelConfig.Registry.EnabledValue, "false");
                    return Task.FromResult(new OutputData(0, new() { ["installed"] = true, ["enabled"] = false }, ""));
                }
                return Task.FromResult(new OutputData(0, new() { ["installed"] = false, ["enabled"] = false }, ""));
            }
            throw new NotImplementedException($"{nameof(Disable)} has only been implemented for Windows.");
        }
    }
}
