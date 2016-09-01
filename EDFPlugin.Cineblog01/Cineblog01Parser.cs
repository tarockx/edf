using HtmlAgilityPack;
using libEraDeiFessi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace EDFPlugin.Cineblog01
{
    class Cineblog01Parser
    {
        public enum SearchSection {Movies, Cartoons, Series}
        private string pluginID;

        public Cineblog01Parser(string pID) { pluginID = pID; }


        public SearchResult PerformSearch(string searchTerm, SearchSection searchSection)
        {
            List<Bookmark> res = new List<Bookmark>();
            string[] terms = searchTerm.Split(new Char[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                RequestMaker requestMaker = new RequestMaker();

                string listUrl = null;
                if (searchSection == SearchSection.Movies)
                {
                    listUrl = Constants.Cineblog01ListaFilmUrl;
                }
                else if (searchSection == SearchSection.Series)
                {
                    listUrl = Constants.Cineblog01ListaSerieUrl;
                }

                string result = requestMaker.MakeRequest(listUrl);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(result);

                HtmlNodeCollection links = null;

                if (searchSection == SearchSection.Movies)
                {
                    var tables = doc.DocumentNode.SelectNodes("descendant-or-self::td");
                    HtmlNode table = null;
                    foreach (var t in tables)
                    {
                        if (table == null || table.InnerHtml.Length < t.InnerHtml.Length)
                        {
                            table = t;
                        }
                    }

                    links = table.SelectNodes("descendant::a");
                }
                else if (searchSection == SearchSection.Series)
                {
                    links = doc.DocumentNode.SelectNodes("descendant::ul/li/a");
                }


                foreach (var link in links)
                {
                    string linktext = link.InnerText.Trim();
                    bool match = true;
                    foreach (var term in terms)
                    {
                        if (!linktext.ToLower().Contains(term.Trim().ToLower()))
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        Bookmark bm = new Bookmark(pluginID, ParsingHelpers.ConvertHtml(link.InnerHtml), link.GetAttributeValue("href", string.Empty));
                        res.Add(bm);
                    }

                }


                return new SearchResult() { Result = res, NextPageUrl = null };
            }
            catch
            {
                return new SearchResult() { Error = "Errore nella ricerca: la pagina di Cineblog non è raggiungibile o ha subito una modifica che necessita l'aggiornamento del plugin" };
            }
        }


        public SearchResult GetResultPage(string uri)
        {
            string qresult = null;
            WebResponse response = null;
            StreamReader reader = null;
            List<Bookmark> res = new List<Bookmark>();

            try
            {
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.UserAgent = "Lynx/2.8.8dev.3 libwww-FM/2.14 SSL-MM/1.4.1";
                request.Method = "GET";
                response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                qresult = reader.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(qresult);

                var articles = doc.DocumentNode.SelectNodes("descendant-or-self::div[contains(@class,'filmbox')]/div[contains(@class,'span8')]/a/h1");
                if (articles == null || articles.Count == 0)
                    return null;

                foreach (var article in articles)
                {
                    var link = article.ParentNode;
                    if (link == null || link.Name != "a")
                        continue;

                    Bookmark bm = new Bookmark(pluginID, WebUtility.HtmlDecode(article.InnerText), link.GetAttributeValue("href", string.Empty));
                    res.Add(bm);
                }

                var nextres = doc.DocumentNode.SelectSingleNode("descendant-or-self::div[@id='wp_page_numbers']");
                string nextpage = string.Empty;
                if (nextres != null)
                {
                    nextres = nextres.SelectSingleNode("descendant-or-self::li[@class='active_page']");
                    if (nextres != null)
                    {
                        nextres = nextres.SelectSingleNode("following-sibling::li");
                        if (nextres != null && nextres.OriginalName.Equals("li"))
                        {
                            nextres = nextres.SelectSingleNode("descendant-or-self::a");
                            {
                                if (nextres != null)
                                    nextpage = nextres.GetAttributeValue("href", string.Empty);
                            }
                        }

                    }
                }

                return new SearchResult() { Result = res, NextPageUrl = nextpage };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        public SearchResult GetMovieList()
        {
            string qresult = null;
            WebResponse response = null;
            StreamReader reader = null;

            List<Bookmark> result = new List<Bookmark>();

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Constants.Cineblog01ListaFilmUrl);
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36";
                request.Method = "GET";
                response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                qresult = reader.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(qresult);

                var tables = doc.DocumentNode.SelectNodes("//td");
                HtmlNode linkstable = null;
                foreach (var item in tables)
                {
                    var curtable_links = item.SelectNodes("descendant-or-self::a");
                    if (curtable_links == null)
                        continue;

                    int linkcount = curtable_links.Count;
                    if (linkstable == null || linkstable.SelectNodes("descendant-or-self::a").Count < linkcount)
                        linkstable = item;
                }

                if (linkstable != null)
                {

                    //appiattimento dell'albero per tentare di dare una struttura a questa porcheria
                    List<HtmlNode> flattened = new List<HtmlNode>();
                    ExtractAnchors(linkstable, flattened);

                    //comincia il parsing, stai per entrare in una valle di lacrime
                    foreach (var item in flattened)
                    {
                        string link = item.GetAttributeValue("href", string.Empty);
                        string title = WebUtility.HtmlDecode(item.InnerText.Trim());

                        if (!string.IsNullOrWhiteSpace(link) && !string.IsNullOrWhiteSpace(title))
                            result.Add(new Bookmark(pluginID, title, link));
                    }
                }
                return new SearchResult() { Result = result };
            }
            catch (Exception)
            {
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

        public SearchResult GetLatestMovies(string nextPage)
        {
            string qresult = null;
            WebResponse response = null;
            StreamReader reader = null;

            List<Bookmark> result = new List<Bookmark>();

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.IsNullOrEmpty(nextPage) ? Constants.Cineblog01HomepageUrl : nextPage);
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36";
                response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                qresult = reader.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(qresult.Replace("target=\"_self\" <", "target=\"_self\" ><").Replace("<a class=\"<", "<"));

                var content = doc.DocumentNode.SelectSingleNode("//div[@id='content']");
                //var content = doc.DocumentNode.SelectNodes("//div[@id='item']");
                if (content == null)
                    return null;

                //var items = doc.DocumentNode.SelectNodes("//div[@id='item']");
                var items = content.SelectNodes("descendant::div[@id='item']");
                if (items == null)
                    return null;

                foreach (var item in items)
                {
                    var post_title = item.SelectSingleNode("descendant-or-self::div[@id='post-title']");
                    if (post_title == null)
                        continue;

                    var post_link = post_title.SelectSingleNode("descendant-or-self::a");
                    if (post_link == null)
                        continue;

                    string link = post_link.GetAttributeValue("href", string.Empty);
                    string title = WebUtility.HtmlDecode(post_link.InnerText.Trim());

                    if (!string.IsNullOrWhiteSpace(link) && !string.IsNullOrWhiteSpace(title))
                        result.Add(new Bookmark(pluginID, title, link));
                }

                string nextpage = string.Empty;
                var navigator = content.SelectSingleNode("descendant-or-self::div[@id='pagination']/descendant::li[contains(@class, 'active_page')]/following-sibling::li[1]/descendant::a");
                if (navigator != null)
                    nextpage = navigator.GetAttributeValue("href", string.Empty);

                return new SearchResult() { Result = result, NextPageUrl = nextpage };
            }
            catch (Exception ex)
            {
                var x = ex.Message;
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


        public ParseResult ParsePage(string url, string referer = "")
        {
            string qresult = null;
            WebResponse response = null;
            StreamReader reader = null;

            string result = string.Empty;
            string desc = string.Empty;
            string imageurl = string.Empty;

            HtmlContent parseresult = null;

            List<HtmlNode> description = new List<HtmlNode>();

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36";
                
                request.Method = "POST";
                response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                qresult = reader.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(qresult);

                var maindiv = doc.DocumentNode.SelectSingleNode("//div[@class='post_content']");
                if (maindiv == null)
                    throw new Exception("Impossibile eseguire il parsing della pagina. Il layout del sito è stato modificato o il caricamento è fallito");

                //remove comments
                var disq = maindiv.SelectSingleNode("descendant::div[@id='disqus_thread']");
                if (disq != null)
                    disq.Remove();

                //remove ads/twitter
                var twitter = maindiv.SelectSingleNode("descendant::div[contains(@id, 'twitter_button')]");
                if(twitter != null)
                {
                    twitter.Remove();
                }

                //extract cover & description
                try
                {
                    var img = maindiv.SelectSingleNode("descendant-or-self::img");
                    if (img != null)
                    {
                        imageurl = img.GetAttributeValue("src", string.Empty);
                        if(!string.IsNullOrEmpty(imageurl) && imageurl.StartsWith("/"))
                        {
                            imageurl = (Constants.Cineblog01HomepageUrl + imageurl).Replace("//", "/");
                        }
                    }

                    var descp = maindiv.SelectNodes("descendant::p");
                    if (descp != null)
                    {
                        foreach (var item in descp)
                        {
                            if (!item.InnerText.Contains("WP Twitter") && !item.InnerText.Contains("<img") && !item.ParentNode.Name.Equals("td"))
                                desc += (item.InnerText + "<br />");
                        }
                    }

                }
                catch (Exception)
                {
                    //cover and/or description not found, whatever, we'll do without
                }



                var tables = maindiv.SelectNodes("descendant-or-self::table");
                HtmlNode linkstable = null;
                if (tables != null && tables.Count > 0)
                {
                    foreach (var item in tables)
                    {
                        var curtable_links = item.SelectNodes("descendant-or-self::a");
                        if (curtable_links == null)
                            continue;

                        int linkcount = curtable_links.Count;
                        if (linkstable == null || linkstable.SelectNodes("descendant-or-self::a").Count < linkcount)
                            linkstable = item;
                    }

                    if (linkstable != null)
                    {
                        linkstable = linkstable.SelectSingleNode("descendant-or-self::td");

                        var iframe = linkstable.SelectSingleNode("descendant-or-self::iframe");
                        if (iframe != null)
                            iframe.Remove();

                        var hdItems = linkstable.SelectNodes("descendant-or-self::a[contains(@href,'cineblog01hd')]");
                        if (hdItems != null)
                        {
                            foreach (var item in hdItems)
                            {
                                item.Remove();
                            }
                        }

                        var downlaodHD = linkstable.SelectSingleNode("descendant::strong[contains(.,'Download HD:')]");
                        if (downlaodHD != null)
                            downlaodHD.Remove();

                        result = "<html> <head> <meta http-equiv='Content-Type' content='text/html;charset=UTF-8' /> </head> <body> ";
                        result += linkstable.OuterHtml;
                        result += "</body> </html>";
                        result = result.Replace("target=\"_blank\"", "target=\"_self\"");

                        parseresult = new HtmlContent();
                        parseresult.Content = result;

                        desc = "<head> <meta http-equiv='Content-Type' content='text/html;charset=UTF-8'> </head> <body> " + desc + "</body>";
                        parseresult.Description = ParsingHelpers.ConvertHtml(desc).Trim();
                        parseresult.CoverImageUrl = imageurl;
                    }
                }
                return new ParseResult(parseresult);
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

        private void ExtractAnchors(HtmlNode input, List<HtmlNode> output)
        {
            foreach (var item in input.ChildNodes)
            {
                if (item.OriginalName.Equals("a"))
                    output.Add(item);
                else
                    ExtractAnchors(item, output);
            }
        }
    }
}
