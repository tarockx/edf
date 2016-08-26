using HtmlAgilityPack;
using libEraDeiFessi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;


namespace EDFPlugin.AllUC
{
    class AllUCParser
    {
        private string pluginID;

        public AllUCParser(string pluginID)
        {
            this.pluginID = pluginID;
        }

        public SearchResult PerformSearch(string searchTerm, bool streamMode)
        {
            string baseurl = streamMode ? Constants.AllUCStreamSearchSearchUrl : Constants.AllUCDownloadSearchSearchUrl;
            string searchurl = baseurl.Replace("$searchterm$", searchTerm.Replace(' ', '+'));
            return GetResultPage(searchurl);
        }

        //private string GetPage(string link, string elementToWatchFor)
        //{
        //    string resp = string.Empty;
        //    var th = new Thread(() =>
        //    {
        //        try
        //        {
        //            using (var browser = new WatiN.Core.IE(link))
        //            {
        //                browser.ShowWindow(NativeMethods.WindowShowStyle.Hide);
        //                //var maindiv = browser.Div(Find.ById("resultlist"));
        //                int counter = 0;
        //                browser.WaitForComplete(10);
        //                while (counter < 20 && !browser.Elements.Exists(elementToWatchFor))
        //                {
        //                    Thread.Sleep(500);
        //                    counter++;
        //                }

        //                //resp = skiplink.Url;

                        
        //                resp = browser.Html;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //            resp = string.Empty;
        //        }

        //    });


        //    th.SetApartmentState(ApartmentState.STA); //necessario per WatIn
        //    th.Start();
        //    th.Join();

        //    return resp;

        //}

        public SearchResult GetResultPage(string url)
        {
            string qresult = null;
            List<Bookmark> res = new List<Bookmark>();

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                qresult = reader.ReadToEnd();

                //qresult = GetPage(url, "resultlist");

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(qresult);

                var articles = doc.DocumentNode.SelectNodes("//div[@id='resultlist']/descendant::div/descendant::div[@class='title']");
                if (articles == null)
                    return null;

                foreach (var article in articles)
                {
                    var linknode = article.SelectSingleNode("descendant::a");
                    if (linknode == null || article.ParentNode.InnerText.ToLower().Contains("promoted link") || article.ParentNode.SelectSingleNode("descendant::div[@class='sponsoredlink']") != null || article.ParentNode.InnerText.Contains("advertisement"))
                        continue;

                    var link = (Constants.AllUCBaseHostNoTrailingSlash + linknode.GetAttributeValue("href", string.Empty)).Trim();
                    var title = ParsingHelpers.ConvertHtml(linknode.InnerHtml);

                    Bookmark bm = new Bookmark(pluginID, title, link);

                    string subtitle = string.Empty;
                    var subtitlenode = article.ParentNode.SelectSingleNode("descendant::div[contains(@class, 'hoster')]");
                    if (subtitlenode != null)
                    {
                        subtitle = ParsingHelpers.ConvertHtml(subtitlenode.InnerHtml);
                        string[] substrings = subtitle.Replace("\n", " ").Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                        string hoster, size, datestring;
                        DateTime date = new DateTime();
                        hoster = size = datestring = null;

                        for (int i = 0; i < substrings.Length; i++)
                        {
                            substrings[i] = substrings[i].Trim();
                            if (i == 0)
                                hoster = substrings[i];
                            else if (substrings[i].EndsWith(" B") || substrings[i].EndsWith(" KB") || substrings[i].EndsWith(" MB") || substrings[i].EndsWith(" GB"))
                                size = substrings[i];
                            else if (DateTime.TryParseExact(substrings[i], "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out date))
                                datestring = substrings[i];
                        }
                        //var subtitlehostnode = subtitlenode.SelectSingleNode("descendant::a");
                        if (hoster != null || size != null || datestring != null)
                        {
                            bm.Metadata = new LinkMetadata() { Host = hoster == null ? "N/A" : hoster, Date = datestring == null ? "N/A" : datestring, Size = size == null ? "N/A" : size };
                            try {
                                if (title.Contains("."))
                                {
                                    string extension = title.Split('.').Last().ToUpper();
                                    if (extension.Length == 2 || extension.Length == 3)
                                        bm.Metadata.Extension = extension;
                                }
                            } catch { }
                        }

                        bm.Description = subtitle.Replace('\n', ' ').Trim();
                    }

                    res.Add(bm);
                }

                var curres = doc.DocumentNode.SelectSingleNode("//div[@class='pagination']/descendant::li[contains(@class, 'active')]");
                string nextpage = string.Empty;
                var nextres = curres.NextSibling;
                while (nextres != null && !nextres.Name.Equals("li"))
                    nextres = nextres.NextSibling;

                if (nextres != null)
                {
                    nextres = nextres.SelectSingleNode("descendant::a");
                    nextpage = (Constants.AllUCBaseHostNoTrailingSlash + nextres.GetAttributeValue("href", string.Empty)).Replace("&amp;", "&").Trim();
                }

                return new SearchResult() { Result = res, NextPageUrl = nextpage };
            }
            catch (Exception)
            {
                return null;
            }
        }

        private bool AreThereLinksAfter(HtmlNode n)
        {
            while (n.NextSibling != null)
            {
                n = n.NextSibling;
                if (n != null && n.Name == "a")
                    return true;
            }
            return false;
        }

        private bool IsDuplicate(string url, NestBlock nb)
        {
            bool isDuplicate = false;
            foreach (var child in nb.Children)
            {
                ContentNestBlock cnb = child as ContentNestBlock;
                if (cnb == null)
                    continue;

                var links = cnb.Links;

                foreach (var item in links)
                {
                    if (item.Url.ToLower().Equals(url.ToLower()))
                    {
                        isDuplicate = true;
                        break;
                    }

                }
            }

            return isDuplicate;
        }

        public ParseResult ParsePage(string url)
        {
            string qresult = null;
            WebResponse response = null;
            StreamReader reader = null;

            NestedContent result = new NestedContent();

            List<HtmlNode> description = new List<HtmlNode>();

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36";
                response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                qresult = reader.ReadToEnd();

                //qresult = GetPage(url, "resultlistdetails");

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(qresult);

                var maintitle = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'linkdetails')]/descendant::h1");
                var linkdivs = doc.DocumentNode.SelectNodes("//div[contains(@class, 'linkdetails')]/descendant::div[@class = 'linktitleurl']");


