using CliWrap;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace LinkWheelVSIX
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CopyLink
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("802e3c2c-27d8-4ba4-a9a7-26c81e1ac18f");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyLink"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CopyLink(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CopyLink Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in CopyLink's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CopyLink(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();


            DTE dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
            TextSelection textSelection = (TextSelection)dte.ActiveDocument.Selection;

            var stdOutBuffer = new StringBuilder();
            var stdErrBuffer = new StringBuilder();
            var result = Cli.Wrap("linkWheelCli")
                .WithArguments($"register --path {dte.ActiveDocument.FullName} ")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            if (result.ExitCode == 0)
            {
                stdOutBuffer.Clear();
                stdErrBuffer.Clear();
                result = Cli.Wrap("linkWheelCli")
                    .WithArguments(
                        $"get-url --file {dte.ActiveDocument.FullName} "
                            + $"--start-line {textSelection.TopLine} "
                            + (textSelection.TopLine != textSelection.BottomLine ? $"--end-line {textSelection.BottomLine} " : ""))
                    .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                    .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                    .WithValidation(CommandResultValidation.None)
                    .ExecuteAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }

            if (result.ExitCode == 0)
            {
                Clipboard.SetText(stdOutBuffer.ToString().Trim());
            }
            else
            {
                string message = string.Format(CultureInfo.CurrentCulture, "Error getting link: " + stdOutBuffer + "\n" + stdErrBuffer, GetType().FullName);
                string title = "Copy Error";

                // Show a message box to prove we were here
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    message,
                    title,
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }

        }
    }
}
