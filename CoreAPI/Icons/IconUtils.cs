using CoreAPI.Config;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

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
                string browserClass = (string)Registry.GetValue(LinkWheelConfig.Registry.DefaultBrowserHttpKey, LinkWheelConfig.Registry.DefaultBrowserValue, "");
                browserExePath = ((string)Registry.GetValue($@"HKEY_CLASSES_ROOT\{browserClass}\DefaultIcon", "", "")).Split(',')[0];
            }
            else
            {
                throw new NotImplementedException("Not implemented for this OS");
            }
            return GetIconForFile(browserExePath);
        });

        public static IconResult GetIconForFile(string filepath)
        {
            string cachePath = Path.Combine(LinkWheelConfig.IconCachePath, $"{filepath}.png");
            if (File.Exists(cachePath))
            {
                return new(new Bitmap(cachePath), cachePath);
            }

            if (OperatingSystem.IsWindows())
            {
                return JumboIcons.GetJumboIcon(filepath);
            }
            else
            {
                throw new NotImplementedException("Not implemented for this OS");
            }
        }

        /// <summary>
        /// Returns the path to a website's icon, if it exists.
        /// </summary>
        public static bool TryGetWebsiteIconPath(Uri url, out string localPath)
        {
            string host = url.Host;
            string localCachePath = Path.Combine(LinkWheelConfig.IconCachePath, $"{host}.png");
            if (File.Exists(localCachePath))
            {
                localPath = localCachePath;
                return true;
            }
            localPath = null;
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
