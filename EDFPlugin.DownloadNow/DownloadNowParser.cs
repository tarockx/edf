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

namespace EDFPlugin.DownloadNow
{
    class DownloadNowParser
    {
        private string pluginID;

        public DownloadNowParser(string pluginID)
        {
            this.pluginID = pluginID;
        }

        public SearchResult PerformSearch(string searchTerm)
        {
            string searchurl = Constants.DownloadNowSearchUrl.Replace("$searchterm$", searchTerm.Replace(' ', '+'));
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

                var articles = doc.DocumentNode.SelectNodes("//div[contains(@class, 'results')]/descendant::div[contains(@class, 'result--search')]");
                if (articles == null)
                    return null;

                foreach (var article in articles)
                {
                    var linknode = article.SelectSingleNode("descendant-or-self::a[@class='result__link']");
                    if (linknode == null)
                        continue;

                    var link = (Constants.DownloadNowBaseHostNoTrailingSlash + linknode.GetAttributeValue("href", string.Empty)).Trim();
                    var title = linknode.GetAttributeValue("title", string.Empty).Trim();

                    Bookmark bm = new Bookmark(pluginID, title, link);

                    string subtitle = string.Empty;
                    var subtitlenode = linknode.NextSibling;
                    while (subtitlenode != null && subtitlenode.Name != "br") { 
                        subtitle = subtitle + subtitlenode.InnerText;
                        subtitlenode = subtitlenode.NextSibling;
                    }

                    if (subtitle != string.Empty)
                    {
                        subtitle = ParsingHelpers.ConvertHtml(subtitle);
                        bm.Description = subtitle.Replace('\n', ' ').Trim();
                    }

                    res.Add(bm);
                }

                var nextres = doc.DocumentNode.SelectNodes("descendant-or-self::div[@id='pager']/descendant::a[contains(@class, 'pager__link')]");
                string nextpage = string.Empty;
                if (nextres != null && nextres.Count > 0 && (nextres.Count == 2 || !AreThereLinksAfter(nextres.Last())))
                {
                    var nextlink = nextres.Last();
                    nextpage = (Constants.DownloadNowBaseHostNoTrailingSlash + nextlink.GetAttributeValue("href", string.Empty)).Replace("&amp;", "&").Trim();
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

                var maintitle = doc.DocumentNode.SelectSingleNode("//p[contains(@class, 'file__subheader')]");
                var links = doc.DocumentNode.SelectNodes("//div[contains(@class, 'file-box')]/descendant::a[contains(@class, 'file-box__link')]");
                //var maindiv = doc.DocumentNode.SelectSingleNode("//pre[@id='copy_paste_links']");

                if (links != null)
                {
                    NestBlock nb = new NestBlock() { Title = maintitle.InnerText.Trim() };

                    foreach (var link in links)
                    {
                        var href = link.GetAttributeValue("href", string.Empty);
                        var title = link.GetAttributeValue("title", string.Empty);

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
