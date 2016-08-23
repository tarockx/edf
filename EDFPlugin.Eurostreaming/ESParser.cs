using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HtmlAgilityPack;
using libEraDeiFessi;
using System.Web;

namespace EDFPlugin.Eurostreaming
{
    class ESParser
    {
        private string pluginID;
        public enum Section {Film, Series, Anime }

        public ESParser(string pluginID)
        {
            this.pluginID = pluginID;
        }

        public SearchResult GetShowList(Section section)
        {
            string qresult = null;
            WebResponse response = null;
            StreamReader reader = null;

            List<Bookmark> result = new List<Bookmark>();

            try
            {
                String baseurl = string.Empty;
                switch (section)
                {
                    case Section.Film:
                        baseurl = Constants.EurostreamingListaFilmUrl;
                        break;
                    case Section.Series:
                        baseurl = Constants.EurostreamingListaSerieUrl;
                        break;
                    case Section.Anime:
                        baseurl = Constants.EurostreamingListaAnimeUrl;
                        break;
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(baseurl);
                request.Method = "GET";
                response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                qresult = reader.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(qresult);

                var links = doc.DocumentNode.SelectNodes("//div[@class='entry']/descendant::a");

                if (links != null) { 
                    foreach (var item in links)
                    {
                        Bookmark b = new Bookmark(pluginID, ParsingHelpers.ConvertHtml(item.InnerText).Trim(), item.GetAttributeValue("href", string.Empty));
                        if (!string.IsNullOrWhiteSpace(b.Name) && !string.IsNullOrWhiteSpace(b.Url))
                            result.Add(b);
                    }
                }

                return new SearchResult() { Result = result };
            }
            catch (Exception ex)
            {
                String s = ex.Message;
                return null;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (response != null)
                    response.Close();
            }
        }


        public SearchResult PerformSearch(string searchTerm)
        {
            string searchurl = Constants.EurostreamingSearchUrl.Replace("$searchterm$", System.Net.WebUtility.UrlEncode(searchTerm));
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
                response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                qresult = reader.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(qresult);

                var articles = doc.DocumentNode.SelectNodes("//div[@id='content']/descendant::li");
                if (articles == null || articles.Count == 0)
                    return null;

                foreach (var article in articles)
                {
                    var link = article.SelectSingleNode("descendant::div[@class='post-content']/descendant::a");

                    if (link == null)
                        continue;

                    Bookmark bm = new Bookmark(pluginID, System.Net.WebUtility.HtmlDecode(link.InnerText), link.GetAttributeValue("href", string.Empty));
                        res.Add(bm);
                }

                var nextres = doc.DocumentNode.SelectNodes("descendant::div[@class='navigation']/descendant::a");
                string nextpage = string.Empty;
                if (nextres != null)
                {
                    foreach (var item in nextres)
                    {
                        if (item.InnerText.ToLower().Contains("avanti"))
                        {
                            nextpage = item.GetAttributeValue("href", string.Empty);
                            break;
                        }
                    }
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
            WebResponse response = null;
            StreamReader reader = null;

            NestedContent result = new NestedContent();

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

                var maindiv = doc.DocumentNode.SelectSingleNode("//div[@class='entry-content']");
                if (maindiv == null)
                    maindiv = doc.DocumentNode.SelectSingleNode("//div[@id='content']/div[@class='post-entry']/div[@class='entry']");

                if (maindiv != null)
                {
                    var footers = maindiv.SelectNodes("descendant::center");
                    if (footers != null)
                        foreach (var footer in footers)
                        {
                            footer.Remove();
                        }                        
                    var fblike = maindiv.SelectSingleNode("descendant::div[@class='fblike']");
                    if (fblike != null)
                        fblike.Remove();

                    var descNodes = maindiv.SelectNodes("descendant::p");
                    HtmlNode descNode = null;
                    foreach (var item in descNodes)
                    {
                        if (descNode == null || descNode.InnerText.Length < item.InnerText.Length)
                        {
                            descNode = item;
                        }
                    }

                    if (descNode != null)
                    {
                        var descLinks = descNode.SelectNodes("descendant::a");
                        if (descLinks != null)
                        {
                            foreach (var item in descLinks)
                            {
                                item.Remove();
                            }
                        }
                        result.Description = ParsingHelpers.ConvertHtml(descNode.InnerText).Trim();
                        descNode.Remove();
                    }

                    HtmlNode cover = maindiv.SelectSingleNode("descendant::img");
                    if (cover != null)
                    {
                        result.CoverImageUrl = cover.GetAttributeValue("src", String.Empty);
                        cover.Remove();
                    }

                    //Check for page redirect
                    HtmlNode redirectNode = maindiv.SelectSingleNode("descendant::span[contains(text(), 'CLICCA QUI')]");
                    if (redirectNode != null)
                        redirectNode = redirectNode.ParentNode;
                    if(redirectNode != null && redirectNode.Name.Equals("a"))
                    {
                        string redirectUrl = string.Empty;
                        if (redirectNode.GetAttributeValue("onclick", string.Empty).Contains("top.location=atob("))
                        {
                            string redirectUrlEnc = redirectNode.GetAttributeValue("onclick", string.Empty).Replace("top.location=atob('", "").Replace("')", "");
                            redirectUrl = Encoding.UTF8.GetString(Convert.FromBase64String(redirectUrlEnc));
                        }
                        else
                        {
                            redirectUrl = redirectNode.GetAttributeValue("href", string.Empty);
                        }
                        var redirectRes = ParsePage(redirectUrl);

                        if(redirectRes != null && !redirectRes.HasError && redirectRes.Result != null)
                        {
                            result.Children = ((NestedContent)redirectRes.Result).Children;
                            return new ParseResult(result);
                        }
                    }

                    //else, parse this page
                    var seasons = maindiv.SelectNodes("descendant::div[contains(concat(' ', @class, ' '), ' su-spoiler ')]");
                    bool oldmode = false;
                    if (seasons == null || seasons.Count == 0)
                    {
                        seasons = maindiv.SelectNodes("descendant::li");
                        oldmode = true;
                    }

                    if (seasons == null || seasons.Count == 0)
                    {
                        //FILM
                        NestBlock nb = new NestBlock();
                        nb.Title = "Links";
                        var links = maindiv.SelectNodes("descendant::a");
                        foreach (var item in links)
                        {
                            ContentNestBlock cnb = new ContentNestBlock();
                            cnb.Title = ParsingHelpers.ConvertHtml(item.InnerText).Trim();
                            cnb.Links.Add(new Link("", item.GetAttributeValue("href", String.Empty)));
                            if (!string.IsNullOrWhiteSpace(cnb.Title) && !cnb.Links[0].Url.ToLower().Contains("imdb"))
                                nb.Children.Add(cnb);
                        }
                        if (nb.Children.Count > 0)
                            result.Children.Add(nb);
                    }
                    else
                    {
                        //SERIE
                        foreach (var item in seasons)
                        {
                            if (oldmode && item.SelectSingleNode("descendant::li") != null)
                                continue;

                            string titleXpath = oldmode ? "child::span/descendant::a" : "child::div[contains(@class, 'su-spoiler-title')]";
                            string linkblockXpath = oldmode ? "child::div" : "child::div[contains(@class, 'su-spoiler-content')]";

                            String seasontitle = item.SelectSingleNode(titleXpath).InnerText;
                            NestBlock nb = new NestBlock();
                            nb.Title = seasontitle.Trim();

                            var linkblock = item.SelectSingleNode(linkblockXpath);

                            List<HtmlNode> links = new List<HtmlNode>();
                            String desc = string.Empty;
                            for (int linkline = 0; linkline < linkblock.ChildNodes.Count; linkline++)
                            {
                                var curnode = linkblock.ChildNodes[linkline];

                                if (curnode.OriginalName.Equals("br"))
                                {
                                    if (links.Count > 0)
                                    {
                                        ContentNestBlock cnb = new ContentNestBlock();
                                        String eptitle = ParsingHelpers.ConvertHtml(desc).Trim();
                                        if (eptitle.Contains("|"))
                                            eptitle = eptitle.Substring(0, eptitle.IndexOf("|"));
                                        cnb.Title = eptitle;

                                        foreach (var link in links)
                                        {
                                            Link l = new Link("", link.GetAttributeValue("href", string.Empty));
                                            cnb.Links.Add(l);
                                        }
                                        nb.Children.Add(cnb);
                                    }
                                    desc = string.Empty;
                                    links.Clear();
                                    continue;
                                }

                                if (curnode.OriginalName.Equals("a"))
                                    links.Add(curnode);
                                desc = desc + curnode.InnerText;
                            }
                            result.Children.Add(nb);
                        }
                    }
                    
                    //End Parsing
                }
                return new ParseResult(result);
            }
            catch (Exception ex)
            {
                string x = ex.Message;
                return new ParseResult(x);
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
