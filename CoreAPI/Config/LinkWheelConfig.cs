using System;
using System.IO;

namespace CoreAPI.Config
{
    public static class LinkWheelConfig
    {
        public const string ApplicationName = "LinkWheel";
        public static string InstallDirectory => installPath.Value;

        public static string IconCachePath => GetIconCachePath.Value;
        private static Lazy<string> GetIconCachePath = new(() => Path.Combine(InstallDirectory, "iconCachePath"));

        public static string TrackedReposFile => Path.Combine(installPath.Value, "trackedRepos.json");
        private static readonly Lazy<string> installPath = new(() =>
        {
            if (OperatingSystem.IsWindows())
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "linkWheel");
            }
            else
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".linkWheel");
            }
        });

        public static class Registry
        {
            public static readonly string ClassKey = @$"SOFTWARE\Classes\{ApplicationName}";

            public const string DefaultBrowserHttpKey = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\Shell\Associations\URLAssociations\http\UserChoice";
            public const string DefaultBrowserHttpsKey = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\Shell\Associations\URLAssociations\https\UserChoice";
            public const string DefaultBrowserValue = "ProgId";
            public const string DefaultBrowserProgId = "MSEdgeHTM";
            /// <remarks>
            /// This value is under <see cref="ClassKey"/> within the Registry.
            /// </remarks>
            public const string EnabledValue = "Enabled";
        }
    }
}
