using CoreAPI.Config;
using CoreAPI.Utils;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

namespace CoreAPI.Icons
{
    public static class IconUtils
    {
        /// <remarks>
        /// Arbitrary size. In the lower end ballpark of Apple Touch Icons.
        /// </remarks>
        public const int IconSize = 128;

        public static IconResult DefaultBrowserIcon => DefaultBrowserIconLazy.Value;

        public static Lazy<IconResult> DefaultBrowserIconLazy => new(() =>
        {
            string browserExePath;
            if (OperatingSystem.IsWindows())
            {
                string? browserClass = (string?)Registry.GetValue(
                    LinkWheelConfig.Registry.DefaultBrowserHttpKey, 
                    LinkWheelConfig.Registry.DefaultBrowserValue, 
                    LinkWheelConfig.Registry.DefaultBrowserProgId);
                if (browserClass is null)
                {
                    throw new Exception($"The registry key {LinkWheelConfig.Registry.DefaultBrowserHttpKey} does not exist.");
                }
                string browserKey = $@"HKEY_CLASSES_ROOT\{browserClass}\DefaultIcon";
                browserExePath = ((string?)Registry.GetValue(browserKey, "", "") ?? "").Split(',')[0];
                if (string.IsNullOrWhiteSpace(browserExePath))
                {
                    throw new Exception($"The registry key {browserKey} does not exist.");
                }
            }
            else
            {
                throw new NotImplementedException("Not implemented for this OS");
            }
            return FetchIcon(browserExePath);
        });

        public static IconResult FetchIcon(string userRequestString)
        {
            if (userRequestString.StartsWith("http"))
            {
                return AppleTouchIcons.GetFromUrl(new Uri(userRequestString));
            }
            string localFile = FileUtils.GetFullNormalizedPath(userRequestString);
            if (File.Exists(localFile))
            {
                string extension = Path.GetExtension(localFile).ToLower();

                // The extensions here are limited by what is supported by Image.FromFile. See
                // https://docs.microsoft.com/en-us/dotnet/api/system.drawing.image.fromfile
                string[] supportedImageExtensions = new[] { ".bmp", ".gif", ".jpg", ".jpeg", ".png", ".tiff" };
                if (supportedImageExtensions.Contains(extension))
                {
                    return new((Bitmap)Image.FromFile(localFile), localFile);
                }

                string[] cacheableExtensions = new[] { ".exe", ".dll" };
                return JumboIcons.GetJumboIcon(
                    localFile,
                    shouldCache: cacheableExtensions.Contains(Path.GetExtension(localFile))
                );
            }
            else if (FileUtils.TryGetInstalledExe(userRequestString, out string? exePath))
            {
                // Forgiveness: will be non-null if found.
                return JumboIcons.GetJumboIcon(exePath!, shouldCache: true);
            }
            else
            {
                throw new Exception(
                    $"I don't know how to parse the icon for the string `{userRequestString}`. " +
                    $"It's not a file or URL.");
            }
        }

        /// <summary>
        /// Returns the path to a website's icon, if it exists.
        /// </summary>
        public static bool TryGetCachedWebsiteIconPath(Uri url, out string localPath)
        {
            string host = url.Host;
            localPath = Path.Combine(LinkWheelConfig.IconCachePath, $"{host}.png");
            if (File.Exists(localPath))
            {
                return true;
            }
            return false;
        }

        public static IconResult GetIconForUrl(string url)
        {
            return AppleTouchIcons.GetFromUrl(new Uri(url));
        }
        public static IconResult GetIconForUrl(Uri url)
        {
            return AppleTouchIcons.GetFromUrl(url);
        }

        public static Bitmap Compose(Bitmap primary, Bitmap secondary)
        {
            Bitmap bmp = new(IconSize, IconSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawImage(primary, 0, 0, IconSize, IconSize);
            g.DrawImage(secondary, IconSize * 3 / 5, IconSize * 3 / 5, IconSize * 2 / 5, IconSize * 2 / 5);
            return bmp;
        }

        public static Bitmap RoundCorners(Bitmap startImage)
        {
            return RoundCorners(startImage, startImage.Width / 3, Color.Transparent);
        }

        public static Bitmap RoundCorners(Bitmap startImage, int cornerRadius)
        {
            return RoundCorners(startImage, cornerRadius, Color.Transparent);
        }

        public static Bitmap RoundCorners(Bitmap startImage, int cornerRadius, Color backgroundColor)
        {
            int diameter = cornerRadius * 2;
            Bitmap roundedImage = new(startImage.Width, startImage.Height);

            using Graphics g = Graphics.FromImage(roundedImage);
            Brush brush = new TextureBrush(startImage);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.Clear(backgroundColor);

            GraphicsPath gp = new();
            gp.AddArc(0, 0, diameter, diameter, 180, 90);
            gp.AddArc(0 + roundedImage.Width - diameter, 0, diameter, diameter, 270, 90);
            gp.AddArc(0 + roundedImage.Width - diameter, 0 + roundedImage.Height - diameter, diameter, diameter, 0, 90);
            gp.AddArc(0, 0 + roundedImage.Height - diameter, diameter, diameter, 90, 90);

            g.FillPath(brush, gp);
            return roundedImage;
        }
    }
}
