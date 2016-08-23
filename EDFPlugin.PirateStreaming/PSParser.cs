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
using System.Text.RegularExpressions;

namespace EDFPlugin.PirateStreaming
{
    class PSParser
    {
        public enum ContentType {Movies, Shows}

        private string pluginID;

        public PSParser(string pluginID)
        {
            this.pluginID = pluginID;
        }

        public List<Bookmark> GetShowList()
        {
            string qresult = null;
            WebResponse response = null;
            StreamReader reader = null;

            List<Bookmark> result = new List<Bookmark>();

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Constants.PirateStreamingHomepageUrl);
                request.Method = "GET";
                response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                qresult = reader.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(qresult);

                var divs_shows = doc.DocumentNode.SelectSingleNode("//div[@id='Text1']");
                var list_shows = divs_shows.SelectSingleNode("descendant-or-self::div[@class='widget-content']");
                //var divs_anime = doc.DocumentNode.SelectSingleNode("//aside[@id='linkcat-46']");
                //var list_anime = divs_anime.SelectSingleNode("descendant-or-self::ul");

                

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
                return result;
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


        public SearchResult PerformSearch(string searchTerm, ContentType type)
        {
            string searchurl = Constants.PirateStreamingSearchUrl.Replace("$searchterm$", searchTerm.Replace(" ", "+"));
            return GetResultPage(searchurl, type);
        }


        public SearchResult GetResultPage(string url, ContentType type)
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

                var articles = doc.DocumentNode.SelectNodes("//div[contains(@class, 'ribbon')]");
                if (articles == null || articles.Count == 0)
                    return null;

                HtmlNode seriesRibbon = null, filmRibbon = null, filmTable = null, seriesTable = null;
                var seriesRibbonCandidates = from ar in articles where ar.InnerText.ToLower().Contains("serie tv") select ar;
                if (seriesRibbonCandidates != null && seriesRibbonCandidates.Count() > 0)
                    seriesRibbon = seriesRibbonCandidates.First();

                var filmRibbonCandidates = from ar in articles where ar.InnerText.ToLower().Contains("film") select ar;
                if (filmRibbonCandidates != null && filmRibbonCandidates.Count() > 0)
                    filmRibbon = filmRibbonCandidates.First();

                if(filmRibbon != null)
                    filmTable = filmRibbon.SelectSingleNode("following::table");
                if(seriesRibbon != null)
                    seriesTable = seriesRibbon.SelectSingleNode("following::table");

                if (type == ContentType.Movies && filmTable == seriesTable)
                    return null; //nessun film trovato, solo serie

                var table = type == ContentType.Movies ? filmTable : seriesTable;

                if (table != null)
                    res.AddRange(ExtractTitlesFromPSTable(table));
                else
                    return null;

                HtmlNode filmNextRes = null, seriesNextRes = null;
                if(filmTable != null)
                    filmNextRes = filmTable.SelectSingleNode("following::ul[@id='paginazione']");
                if (seriesTable != null)
                    seriesNextRes = seriesTable.SelectSingleNode("following::ul[@id='paginazione']");
                
                string nextpage = string.Empty;

