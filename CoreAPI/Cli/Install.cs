using CommandLine;
using CoreAPI.Config;
using CoreAPI.Installers;
using CoreAPI.OutputFormat;
using System;
using System.IO;
using System.Threading.Tasks;
using CliWrap;
using System.Linq;

namespace CoreAPI.Cli
{
    [Verb("install", HelpText = HelpText)]
    public class Install
    {
        public const string HelpText = "Installs LinkWheel and adds it to your system path.";
        public Task<OutputData> ExecuteAsync()
        {
            if (OperatingSystem.IsWindows())
            {
                // TODO: Figure out the best way to support installing both the debug executable and release.

                // https://github.com/mderu/LinkWheel/blob/master/LinkWheel/Program.cs
                string[] xcopyArgs = new[] {
                    Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)!,
                    LinkWheelConfig.InstallDirectory + "\\",
                    "/s",
                    "/e",
                    "/y",
                };
                Console.WriteLine("Running: " + string.Join(" ", xcopyArgs.Select(arg => $"\"{arg}\"").Prepend("xcopy")));

                var cmd = CliWrap.Cli
                    .Wrap($"xcopy")
                    .WithArguments(xcopyArgs)
                    .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                    .WithStandardErrorPipe(PipeTarget.ToDelegate(Console.Error.WriteLine))
                    .ExecuteAsync();
                new WindowsInstaller().ForceInstall();
                return Task.FromResult(new OutputData(0, new(), ""));
            }
            return Task.FromResult(new OutputData(1, new(), "Only possible to install on Windows for now."));
        }
    }
}
