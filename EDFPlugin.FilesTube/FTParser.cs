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

namespace EDFPlugin.FilesTube
{
    class FTParser
    {
        private string pluginID;

        public FTParser(string pluginID)
        {
            this.pluginID = pluginID;
        }

        public SearchResult PerformSearch(string searchTerm, string extension)
        {
            string searchurl = Constants.FilesTubeSearchUrl.Replace("$searchterm$", searchTerm.Replace(' ', '+')).Replace("$extension$", extension);
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

                var articles = doc.DocumentNode.SelectNodes("//div[@id='results']/descendant::div[@class='r']");
                if (articles == null)
                    return null;

                foreach (var article in articles)
                {
                    var linknode = article.SelectSingleNode("descendant-or-self::a[@class='rL']");
                    if (linknode == null)
                        continue;

                    var link = (Constants.FilestTubeBaseHostNoTrailingSlash + linknode.GetAttributeValue("href", string.Empty)).Trim();
                    var title = ParsingHelpers.ConvertHtml(linknode.InnerHtml).Replace("\n", " ").Trim();

                    Bookmark bm = new Bookmark(pluginID, title, link);

                    string subtitle = string.Empty;
                    var subtitlenode = article.SelectSingleNode("descendant-or-self::span[contains(@class, 'eT')]");
                    if (subtitlenode == null)
                    {
                        subtitlenode = linknode.NextSibling;
                    }

                    if (subtitlenode != null)
                    {
                        subtitle = ParsingHelpers.ConvertHtml(subtitlenode.InnerHtml);
                        var sibling = subtitlenode.NextSibling;
                        while (sibling != null && !sibling.GetAttributeValue("class", string.Empty).Equals("rate"))
                        {
                            if (!sibling.GetAttributeValue("class", string.Empty).Contains("rSt"))
                                subtitle += " " + ParsingHelpers.ConvertHtml(sibling.OuterHtml);
                            sibling = sibling.NextSibling;
                        }

                        bm.Description = subtitle.Replace('\n', ' ').Trim();
                    }


                    res.Add(bm);
                }

                var nextres = doc.DocumentNode.SelectSingleNode("descendant-or-self::div[@id='pager']");
                string nextpage = string.Empty;
                if (nextres != null)
                {
                    nextres = nextres.SelectSingleNode("descendant-or-self::a[@class='nt']");
                    if (nextres != null)
                        nextpage = (Constants.FilestTubeBaseHostNoTrailingSlash + nextres.GetAttributeValue("href", string.Empty)).Replace("&amp;", "&").Trim();
                }

                return new SearchResult() { Result = res, NextPageUrl = nextpage };
            }
            catch (Exception)
            {
                return null;
            }
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

                var maintitle = doc.DocumentNode.SelectSingleNode("//div[@id='js_files_list']");
                var maindiv = doc.DocumentNode.SelectSingleNode("//pre[@id='copy_paste_links']");

                if (maintitle != null)
                {
                    NestBlock nb = new NestBlock() { Title = "Links" };
                    var links = maintitle.SelectNodes("descendant::a");
                    if (links != null)
                    {
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
