using LinkWheel.Cli;
using System;
using CommandLine;
using System.Threading.Tasks;
using CoreAPI.Cli;
using System.Linq;
using CoreAPI.OutputFormat;

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
            string[] args = Environment.GetCommandLineArgs()[1..];

            int argStart = 0;
            // TODO: Handle global arguments that don't take in a following argument.
            for (; argStart < args.Length; argStart += 2)
            {
                if (!args[argStart].StartsWith('-'))
                {
                    break;
                }
            }

            OutputFormatter outputFormatter = new Parser()
                .ParseArguments<HandleOutput>(args[0..argStart].Prepend("handle-output"))
                .MapResult(
                    (HandleOutput verb) => verb.Create(),
                    errs => null);

            var parser = new Parser(settings => settings.EnableDashDash = true);
            OutputData result = Task.Run(() => parser
                .ParseArguments<
                        Disable,
                        Enable,
                        GetRoot,
                        GetUrl,
                        Install,
                        GetRegistration,
                        OpenInDefaultBrowser,
                        RegisterRepo,
                        ServeClipboard,
                        GetActions,
                        Uninstall
                    >(args[argStart..])
                .MapResult(
                    (Disable verb) => verb.ExecuteAsync(),
                    (Enable verb) => verb.ExecuteAsync(),
                    (GetRoot verb) => verb.ExecuteAsync(),
                    (GetUrl verb) => verb.ExecuteAsync(),
                    (Install verb) => verb.ExecuteAsync(),
                    (GetRegistration verb) => verb.ExecuteAsync(),
                    (OpenInDefaultBrowser verb) => verb.ExecuteAsync(),
                    (ServeClipboard verb) => verb.ExecuteAsync(),
                    (RegisterRepo verb) => verb.ExecuteAsync(),
                    (GetActions verb) => verb.ExecuteAsync(),
                    (Uninstall verb) => verb.ExecuteAsync(),
                    errs => Task.FromResult(new OutputData(1, new(), "Unable to parse args."))
                )).ConfigureAwait(false).GetAwaiter().GetResult();
            Console.WriteLine(outputFormatter.GetOutput(result));
            return result.ExitCode;
        }
    }
}
