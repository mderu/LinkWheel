using LinkWheel.Cli;
using System;
using System.Windows.Forms;
using CommandLine;
using System.Threading.Tasks;

// Links provided to make testing easier:
// http://www.google.com (for the case where we don't want to intercept).
// https://github.com/mderu/LinkWheel/blob/master/LinkWheel/Program.cs (for this file).

namespace LinkWheel
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main()
        {
            // Immediately capture the cursor position. This allows us to center the Link Wheel around
            // the location where the link was clicked.
            //
            // This is still not perfect. We may want the system tray tool to record mouse clicks using
            // something like https://stackoverflow.com/a/1112959/6876989, and then have it pass the most
            // recent click. Technically the user can click something else in that time too, but it's less
            // likely, and would still have the same behavior we are seeing now.
            var cursorPosition = Cursor.Position;

            string[] args = Environment.GetCommandLineArgs()[1..];

            var parser = new Parser(settings => settings.EnableDashDash = true);
            if (args.Length == 0)
            {
                // Running without a verb should start the system tray application.
                // As a workaround for having no verbs, we open check the # of arguments.
                Application.Run(new SystemTrayApplicationContext());
                return 0;
            }
            else
            {
                if (args[0].EndsWith("linkWheel.exe"))
                {
                    args = args[1..];
                }
                int result = Task.Run(() => parser
                    .ParseArguments<
                            Disable,
                            Enable,
                            GetUrl,
                            Install,
                            OpenInDefaultBrowser,
                            RegisterRepo,
                            Serve,
                            Uninstall
                        >(args)
                    .MapResult(
                        (Disable verb) => verb.ExecuteAsync(),
                        (Enable verb) => verb.ExecuteAsync(),
                        (GetUrl verb) => verb.ExecuteAsync(),
                        (Install verb) => verb.ExecuteAsync(),
                        (OpenInDefaultBrowser verb) => verb.ExecuteAsync(),
                        (RegisterRepo verb) => verb.ExecuteAsync(),
                        (Serve verb) => verb.ExecuteAsync(cursorPosition),
                        (Uninstall verb) => verb.ExecuteAsync(),
                        errs => Task.FromResult(1)
                    )).ConfigureAwait(false).GetAwaiter().GetResult();
                return result;
            }
        }
    }
}
