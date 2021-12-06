using CoreAPI.Config;
using CoreAPI.Utils;
using Microsoft.Win32;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WinInstaller.Properties;

namespace WinInstaller
{
    public partial class InstallForm : Form
    {

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {
            if (IsWindows10OrGreater(17763))
            {
                var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = enabled ? 1 : 0;
                return DwmSetWindowAttribute(handle, (int)attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }

            return false;
        }

        private static bool IsWindows10OrGreater(int build = -1)
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }

        public InstallForm()
        {
            InitializeComponent();
            int usesLightTheme = (int)Registry
                .CurrentUser
                .OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize")
                .GetValue("SystemUsesLightTheme", 0);
            if (usesLightTheme == 0)
            {
                UseImmersiveDarkMode(Handle, true);
            }
        }

        // Create/use a `bin` directory to prevent user data from being wiped upon re-install.
        string InstallPath => Path.Combine(LinkWheelConfig.DataDirectory, "bin");
        string VSExtensionPath => Path.Combine(InstallPath, "vs", "linkWheelVSIX_VS.vsix");
        string VSCodeExtensionPath => Path.Combine(InstallPath, "vscode", "linkWheelVSIX_VSCode.vsix");
        string GlobalConfigPath => Path.Combine(LinkWheelConfig.DataDirectory, ".idelconfig");
        string GlobalConfigResourcesDir => Path.Combine(LinkWheelConfig.DataDirectory, ".ideld");

        private void WipeInstallDirectory()
        {
            if (Directory.Exists(InstallPath))
            {
                Directory.Delete(InstallPath, recursive: true);
            }
        }
        private void UnpackResource(byte[] resource, string destination)
        {
            new ZipArchive(new MemoryStream(resource)).ExtractToDirectory(destination);
        }

        private void InstallButton_Click(object sender, EventArgs e)
        {
            if (linkWheelCheckbox.Checked)
            {
                WipeInstallDirectory();
                UnpackResource(Resources.LinkWheelZip, InstallPath);
                CliUtils.SimpleInvoke("linkWheelCli.exe install", InstallPath);
            }
            if (visualStudioCheckbox.Checked)
            {
                if (!FileUtils.TryGetInstalledExe("devenv.exe", out string devEnvPath))
                {
                    MessageBox.Show(
                        "Unable to install the Visual Studio Extension. Visual Studio is not installed, " +
                        "or a previous version of Visual Studio was uninstalled after the current version " +
                        "was installed. Please run the Visual Studio installer again so devenv.exe can be " +
                        "added to the Windows Registry.");
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(VSExtensionPath));
                    if (File.Exists(VSExtensionPath))
                    {
                        File.Delete(VSExtensionPath);
                    }
                    File.WriteAllBytes(VSExtensionPath, Resources.VSExtension);
                    CliUtils.SimpleInvoke($"VSIXInstaller.exe \"{VSExtensionPath}\"", Path.GetDirectoryName(devEnvPath));
                }
            }
            if (vscodeCheckbox.Checked)
            {
                if (!FileUtils.TryGetInstalledExe("code.cmd", out string outString))
                {
                    MessageBox.Show(
                        "Unable to install the VSCode Extension. Visual Studio Code is not installed, " +
                        "or was removed from the system path.");
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(VSCodeExtensionPath));
                    if (File.Exists(VSCodeExtensionPath))
                    {
                        File.Delete(VSCodeExtensionPath);
                    }
                    File.WriteAllBytes(VSCodeExtensionPath, Resources.VSCodeExtension);
                    CliUtils.SimpleInvoke($"code --install-extension \"{VSCodeExtensionPath}\"", InstallPath);
                }
            }
            if (globalConfigCheckbox.Checked)
            {
                if (Directory.Exists(GlobalConfigResourcesDir))
                {
                    Directory.Delete(GlobalConfigResourcesDir, recursive: true);
                }
                UnpackResource(Resources.ideld, GlobalConfigResourcesDir);
                if (File.Exists(GlobalConfigPath))
                {
                    File.Delete(GlobalConfigPath);
                }
                File.WriteAllText(GlobalConfigPath, Resources.idelconfig);
            }

            MessageBox.Show("Installation complete. You may now close the installer.");
        }

        private void UninstallButton_Click(object sender, EventArgs e)
        {
            if (vscodeCheckbox.Checked)
            {
                if (!FileUtils.TryGetInstalledExe("code.cmd", out string outString))
                {
                    MessageBox.Show(
                        "Unable to uninstall the VSCode Extension. Visual Studio Code is not installed, " +
                        "or was removed from the system path.");
                }
                // If the file has already been deleted, uninstall using the package name.
                else if (!File.Exists(VSCodeExtensionPath))
                {
                    CliUtils.SimpleInvoke($"code --uninstall-extension undefined.codelink", InstallPath);
                }
                else
                {
                    CliUtils.SimpleInvoke($"code --uninstall-extension \"{VSCodeExtensionPath}\"", InstallPath);
                }
                Directory.Delete(Path.GetDirectoryName(VSCodeExtensionPath), recursive: true);
            }
            if (visualStudioCheckbox.Checked)
            {
                if (!FileUtils.TryGetInstalledExe("devenv.exe", out string devEnvPath))
                {
                    MessageBox.Show(
                        "Unable to un-install the Visual Studio Extension. Visual Studio is not installed, " +
                        "or a previous version of Visual Studio was uninstalled after the current version " +
                        "was installed. Please run the Visual Studio installer again so devenv.exe can be " +
                        "added to the Windows Registry.");
                }
                else
                {
                    var unzipDir = Path.Combine(InstallPath, "vs");
                    CliUtils.SimpleInvoke("VSIXInstaller.exe /u:\"LinkWheelVS.f3989fa1-d90e-4d10-9c49-24c2fbfcbcba\"",
                        Path.GetDirectoryName(devEnvPath));
                    Directory.Delete(unzipDir, recursive: true);
                }
            }
            if (linkWheelCheckbox.Checked)
            {
                CliUtils.SimpleInvoke("linkWheelCli.exe uninstall", InstallPath);
                WipeInstallDirectory();
            }
            if (globalConfigCheckbox.Checked)
            {
                if (Directory.Exists(GlobalConfigResourcesDir))
                {
                    Directory.Delete(GlobalConfigResourcesDir, recursive: true);
                }
                if (File.Exists(GlobalConfigPath))
                {
                    File.Delete(GlobalConfigPath);
                }
            }

            MessageBox.Show("Uninstall complete. You may now close the uninstaller.");
        }

        private void InstallForm_Load(object sender, EventArgs e)
        {
            var currentScreen = Screen.FromPoint(Cursor.Position);
            Location = currentScreen.WorkingArea.Location + currentScreen.WorkingArea.Size / 2 - Size / 2;
            Show();
            Activate();
        }
    }
}
