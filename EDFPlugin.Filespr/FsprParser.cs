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

namespace EDFPlugin.Filespr
{
    class FsprParser
    {
        private string pluginID;

        public FsprParser(string pluginID)
        {
            this.pluginID = pluginID;
        }

        public SearchResult PerformSearch(string searchTerm)
        {
            string searchurl = Constants.FilesprSearchUrl.Replace("$searchterm$", searchTerm.Trim().Replace(' ', '-')).Replace("$searchinitial$", searchTerm.Trim().Substring(0, 1));
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

                var articles = doc.DocumentNode.SelectNodes("//div[@class='info2']");
                if (articles == null)
                    return null;

                foreach (var article in articles)
                {
                    try
                    {
                        if (article.InnerText.ToLower().Contains("sponsored link"))
                            continue;

                        var linknode = article.SelectNodes("descendant-or-self::div/div")[0].SelectSingleNode("descendant-or-self::a");
                        if (linknode == null)
                            continue;

                        var link = (Constants.FilesprBaseHostNoTrailingSlash + linknode.GetAttributeValue("href", string.Empty)).Trim();
                        var title = ParsingHelpers.ConvertHtml(linknode.InnerHtml).Replace("\n", " ").Trim();

                        Bookmark bm = new Bookmark(pluginID, title, link);

                        string subtitle = string.Empty;
                        var subtitlenode = article.SelectSingleNode("descendant-or-self::p");
                        if (subtitlenode != null)
                        {
                            subtitle = ParsingHelpers.ConvertHtml(subtitlenode.InnerHtml);
                            bm.Description = subtitle.Replace('\n', ' ').Replace('\t', ' ').Trim();

                            string[] substrings = subtitle.Replace("\n", " ").Replace('\t', ' ').Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                            string hoster = null, size = null, extension = null;

                            for (int i = 0; i < substrings.Length; i++)
                            {
                                if (i == 0)
                                    hoster = substrings[i].Trim();
                                if (i == 1)
                                    extension = substrings[i].Trim();
                                if (i == 2)
                                    size = substrings[i].Trim();
                            }
                            //var subtitlehostnode = subtitlenode.SelectSingleNode("descendant::a");
                            if (hoster != null || size != null || extension != null)
                            {
                                bm.Metadata = new LinkMetadata() { Host = hoster == null ? "N/A" : hoster.Trim(), Extension = extension == null ? "N/A" : extension.Trim().ToUpper(), Size = size == null ? "N/A" : size.Trim() };
                            }
                        }

                        res.Add(bm);
                    }
                    catch (Exception)
                    {
                        continue;
                    }                    
                }

                var nextreses = doc.DocumentNode.SelectNodes("descendant-or-self::div[@class='str']/ul/li");
                HtmlNode nextreslink = null;
                string nextpage = string.Empty;
                if (nextreses != null)
                {
                    foreach (var r in nextreses)
                    {
                        try
                        {
                            var span = r.SelectSingleNode("descendant::span");
                            if (span == null)
                                continue;

                            if (nextreses.Last() == r)
                                nextreslink = doc.DocumentNode.SelectSingleNode("descendant-or-self::div[@class='str']/span[@class='next']/a");
                            else
                                nextreslink = r.NextSibling.SelectSingleNode("descendant::a");
                            break;
                        }
                        catch (Exception)
                        {
                            
                        }
                    }
                    if(nextreslink != null)
                        nextpage = (Constants.FilesprBaseHostNoTrailingSlash + nextreslink.GetAttributeValue("href", string.Empty)).Replace("&amp;", "&").Trim();
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

                var maintitle = doc.DocumentNode.SelectSingleNode("//div[@class='filelist']/table");
                var links = doc.DocumentNode.SelectSingleNode("//textarea[@id='copy-links']").InnerText.Split("\n".ToCharArray());

                if (maintitle != null)
                {
                    NestBlock nb = new NestBlock() { Title = "Links" };
                    var titles = maintitle.SelectNodes("descendant-or-self::tr");
                    if (titles != null)
                    {
                        for (int i = 0; i < titles.Count; i++ )
                        {
                            var titlenode = titles[i].SelectSingleNode("descendant-or-self::td[2]");
                            string title = "[untitled]";
                            if (titlenode != null)
                                title = titlenode.InnerText;

                            string href = links[i];

                            if (!string.IsNullOrWhiteSpace(href) && !IsDuplicate(href, nb))
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
