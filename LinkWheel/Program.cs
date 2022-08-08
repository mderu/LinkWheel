using System;
using System.Windows.Forms;
using CommandLine;
using System.Threading.Tasks;
using System.Collections.Generic;
using CoreAPI.Installers;
using CoreAPI.Utils;
using LinkWheel.Cli;
using CoreAPI.Models;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using CoreAPI.Config;

// Links provided to make testing easier:
// http://www.google.com (for the case where we don't want to intercept).
// https://github.com/mderu/LinkWheel/blob/master/LinkWheel/Program.cs (for this file).

namespace LinkWheel
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static int Main()
        {
            // Immediately capture the cursor position. This allows us to center the Link Wheel around
            // the location where the link was clicked.
            //
            // This is still not perfect. We may want the system tray tool to record mouse clicks using
            // something like https://stackoverflow.com/a/1112959/6876989, and then have it pass the most
            // recent click. Technically the user can click something else in that time too, but it's less
            // likely, and would still have the same behavior we are seeing now.
            var cursorPosition = Cursor.Position;

            // Always ensure LinkWheel is installed before running a command. No point in making the user
            // run an install command before being able to use the executable.
            Installer.EnsureInstalled();

            string[] args = Environment.GetCommandLineArgs()[1..];
            
            if (args.Length == 0)
            {
                // Do nothing if there are no args. TODO: Show a version number dialog or something, idk.
                return 0;
            }

            var parser = new Parser(settings => settings.EnableDashDash = true);
            List<IdelAction> actions = Task.Run(() => parser
                    .ParseArguments<Serve>(args)
                    .MapResult(
                        (Serve verb) => verb.ExecuteAsync(),
                        errs => Task.FromResult(new List<IdelAction>())
                    )).ConfigureAwait(false).GetAwaiter().GetResult();

            if (actions.Count == 0)
            {
                return 1;
            }
            else if (actions.Count == 1)
            {
                // Don't open the option wheel if there's only one option.

                // TODO: Add a flag for enabling this, as well as better logging with timestamps
# if DEBUG
                File.WriteAllText(Path.Combine(LinkWheelConfig.DataDirectory, "invocation.txt"), Environment.CommandLine);
                File.WriteAllText(Path.Combine(LinkWheelConfig.DataDirectory, "lastLink.txt"), actions[0].Command);
# endif
                // TODO: A library might handle argument parsing better.
                string command = actions[0].Command.Trim();
                Regex word = new(@"\S+");
                string fileName;
                string arguments;
                if (command.StartsWith('"'))
                {
                    fileName = command[1..command.IndexOf('"', 1)];
                    arguments = command[(command.IndexOf('"', 1) + 1)..];
                }
                else
                {
                    fileName = word.Match(command).Value;
                    arguments = command[(fileName.Length + 1)..];
                }

                Process process = new()
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        FileName = fileName,
                        WorkingDirectory = actions[0].CommandWorkingDirectory,
                        Arguments = arguments
                    },
                };
                process.Start();
            }
            else
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.Run(new Form1(cursorPosition, actions));
            }
            return 0;
        }
    }
}
