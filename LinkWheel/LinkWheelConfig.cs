using System;
using System.IO;

namespace LinkWheel
{
    public static class LinkWheelConfig
    {
        public static string CacheDirectory => CachePathLazy.Value;
        public static string TrackedReposFile => Path.Combine(CachePathLazy.Value, "trackedRepos.json");
        private static readonly Lazy<string> CachePathLazy = new(() =>
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
            public static readonly string ClassKey = @$"SOFTWARE\Classes\{nameof(LinkWheel)}";

            public const string DefaultBrowserHttpKey = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\Shell\Associations\URLAssociations\http\UserChoice";
            public const string DefaultBrowserHttpsKey = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\Shell\Associations\URLAssociations\https\UserChoice";
            public const string DefaultBrowserValue = @"ProgId";
            /// <remarks>
            /// This value is under <see cref="ClassKey"/> within the Registry.
            /// </remarks>
            public const string EnabledValue = "Enabled";
        }
    }
}
