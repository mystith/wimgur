using HtmlAgilityPack;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models.Impl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace wallhaven
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                StringBuilder syntax = new StringBuilder();
                syntax.Append("wimgur, a wallhaven / imgur album downloader"); syntax.Append(Environment.NewLine);
                syntax.Append("syntax: wallhaven [parameters]"); syntax.Append(Environment.NewLine);
                syntax.Append("parameters qualify as following: website,search"); syntax.Append(Environment.NewLine);
                syntax.Append("there can be multiple parameters"); syntax.Append(Environment.NewLine);
                syntax.Append("website = either wallhaven or imgur, capitalization doesn't matter"); syntax.Append(Environment.NewLine);
                syntax.Append("search = any valid search on wallhaven, or any valid album on imgur"); syntax.Append(Environment.NewLine);
                Console.WriteLine(syntax.ToString());
                args = Console.ReadLine().Split(' ');
            }

            using (WebClient wc = new WebClient()) {
                foreach (string s in args.Where(o => o.ToLower().Contains("wallhaven")))
                {
                    string search = s.Split(',')[1];
                    if (search.StartsWith("id:"))
                    {
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(wc.DownloadString($"https://alpha.wallhaven.cc/tag/{ search.Split(new string[] { "id:" }, StringSplitOptions.None)[1] }"));
                        search = doc.DocumentNode.Descendants("h1").FirstOrDefault(o => o.HasClass("tagname")).InnerText;
                    }

                    Directory.CreateDirectory(search);

                    int i = 1;
                    while (true)
                    {
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(wc.DownloadString($"https://alpha.wallhaven.cc/search?q={ search }&page={ i }"));
                        doc.Save("TEMP1.html");

                        IEnumerable<HtmlNode> list = doc.DocumentNode.SelectNodes("//*[contains(@class,'preview')]") ?? new List<HtmlNode>().AsEnumerable();
                        if (list.Count() == 0) break;
                        foreach (HtmlNode link in list)
                        {
                            doc.LoadHtml(wc.DownloadString(link.Attributes["href"].Value));
                            doc.Save("TEMP2.html");

                            HtmlNode picture = doc.DocumentNode.SelectSingleNode("//img[contains(@id,'wallpaper')]");
                            string pictureLink = "https:" + picture.Attributes["src"].Value;
                            string pictureName = pictureLink.Replace("https://wallpapers.wallhaven.cc/wallpapers/full/", "");

                            wc.DownloadFile(pictureLink, search + "\\" + pictureName);

                            Console.WriteLine(pictureName);
                        }

                        i++;
                    }
                }

                File.Delete("TEMP1.html");
                File.Delete("TEMP2.html");

                foreach (string s in args.Where(o => o.ToLower().Contains("imgur")))
                {
                    string search = s.Split(',')[1];
                    Directory.CreateDirectory(search);

                    var client = new ImgurClient("569dfa57bdec8ec");
                    var endpoint = new AlbumEndpoint(client);
                    var album = endpoint.GetAlbumAsync(search).GetAwaiter().GetResult();
                    foreach (Image img in album.Images)
                    {
                        string name = img.Id + MimeTypeMap.List.MimeTypeMap.GetExtension(img.Type).FirstOrDefault();
                        wc.DownloadFile(img.Link, search + "\\" + name);
                        Console.WriteLine(name);
                    }
                }
            }

            if (args.FirstOrDefault().Contains("merge"))
            {
                string dirname = string.Join(" ", args).Split(',').LastOrDefault();
                DirectoryInfo di = Directory.CreateDirectory(dirname);
                DirectoryInfo current = new DirectoryInfo(Environment.CurrentDirectory);
                foreach (FileInfo fi in current.GetFiles("*.*", SearchOption.AllDirectories).Where(o => o.FullName != AppDomain.CurrentDomain.BaseDirectory))
                {
                    fi.MoveTo(di.FullName + "\\" + fi.Name);
                }

                foreach (DirectoryInfo dir in current.GetDirectories().Where(o => o.Name != di.Name))
                {
                    dir.Delete();
                }
            }
        }
    }
}
