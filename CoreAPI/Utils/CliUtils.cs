using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

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
                    // Forgiveness: `p` is not null.
                    args[i] = Marshal.PtrToStringUni(p)!;
                }
                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }

        /// <summary>
        /// Joins arguments to a batch command line string. Note that special characters may create issues.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string JoinToCommandLine(IEnumerable<string> args)
        {
            // Many special characters will not get caught.
            // Maybe try https://stackoverflow.com/a/10489920/6876989?
            StringBuilder result = new();
            foreach (string arg in args)
            {
                if (arg.Contains(" "))
                {
                    result.Append('\"');
                    result.Append(arg
                        .Replace("\"", "\\\"")
                        .Replace("&", "^&"));
                    result.Append('\"');
                }
                else
                {
                    result.Append(arg
                        .Replace("&", "^&"));
                }
                result.Append(' ');
            }
            if (args.Any())
            {
                result.Remove(result.Length - 1, 1);
            }
            return result.ToString();
        }

        /// <summary>
        /// Writes the script to a temp file and executes it.
        /// Hopefully cleans it up afterwards.
        /// </summary>
        /// <param name="batchScriptContents"></param>
        public static void SimpleInvoke(string batchScriptContents, string workingDirectory)
        {
            if (OperatingSystem.IsWindows())
            {
                string tempFileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                tempFileName += ".cmd";
                File.WriteAllText(tempFileName, batchScriptContents);
                Process process = new()
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        FileName = tempFileName,
                        WorkingDirectory = workingDirectory,
                    },
                };
                // Delete the temp file when done.
                process.Exited += (object? _, EventArgs _) =>
                {
                    File.Delete(tempFileName);
                };
                process.Start();
                // TODO: Don't block. This is here because the process won't run if we return immediately,
                // because it will go out of scope.
                process.WaitForExit();
            }
            else
            {
                // TODO: Can probably redirect stdin to bash instead of making a temp file.
                // Can probably do this with Windows too come to think of it...
                throw new NotImplementedException("Not implemented for Non-Windows yet.");
            }
        }
    }
}
