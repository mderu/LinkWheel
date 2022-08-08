using System;
using System.IO;

namespace CoreAPI.Config
{
    public static class LinkWheelConfig
    {
        public const string ApplicationName = "LinkWheel";
        public static string DataDirectory => dataPath.Value;
        public static string InstallDirectory => Path.Combine(DataDirectory, "bin");
        public static string PluginDirectory => Path.Combine(DataDirectory, "plugins");

        public static string IconCachePath => GetIconCachePath.Value;
        private static Lazy<string> GetIconCachePath = new(() => Path.Combine(DataDirectory, "iconCachePath"));

        public static string DatabaseFile => Path.Combine(dataPath.Value, "liteDB.db");

        private static readonly Lazy<string> dataPath = new(() =>
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
