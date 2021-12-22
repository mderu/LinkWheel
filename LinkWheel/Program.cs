using System;
using System.Windows.Forms;
using CommandLine;
using System.Threading.Tasks;
using System.Collections.Generic;
using CoreAPI.Installers;
using CoreAPI.Utils;
using LinkWheel.Cli;
using CoreAPI.Models;

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
                // Running without a verb should start the system tray application.
                // As a workaround for having no verbs, we open check the # of arguments.
                Application.Run(new SystemTrayApplicationContext());
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
                CliUtils.SimpleInvoke(actions[0].Command, actions[0].CommandWorkingDirectory);
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
