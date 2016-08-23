using HtmlAgilityPack;
using libEraDeiFessi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EDFPlugin.ShareDir
{
    class SDParser
    {
        private string pluginID;

        public SDParser(string pluginID)
        {
            this.pluginID = pluginID;
        }

        public SearchResult PerformSearch(string searchTerm)
        {
            string searchurl = Constants.ShareDirSearchSearchUrl.Replace("$searchterm$", searchTerm.Replace(' ', '+'));
            return GetResultPage(searchurl);
        }

        public SearchResult GetResultPage(string url)
        {
            string qresult = null;
            WebResponse response = null;
            StreamReader reader = null;
            List<Bookmark> res = new List<Bookmark>();

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36";
                response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                qresult = reader.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(qresult);

                var articles = doc.DocumentNode.SelectNodes("//div[contains(@class, 'results')]/descendant::table[@class='tres']/descendant::tr[@class='infoline3']");
                if (articles == null)
                    return null;

                foreach (var article in articles)
                {
                    var linknode = article.SelectSingleNode("descendant-or-self::div[@class='ctagsdiv1']/descendant::a");
                    if (linknode == null)
                        continue;

                    var link = (Constants.ShareDirBaseHostNoTrailingSlash + linknode.GetAttributeValue("href", string.Empty)).Trim();
                    var title = linknode.GetAttributeValue("title", string.Empty).Trim();

                    Bookmark bm = new Bookmark(pluginID, title, link);

                    string subtitle = string.Empty;
                    var subtitlenode = article.NextSibling;
                    while (subtitlenode != null && subtitlenode.Name != "tr" && !subtitlenode.GetAttributeValue("class", string.Empty).Contains("irow"))
                        subtitlenode = subtitlenode.NextSibling;

                    if (subtitlenode != null)
                    {
                        subtitle = ParsingHelpers.ConvertHtml(subtitlenode.InnerHtml.Replace("</div>", "&nbsp;</div>"));
                        if(subtitle.Contains("tags:"))
                            subtitle = subtitle.Substring(0, subtitle.IndexOf("tags:"));
                        bm.Description = subtitle.Replace('\n', ' ').Trim();

                        string[] substrings = subtitle.Replace("\n", " ").Replace("extension", "").Replace("parts", "").Replace("size", "").Replace("date", "").Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                        string hoster = null, size = null, extension = null, date = null, parts = null;

                        int offset = subtitle.Contains("parts:") ? 1 : 0;

                        for (int i = 0; i < substrings.Length; i++)
                        {
                            if (i == 0)
                                hoster = substrings[i].Trim();
                            if (offset > 0 && i == 1)
                                parts = substrings[i].Trim();
                            if (i == 1 + offset)
                                extension = substrings[i].Trim();
                            if (i == 2 + offset)
                                size = substrings[i].Trim();
                            if (i == 3 + offset)
                                date = substrings[i].Trim();
                        }
                        //var subtitlehostnode = subtitlenode.SelectSingleNode("descendant::a");
                        if (hoster != null || size != null || extension != null || date != null || parts != null)
                        {
                            bm.Metadata = new LinkMetadata() {
                                Host = hoster == null ? "N/A" : hoster.Trim(),
                                Extension = extension == null ? "N/A" : extension.Trim().ToUpper(),
                                Size = size == null ? "N/A" : size.Trim(),
                                Date = date == null ? "N/A" : date.Trim(),
                                Parts = parts == null ? 1 : int.Parse(parts.Trim())};
                        }
                    }

                    res.Add(bm);
                }

                var curres = doc.DocumentNode.SelectSingleNode("descendant-or-self::div[@id='page_links']/descendant::span[@class='rdonly']");
                string nextpage = string.Empty;
                var nextres = curres.NextSibling;
                while (nextres != null && !nextres.Name.Equals("a"))
                    nextres = nextres.NextSibling;

                if (nextres != null)
                {
                    nextpage = (Constants.ShareDirBaseHostNoTrailingSlash + nextres.GetAttributeValue("href", string.Empty)).Replace("&amp;", "&").Trim();
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

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(qresult);

                var maintitle = doc.DocumentNode.SelectSingleNode("//table[contains(@class, 'tres')]/descendant::tr[contains(@class, 'infoline3')]/descendant::div[@class='big2']");
                var linksttitles = doc.DocumentNode.SelectNodes("(//table[contains(@class, 'tres')])[1]/descendant::tr[contains(@class, 'rrow') and not(contains(@class, 'sponsor_dlfrom'))]/descendant::td[@class='reslink']/descendant::a");
                //var linksttitlesUnfiltered = doc.DocumentNode.SelectNodes("(//table[contains(@class, 'tres')])[1]/descendant::tr[contains(@class, 'rrow')]/descendant::td[@class='reslink']/descendant::a");
                
                var links = doc.DocumentNode.SelectSingleNode("//pre[@id='dirlinks']").InnerText.Trim().Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                //var maindiv = doc.DocumentNode.SelectSingleNode("//pre[@id='copy_paste_links']");

                if (links != null && linksttitles != null && links.Count() == linksttitles.Count)
                {
                    NestBlock nb = new NestBlock() { Title = maintitle.InnerText.Trim() };

                    for (int i = 0; i < linksttitles.Count; i++)
                    {
                        var href = links[i];
                        var title = linksttitles[i].InnerText.Trim();

                        if (!string.IsNullOrWhiteSpace(href) && !string.IsNullOrWhiteSpace(title) && !IsDuplicate(href, nb))
                        {
                            ContentNestBlock cnb = new ContentNestBlock();
                            cnb.Title = title;
                            cnb.Links.Add(new Link(cnb.Title, href));
                            nb.Children.Add(cnb);
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
    }

}