                if (linkdivs != null)
                {
                    NestBlock nb = new NestBlock() { Title = maintitle.InnerText.Trim() };

                    foreach (var linkdiv in linkdivs)
                    {
                        try
                        {
                            var decscript = linkdiv.SelectSingleNode("descendant::script").InnerText;
                            string startTag = "decrypt(";
                            string endTag = " ));";
                            decscript = decscript.Substring(decscript.IndexOf(startTag) + startTag.Length);
                            decscript = decscript.Substring(0, decscript.IndexOf(endTag));
                            string[] encryptedVals = decscript.Replace("'", string.Empty).Split(',');
                            string href = decryptUrl(encryptedVals[0].Trim(), encryptedVals[1].Trim());

                            var link = linkdiv.SelectSingleNode("descendant::a");

                            if (href.Contains("alluc.") || string.IsNullOrEmpty(href) || href.StartsWith("/"))
                                continue;

                            var title = link.InnerText.Trim();

                            if (!string.IsNullOrWhiteSpace(href) && !string.IsNullOrWhiteSpace(title) && !IsDuplicate(href, nb))
                            {
                                ContentNestBlock cnb = new ContentNestBlock();
                                cnb.Title = title;
                                cnb.Links.Add(new Link(cnb.Title, href));
                                nb.Children.Add(cnb);
                            }
                        }
                        catch
                        {
                            continue;
                        }

                    }
                    result.Children.Add(nb);
                }

                result.Description = "N/A";
                return new ParseResult(result);
            }
            catch (Exception ex)
            {
                return new ParseResult(ex.Message);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (response != null)
                    response.Close();
            }
        }

        private string decryptUrl(string encoded, string e)
        {
            byte[] bytes = Convert.FromBase64String(encoded);

            string t = "";
            string r = Encoding.UTF7.GetString(bytes);
            int o = 0;
            for (o = 0; o < r.Length; o++)
            {
                string n = r.Substring(o, 1);
                int start = o % e.Length - 1;
                string a = e.Substring(start >= 0 ? start : e.Length + start , 1);
                int ncode = (int)Math.Floor((double)(Convert.ToInt32(n[0]) - Convert.ToInt32(a[0])));
                t += Convert.ToChar(ncode);
            }
            return t;
        }
    }

}
