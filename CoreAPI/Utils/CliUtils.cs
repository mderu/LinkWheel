using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace CoreAPI.Utils
{
    public static class CliUtils
    {
        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        public static string[] CommandLineToArgs(string commandLine)
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new NotImplementedException("Only implemented for Windows.");
            }

            var argv = CommandLineToArgvW(commandLine, out int argc);
            if (argv == IntPtr.Zero)
            {
                throw new System.ComponentModel.Win32Exception();
            }

            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }
                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }

        public static void SimpleInvoke(IEnumerable<string> args)
        {
            ProcessStartInfo startInfo = new(args.First(), string.Join(" ", args.Skip(1)))
            {
                UseShellExecute = true,
            };
            Process.Start(startInfo);
        }
    }
}
