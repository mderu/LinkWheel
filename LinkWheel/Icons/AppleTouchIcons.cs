using HtmlAgilityPack;
using LinkWheel.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;

namespace LinkWheel.Icons
{
    /// <summary>
    /// Apple Touch Icons are the big icons browsers use to show websites on their home pages, and
    /// pretend websites are apps on Mobile.
    /// 
    /// We use these to display where the user is headed when they click on a link.
    /// </summary>
    public class AppleTouchIcons
    {
        public static string IconCachePath => GetIconCachePath.Value;
        private static Lazy<string> GetIconCachePath = new(() =>
            Path.Combine(LinkWheelConfig.CacheDirectory, "iconCachePath")
        );

        public static Dictionary<string, Bitmap> hostToIcon = new Dictionary<string, Bitmap>();

        private static Bitmap DownloadBitmap(WebClient client, Uri imageUrl)
        {
            byte[] data = client.DownloadData(imageUrl);

            using (MemoryStream mem = new MemoryStream(data))
            {
                using (var downloadedImage = Image.FromStream(mem))
                {
                    return new Bitmap(downloadedImage);
                }
            }
        }

        public static Bitmap GetFromUrl(Uri url)
        {
            string host = url.Host;
            if (hostToIcon.ContainsKey(host))
            {
                return hostToIcon[host];
            }

            string localCachePath = Path.Combine(IconCachePath, $"{host}.png");
            if (File.Exists(localCachePath))
            {
                hostToIcon[host] = (Bitmap)Image.FromFile(localCachePath);
                return hostToIcon[host];
            }

            // Attempt to download from common location.
            Bitmap icon = null;
            bool iconIsMissing = false;
            using (var client = new WebClient())
            {
                try
                {
                    icon = DownloadBitmap(client, new Uri($"{url.Scheme}://{url.Host}/apple-touch-icon.png"));
                }
                catch (WebException)
                {
                    HtmlWeb web = new();
                    HtmlDocument htmlDoc = web.Load(url);
                    var links = htmlDoc.DocumentNode.SelectNodes("/html/head/link");
                    bool broken = false;
                    foreach (var link in links)
                    {
                        if (link.Attributes["rel"].Value == "apple-touch-icon")
                        {
                            broken = true;
                            icon = DownloadBitmap(client, new Uri(url, link.Attributes["href"].Value));
                            break;
                        }
                    }
                    if (!broken)
                    {
                        // Be sad and grab the favicon.
                        broken = false;
                        foreach (var link in links)
                        {
                            if (link.Attributes["rel"].DeEntitizeValue.Contains("shortcut icon"))
                            {
                                broken = true;
                                icon = DownloadBitmap(client, new Uri(url, link.Attributes["href"].DeEntitizeValue));
                                break;
                            }
                        }
                        if (!broken)
                        {
                            icon = Resources.MissingIcon;
                            iconIsMissing = true;
                        }
                    }
                }
            }
            if (!iconIsMissing)
            {
                Directory.CreateDirectory(IconCachePath);
                icon.Save(localCachePath);
                hostToIcon[host] = icon;
            }
            return icon;
        }
    }
}
