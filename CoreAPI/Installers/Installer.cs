using System;

namespace CoreAPI.Installers
{
    public class Installer
    {
        public static void EnsureInstalled()
        {
            if (OperatingSystem.IsWindows())
            {
                new WindowsInstaller().EnsureInstalled();
            }
        }
    }
}
