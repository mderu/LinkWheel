using LinkWheel.Cli;
using System;
using CommandLine;
using System.Threading.Tasks;
using System.Diagnostics;
using CoreAPI.Cli;

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
        public static int Main()
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.AutoFlush = true;

            string[] args = Environment.GetCommandLineArgs()[1..];

            var parser = new Parser(settings => settings.EnableDashDash = true);
            int result = Task.Run(() => parser
                .ParseArguments<
                        Disable,
                        Enable,
                        GetUrl,
                        Install,
                        OpenInDefaultBrowser,
                        RegisterRepo,
                        GetActions,
                        Uninstall
                    >(args)
                .MapResult(
                    (Disable verb) => verb.ExecuteAsync(),
                    (Enable verb) => verb.ExecuteAsync(),
                    (GetUrl verb) => verb.ExecuteAsync(),
                    (Install verb) => verb.ExecuteAsync(),
                    (OpenInDefaultBrowser verb) => verb.ExecuteAsync(),
                    (RegisterRepo verb) => verb.ExecuteAsync(),
                    (GetActions verb) => verb.ExecuteAsync(),
                    (Uninstall verb) => verb.ExecuteAsync(),
                    errs => Task.FromResult(1)
                )).ConfigureAwait(false).GetAwaiter().GetResult();
            return result;
        }
    }
}
