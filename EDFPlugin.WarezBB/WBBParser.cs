using HtmlAgilityPack;
using libEraDeiFessi;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace EDFPlugin.WarezBB
{
    class WBBParser
    {
        private CookieAwareWebClient wc;
        private WBBPlugin myMaster;

        Options options { get { return myMaster.Options as Options; } }

        public WBBParser(WBBPlugin master)
        {
            myMaster = master;
        }

        private void LogMeIn()
        {
            if (string.IsNullOrWhiteSpace(options.Username) || string.IsNullOrWhiteSpace(options.Password))
                throw new Exception("username e/o password non impostate. Impossibile connettersi a WarezBB");

            if (wc == null)
                wc = new CookieAwareWebClient();
            else
                return;

            var loginData = new NameValueCollection();
            loginData.Add("username", options.Username);
            loginData.Add("password", options.Password);
            loginData.Add("autologin", "on");
            loginData.Add("redirect", "search.php%3Famp");
            loginData.Add("login", "Log+in");

            string response = Encoding.UTF8.GetString(wc.UploadValues(Constants.WBB_LoginUrl, "POST", loginData));

            bool success = response.Contains("You have successfully logged in");
            if (!success)
            {
                wc = null;
                throw new Exception("impossibile autenticarsi su WarezBB (username/password errati?)");
            }
        }

        public SearchResult PerformSearch(string searchTerm)
        {
            try
            {
                LogMeIn();
                    

                var searchData = new NameValueCollection();
                searchData.Add("search_keywords", searchTerm);
                searchData.Add("search_terms", "all");
                searchData.Add("search_author", "");
                searchData.Add("search_time", "0");
                searchData.Add("search_fields", "titleonly");
                searchData.Add("sort_by", "0");
                searchData.Add("sort_dir", "DESC");
                searchData.Add("show_results", "topics");
                searchData.Add("return_chars", "200");
                searchData.Add("search_forum[]", "-1");

                string response = Encoding.UTF8.GetString(wc.UploadValues(Constants.WBB_SearchUrl, "POST", searchData));
                return ProcessPage(response);
            }
            catch (Exception ex)
            {
                return new SearchResult() { Error = "Ricerca fallita: " + ex.Message };
            }
        }

        public SearchResult GetResultPage(string url)
        {
            LogMeIn();
            return ProcessPage(wc.DownloadString(url));
        }

        public SearchResult ProcessPage(string pageData)
        {
            List<Bookmark> res = new List<Bookmark>();

            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(pageData);

                var articles = doc.DocumentNode.SelectNodes("//div[@class='topicrow']");
                if (articles == null)
                    return null;

                foreach (var article in articles)
                {
                    try
                    {
                        var postTitleNode = article.SelectSingleNode("descendant::div[@class='description']/descendant::span[@class='topictitle']");
                        if (postTitleNode == null)
                            continue;

                        var postLinks = postTitleNode.SelectNodes("descendant::a");
                        if (postLinks == null || postLinks.Count == 0)
                            continue;

                        string link = null;
                        string title = null;
                        foreach (var linkNode in postLinks)
                        {
                            string href = linkNode.GetAttributeValue("href", "");
                            if (!href.Contains("view=newest") && !string.IsNullOrEmpty(href))
                            {
                                link = string.Concat(Constants.WBB_BaseAddress, "/", href);
                                title = linkNode.InnerText.Trim();
                                break;
                            }
                        }
                        if (link == null || title == null)
                            continue;

                        string subtitle = "";
                        foreach (var item in postTitleNode.ParentNode.ChildNodes)
                        {
                            if (item.Name == "#text")
                                subtitle += item.InnerText.Trim();
                        }

                        Bookmark bm = new Bookmark(myMaster.pluginID, title, link, subtitle);

                        res.Add(bm);
                    }
                    catch (Exception)
                    {
                        continue;
                    }                    
                }

                string nextpage = string.Empty;
                try
                {
                    var nextreses = doc.DocumentNode.SelectNodes("descendant-or-self::span[@class='pagination-wrapper']/a").Last();
                    if (nextreses != null && nextreses.InnerText.Contains("Next"))
                        nextpage = string.Concat(Constants.WBB_BaseAddress, "/", nextreses.GetAttributeValue("href", string.Empty));
                }
                catch (Exception)
                {
                }

                
                return new SearchResult() { Result = res, NextPageUrl = nextpage };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public ParseResult ParsePage(string url)
        {
            string qresult = null;
            NestedContent result = new NestedContent();

            try
            {
                LogMeIn();

                qresult = wc.DownloadString(url);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(qresult);

                var postBody = doc.DocumentNode.SelectSingleNode("//div[@class='postbody']");
                var codeDivs = postBody.SelectNodes("descendant::div[@class='code']/span[@class='inner-content']");

                if (codeDivs != null)
                {

                    Regex linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    for (int i = 0; i < codeDivs.Count; i++)
                    {
                        NestBlock block = new NestBlock();
                        var codeDiv = codeDivs[i];
                        string codeBlock = ParsingHelpers.ConvertHtml(codeDiv.InnerHtml);
                        
                        foreach (Match match in linkParser.Matches(codeBlock))
                        {
                            codeBlock = codeBlock.Replace(match.Value, string.Concat("<a href=\"", match.Value, "\">", match.Value, "</a>"));
                        }
                        codeDiv.InnerHtml = codeBlock.Replace("\n", "</br>");
                    }
                }


                HtmlContent content = new HtmlContent() { Content = postBody.InnerHtml, HideDescriptionPanel = true };
                return new ParseResult(content);

                /*
                var codeDivs = doc.DocumentNode.SelectNodes("//div[@class='code']/span[@class='inner-content']");

                if(codeDivs != null)
                {
                    
                    Regex linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    for (int i = 0; i < codeDivs.Count; i++)
                    {
                        NestBlock block = new NestBlock();
                        var codeDiv = codeDivs[i];
                        string[] lines = ParsingHelpers.ConvertHtml(codeDiv.InnerHtml).Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines)
                        {
                            foreach (Match m in linkParser.Matches(line))
                            {
                                ContentNestBlock cnb = new ContentNestBlock();
                                cnb.Title = string.Concat("[", DomainExtractor.GetDomainFromUrl(m.Value), "] ", DomainExtractor.GetLastPathPieceFromUrl(m.Value));
                                cnb.Links.Add(new Link(cnb.Title, m.Value));
                                block.Children.Add(cnb);
                            }                                
                        }

                        block.Title = "Link block " + i.ToString();

                        if (block.Children.Count > 0)
                            result.Children.Add(block);
                    }
                }

                result.Description = "N/A";
                return new ParseResult(result);
                */
            }
            catch (Exception ex)
            {
                return new ParseResult(ex.Message);
            }
            finally
            {

            }
        }
    }

}