                if ((type == ContentType.Movies && filmNextRes != null && filmNextRes != seriesNextRes) || type == ContentType.Shows && seriesNextRes != null)
                {
                    var nextres = type == ContentType.Movies ? filmNextRes : seriesNextRes;
                    if (nextres != null && nextres.InnerText.ToLower().Contains("ultima pagina"))
                    {
                        var aNodes = nextres.SelectNodes("descendant::a");

                        //finding the gap
                        HtmlNode nextA = null;
                        for (int i = 0; i < aNodes.Count; i++ )
                        {
                            var item = aNodes[i];
                            if (item.InnerText.ToLower().Contains("prima pagina"))
                                continue;
                            else if (i == 0)
                            {
                                nextA = i == aNodes.Count - 1 ? aNodes[i] : aNodes[i + 1];
                                break;
                            }

                            int index = -1;
                            if (int.TryParse(item.InnerText.Trim(), out index))
                            {
                                if (i == aNodes.Count - 1)
                                    break;

                                if(aNodes[i + 1].InnerText.ToLower().Contains("ultima pagina")){
                                    nextA = aNodes[i+1];
                                    break;
                                }

                                int nextIndex = -1;
                                if (int.TryParse(aNodes[i + 1].InnerText.Trim(), out nextIndex))
                                {
                                    if (index + 2 == nextIndex)
                                    {
                                        nextA = i == aNodes.Count - 2 ? aNodes[i+1] : aNodes[i + 2];
                                        break;
                                    }
                                }
                            }
                        }

                        if (nextA != null)
                        {
                                nextpage = nextA.GetAttributeValue("href", string.Empty);
                                if (!string.IsNullOrWhiteSpace(nextpage))
                                    nextpage = GetCurrentPageBase(url) + nextpage;
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

        private string GetCurrentPageBase(string url) {
            if (url.Contains("?"))
                return (url.Substring(0, url.IndexOf("?")));
            else
                return url;
        }

        private IEnumerable<Bookmark> ExtractTitlesFromPSTable(HtmlNode table)
        {
            var res = new List<Bookmark>();
            var cols = table.SelectNodes("descendant-or-self::div[contains(@class, 'featuredItem')]");
                if (cols == null)
                    return null;
                
                foreach (var article in cols)
                {
                    var header = article.SelectSingleNode("descendant-or-self::div[contains(@class, 'featuredText')]");
                    if (header == null)
                        continue;
                    var link = header.SelectSingleNode("descendant-or-self::a");
                    if (link == null)
                        continue;

                    Bookmark bm = new Bookmark(pluginID, System.Net.WebUtility.HtmlDecode(link.InnerText), link.GetAttributeValue("href", string.Empty));
                    if(!string.IsNullOrWhiteSpace(bm.Name) && !string.IsNullOrWhiteSpace(bm.Url))
                        res.Add(bm);
                }
                return res;
        }


        public ParseResult ParseMoviePage(string url)
        {
            string qresult = null;
            WebResponse response = null;
            StreamReader reader = null;

            HtmlContent result = new HtmlContent();

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

                var maindiv = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'featuredContent')]");
                if (maindiv != null)
                {

                    //appiattimento dell'albero per tentare di dare una struttura a questa porcheria
                    List<HtmlNode> flattened = new List<HtmlNode>();
                    Flatten(maindiv, flattened);

                    //comincia il parsing, stai per entrare in una valle di lacrime
                    result.CoverImageUrl = ExtractCoverImage(flattened);
                    result.Description = ExtractDescription(flattened);

                    var pnodes = maindiv.SelectNodes("descendant::p");
                    HtmlNode mainp = null;
                    foreach (var p in pnodes)
                    {
                        var plinks = p.SelectNodes("descendant::a[not(contains(href, 'piratestreamingdownloader'))]");
                        var mainplinks = mainp != null ? mainp.SelectNodes("descendant::a[not(contains(href, 'piratestreamingdownloader'))]") : null;

                        if (mainp == null && plinks != null || (plinks!= null && plinks.Count > mainplinks.Count))
                            mainp = p;
                    }
                    if (mainp.SelectNodes("descendant::a[not(contains(href, 'piratestreamingdownloader'))]").Count > 0)
                        result.Content = mainp.InnerHtml.Replace("target=\"_blank\"", "target=\"_self\"");
                    else
                        result.Content = "Nessun link trovato";
                }

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

        public ParseResult ParseShowPage(string url)
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

                var maindiv = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'featuredContent')]");
                if (maindiv != null)
                {
                    
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
                if (season != null)
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
                if(!string.IsNullOrWhiteSpace(eb.Title))
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
                if (item.OriginalName.Equals("div") && item.HasChildNodes && item.FirstChild.OriginalName.Equals("img"))
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
            string convDesc = ParsingHelpers.ConvertHtml(description).Trim();
            return Regex.Replace(convDesc, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);

        }

        private void Flatten(HtmlNode input, List<HtmlNode> output)
        {
            foreach (var item in input.ChildNodes)
            {
                if (item.OriginalName.Equals("div") && item.GetAttributeValue("class", string.Empty).Equals("ribbon"))
                    return;

                if (IsWhitelistedNode(item))
                    output.Add(item);
                else
                {
                    if (item.OriginalName.Equals("div"))
                        output.Add(HtmlNode.CreateNode("<br />"));
                    Flatten(item, output);
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
                node.OriginalName.Equals("div") && node.HasChildNodes && node.FirstChild.OriginalName.Equals("img") ||
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
                return img.GetAttributeValue("src", string.Empty);
            return string.Empty;
        }

        private bool IsSeasonHeader(HtmlNode node)
        {
            return ((node.OriginalName.Equals("strong") || node.OriginalName.Equals("b")) && (node.InnerText.ToLower().Contains("stagione") || node.InnerText.ToLower().Contains("link")));
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
            return System.Net.WebUtility.HtmlDecode(title);
        }
    }
}
