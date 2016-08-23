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

namespace EDFPlugin.EraDeiPesci
{
    class EDPParser
    {
        private string pluginID;

        public EDPParser(string pluginID)
        {
            this.pluginID = pluginID;
        }


        public SearchResult GetShowList()
        {
            string qresult = null;
            WebResponse response = null;
            StreamReader reader = null;

            List<Bookmark> result = new List<Bookmark>();

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Constants.EraDeiPesciHomepageUrl);
                request.Method = "GET";
                response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                qresult = reader.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(qresult);

                var divs_shows = doc.DocumentNode.SelectSingleNode("//div[@id='Text1']");
                var list_shows = divs_shows.SelectSingleNode("descendant-or-self::div[@class='widget-content']");
               

                if (list_shows != null)
                {

                    //appiattimento dell'albero per tentare di dare una struttura a questa porcheria
                    List<HtmlNode> flattened = new List<HtmlNode>();
                    if (list_shows != null)
                        ExtractAnchors(list_shows, flattened);

                    //comincia il parsing, stai per entrare in una valle di lacrime
                    foreach (var item in flattened)
                    {
                        string link = item.GetAttributeValue("href", string.Empty);
                        string titile = CleanupTitle(item.InnerText);

                        if (!string.IsNullOrWhiteSpace(link) && !string.IsNullOrWhiteSpace(titile) && link.ToLower().Contains("eradeipesci") )
                            result.Add(new Bookmark(pluginID,titile, link));
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


        public SearchResult PerformSearch(string searchTerm)
        {
            string searchurl = Constants.EraDeiPesciSearchUrl.Replace("$searchterm$", WebUtility.UrlEncode(searchTerm));
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

                var articles = doc.DocumentNode.SelectNodes("//section[@role='main']/descendant::article");
                if (articles == null || articles.Count == 0)
                    return null;

                foreach (var article in articles)
                {
                    var header = article.SelectSingleNode("descendant-or-self::header[@class='post-header']");
                    if (header == null)
                        continue;
                    var link = header.SelectSingleNode("descendant-or-self::a");
                    if (link == null)
                        continue;

                    Bookmark bm = new Bookmark(pluginID, WebUtility.HtmlDecode(link.InnerText), link.GetAttributeValue("href", string.Empty));
                    if (!bm.Url.Equals(Constants.EraDeiPesciListaAnimeUrl) && !bm.Url.Equals(Constants.EraDeiPesciListaSerieUrl))
                    res.Add(bm);
                }

                var nextres = doc.DocumentNode.SelectSingleNode("descendant-or-self::div[@class='nav-previous']");
                string nextpage = string.Empty;
                if (nextres != null)
                {
                    nextres = nextres.SelectSingleNode("descendant-or-self::a");
                    if (nextres != null)
                        nextpage = nextres.GetAttributeValue("href", string.Empty);
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
            

            List<HtmlNode> description = new List<HtmlNode>();

            url = url.Replace("eradeipesci.net", "eradeipesci.altervista.org");  //fix per iframe

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

                var maindiv = doc.DocumentNode.SelectSingleNode("//section[@role='main']/descendant::article[contains(@class, 'post')]");
                //var maindiv = doc.DocumentNode.SelectSingleNode("//div[@id='content']");
                if (maindiv != null)
                {
                    //cleanup
                    var meta = maindiv.SelectSingleNode("descendant::section[@class='post-meta']");
                    if (meta != null)
                        meta.Remove();
                    var commentBox = maindiv.SelectSingleNode("descendant::section[@class='comment-box']");
                    if (commentBox != null)
                        commentBox.Remove();
                    var footer = maindiv.SelectSingleNode("descendant::footer");
                    if (footer != null)
                        footer.Remove();

                    
                    //appiattimento dell'albero per tentare di dare una struttura a questa porcheria
                    List<HtmlNode> flattened = new List<HtmlNode>();
                    Flatten(maindiv, flattened);

                    //comincia il parsing, stai per entrare in una valle di lacrime
                    result.CoverImageUrl = ExtractCoverImage(flattened);
                    result.Description = ExtractDescription(flattened);
                    var seasons = ExtractSeasons(flattened);
                    if (seasons != null && seasons.Count > 0)
                        result.Children.AddRange(seasons);
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

        private List<NestBlock> ExtractSeasons(List<HtmlNode> flattened)
        {
            
            List<int> SeasonStarts = new List<int>();
            List<int> SeasonEnds = new List<int>();
            for (int i = 0; i < flattened.Count; i++)
            {
                HtmlNode item = flattened[i];
                if (IsSeasonHeader(item))
                {
                    SeasonStarts.Add(i);
                    if (SeasonStarts.Count > 1)
                        SeasonEnds.Add(SeasonStarts.Last() - 1);
                }
            }
            SeasonEnds.Add(flattened.Count);

            List<NestBlock> seasons = new List<NestBlock>();
            for (int i = 0; i < SeasonStarts.Count; i++)
            {
                var season = ExtractSeason(flattened.GetRange(SeasonStarts[i], SeasonEnds[i] - SeasonStarts[i]));
                if (season != null && season.Children.Count > 0)
                    seasons.Add(season);
            }

            return seasons;
        }

        private NestBlock ExtractSeason(List<HtmlNode> range)
        {
            NestBlock currentSeason = null;
            currentSeason = new NestBlock() { Title = CleanupTitle(range[0].InnerText) };
            

            for (int i = 1; i < range.Count; i++)
            {
                bool titleExtracted = false;
                List<HtmlNode> titleNodes = new List<HtmlNode>();

                var item = range[i];

                //skip al primo set di link o terminazione
                while (!item.OriginalName.Equals("a"))
                {
                    titleNodes.Add(item);
                    if (item.OriginalName.Equals("br"))
                        titleNodes.Clear();
                    i++;
                    if (i >= range.Count)
                        return currentSeason;
                    item = range[i];
                }

                //parse degli episodi 
                ContentNestBlock eb = new ContentNestBlock();
                string episodeTitle = Nodes2Text(titleNodes, false);

                //scorro blocco di link
                while (!item.OriginalName.Equals("br"))
                {
                    //se è un link aggiungo al blocco episodio
                    if (item.OriginalName.Equals("a"))
                    {
                        Link link = new Link(item.InnerText, item.GetAttributeValue("href", string.Empty));
                        if (!string.IsNullOrEmpty(link.Url))
                        {
                            eb.Links.Add(link);
                            if (!titleExtracted)
                            {
                                //imposto titolo episodio
                                if (episodeTitle.Length <= 5)
                                    episodeTitle += (" " + item.InnerText);
                                eb.Title = CleanupTitle(episodeTitle);
                                titleExtracted = true;
                            }
                        }
                    }
                    //prossimo nodo
                    i++;
                    if (i < range.Count)
                        item = range[i];
                    else
                        break;
                }
                currentSeason.Children.Add(eb);
            }
            
            return currentSeason;
        }

        private string ExtractCoverImage(List<HtmlNode> flattened)
        {
            for (int i = 0; i < flattened.Count; i++)
            {
                var item = flattened[i];
                //building cover image
                if (item.OriginalName.Equals("a"))
                {
                    string cover = ContainsImage(item);
                    if (!string.IsNullOrEmpty(cover))
                    {
                        return cover;
                    }
                }
            }
            return string.Empty;
        }

        private string ExtractDescription(List<HtmlNode> flattened)
        {
            string description = "<head> <meta http-equiv='Content-Type' content='text/html;charset=UTF-8'> </head> <body>";
            for (int i = 0; i < flattened.Count; i++)
            {
                var item = flattened[i];
                //building description
                if (!item.OriginalName.Equals("a") && !IsSeasonHeader(item))
                {
                    description += item.InnerHtml;
                }

                if(IsSeasonHeader(item))
                    break;
            }
            description += "</body>";
            return ParsingHelpers.ConvertHtml(description).Trim();
        }

        private void Flatten(HtmlNode input, List<HtmlNode> output)
        {
            foreach (var item in input.ChildNodes)
            {
                if (IsWhitelistedNode(item))
                    output.Add(item);
                else
                {
                    if (!item.GetAttributeValue("class", string.Empty).Equals("social-ring"))
                    {
                        if (item.OriginalName.Equals("div"))
                            output.Add(HtmlNode.CreateNode("<br />"));
                        Flatten(item, output);
                    }
                }
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

        private bool IsWhitelistedNode(HtmlNode node)
        {
            return (
                node.OriginalName.Equals("br") ||
                node.OriginalName.Equals("b") ||
                node.OriginalName.Equals("strong") ||
                node.OriginalName.Equals("a") ||
                node.OriginalName.Equals("#text")
                );
        }

        private string ContainsImage(HtmlNode node)
        {
            var img = node.SelectSingleNode("descendant-or-self::img");
            if (img != null)
            {
                string res = WebUtility.UrlDecode(img.GetAttributeValue("src", string.Empty));
                int lio = res.LastIndexOf("http", StringComparison.CurrentCultureIgnoreCase);
                return res.Substring(lio, res.Length - lio);
            }
                
            return string.Empty;
        }

        private bool IsSeasonHeader(HtmlNode node)
        {
            //return ((node.OriginalName.Equals("strong") || node.OriginalName.Equals("b")) && (node.InnerText.ToLower().Contains("stagione") || node.InnerText.ToLower().Contains("link")));
            return (node.InnerText.ToLower().Contains("stagione") || node.InnerText.ToLower().Contains("link"));
        }

        private string Nodes2Text(IEnumerable<HtmlNode> nodes, bool preserveLineBreaks)
        {
            string s = "";
            foreach (var item in nodes)
            {
                if (item.OriginalName.Equals("br") && preserveLineBreaks)
                    s += "\n";
                else if(item.OriginalName.Equals("#text"))
                    s+= item.InnerText;
            }
            return CleanupTitle(s);
        }

        private string CleanupTitle(string title)
        {
            title = title.Trim();
            title = title.Replace("\n", " ");
            while (title.EndsWith(" ") || title.EndsWith("-") || title.EndsWith("_"))
                title = title.Substring(0, title.Length - 1);
            return WebUtility.HtmlDecode(title);
        }
    }
}
