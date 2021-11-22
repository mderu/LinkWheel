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

        public static IconResult GetFromUrl(Uri url, Bitmap defaultIcon = null)
        {
            string host = url.Host;

            string localCachePath = Path.Combine(LinkWheelConfig.IconCachePath, $"{host}.png");
            if (File.Exists(localCachePath))
            {
                return new((Bitmap)Image.FromFile(localCachePath), localCachePath);
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
                            icon = defaultIcon;
                            iconIsMissing = true;
                        }
                    }
                }
            }
            if (!iconIsMissing)
            {
                Directory.CreateDirectory(LinkWheelConfig.IconCachePath);
                icon.Save(localCachePath);
            }
            return new(icon, localCachePath);
        }
    }
}