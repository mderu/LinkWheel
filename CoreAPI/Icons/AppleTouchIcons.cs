﻿using HtmlAgilityPack;
using CoreAPI.Config;
using System;
using System.Drawing;
using System.IO;
using System.Net;

namespace CoreAPI.Icons
{
    /// <summary>
    /// Apple Touch Icons are the big icons browsers use to show websites on their home pages, and
    /// pretend websites are apps on Mobile.
    /// 
    /// We use these to display where the user is headed when they click on a link.
    /// </summary>
    public class AppleTouchIcons
    {
        private static Bitmap DownloadBitmap(WebClient client, Uri imageUrl)
        {
            byte[] data = client.DownloadData(imageUrl);

            using MemoryStream mem = new(data);
            using var downloadedImage = Image.FromStream(mem);
            return new Bitmap(downloadedImage);
        }

        /// <remarks>
        /// Caches the icon after gathering it.
        /// </remarks>
        public static IconResult GetFromUrl(Uri url, Bitmap? defaultIcon = null)
        {
            if (IconUtils.TryGetCachedWebsiteIconPath(url, out string localCachePath))
            {
                return new((Bitmap)Image.FromFile(localCachePath), localCachePath);
            }

            // Attempt to download from common location.
            Bitmap? icon = null;
            using (var client = new WebClient())
            {
                try
                {
                    icon = DownloadBitmap(client, new Uri($"{url.Scheme}://{url.Host}/apple-touch-icon.png"));
                }
                catch (WebException)
                {
                    try
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
                        }
                    }
                    catch (WebException)
                    {
                        // If this doesn't work, then the URL requested isn't returning anything.
                        return new(defaultIcon, "");
                    }
                }
            }
            // TODO: Should probably cache that we've already tried to get an icon from this website,
            // but it keeps failing.
            if (icon is not null)
            {
                Directory.CreateDirectory(LinkWheelConfig.IconCachePath);
                // Forgiveness: iconIsMissing is always true if icon can be null.
                icon.Save(localCachePath);
                return new(icon, localCachePath);
            }
            else
            {
                return new(defaultIcon, "");
            }
        }
    }
}
