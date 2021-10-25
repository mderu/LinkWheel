using System;

namespace LinkWheel
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
