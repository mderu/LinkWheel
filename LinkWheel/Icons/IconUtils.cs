using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace LinkWheel.Icons
{
    public static class IconUtils
    {
        /// <remarks>
        /// Arbitrary size. In the lower end ballpark of Apple Touch Icons.
        /// </remarks>
        public const int IconSize = 128;

        public static Bitmap DefaultBrowserIcon => DefaultBrowserIconLazy.Value;

        public static Lazy<Bitmap> DefaultBrowserIconLazy => new(() =>
        {
            if (OperatingSystem.IsWindows())
            {
                string browserClass = (string)Registry.GetValue(LinkWheelConfig.Registry.DefaultBrowserHttpKey, LinkWheelConfig.Registry.DefaultBrowserValue, "");
                string browserExePath = ((string)Registry.GetValue($@"HKEY_CLASSES_ROOT\{browserClass}\DefaultIcon", "", "")).Split(',')[0];
                return JumboIcons.GetJumboIcon(browserExePath);
            }
            else
            {
                throw new NotImplementedException("Not implemented for this OS");
            }
        });

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
