using System;

namespace CoreAPI.Installers
{
    class Installer
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
