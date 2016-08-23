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
using System.Xml;
using System.Text.RegularExpressions;

namespace EDFPlugin.ItaliaSerie
{
    class ISParser
    {
        private string pluginID;
        public enum Section { Film, Series, Anime }

        public ISParser(string pluginID)
        {
            this.pluginID = pluginID;
        }

        public SearchResult PerformSearch(string searchTerm)
        {
            string searchurl = Constants.ItaliaSerieSearchUrl.Replace("$searchterm$", searchTerm.Trim().Replace(" ", "+"));
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

                var articles = doc.DocumentNode.SelectNodes("//ul[@class='recent-posts']/descendant::div[@class='post-content']/descendant::a");
                if (articles == null || articles.Count == 0)
                    return null;

                foreach (var article in articles)
                {
                    var link = article.GetAttributeValue("href", string.Empty);
                    var title = article.InnerText;

                    if (string.IsNullOrWhiteSpace(link) || string.IsNullOrWhiteSpace(title))
                        continue;

                    Bookmark bm = new Bookmark(pluginID, System.Net.WebUtility.HtmlDecode(title), link);
                    res.Add(bm);
                }

                var nextres = doc.DocumentNode.SelectNodes("descendant::div[@class='navigation']/descendant::a[contains(@class, 'next')]");
                string nextpage = string.Empty;
                if (nextres != null && nextres.Count > 0)
                {
                    nextpage = nextres[0].GetAttributeValue("href", string.Empty);
                }


                return new SearchResult() { Result = res, NextPageUrl = nextpage };
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                Console.Write(ex.StackTrace);
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

                var maindiv = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'post-entry')]");
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
                if (!string.IsNullOrWhiteSpace(eb.Title))
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
                if (item.OriginalName.Equals("img") && item.GetAttributeValue("class", "").ToLower().Contains("wp-post-image"))
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

                if (IsSeasonHeader(item))
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
                node.OriginalName.Equals("div") && node.GetAttributeValue("class", "").Contains("su-spoiler-title") ||
                node.OriginalName.Equals("img") && node.GetAttributeValue("class", "").Contains("wp-post-image") ||
                node.OriginalName.Equals("p") && node.InnerText.ToLower().Contains("stagione") ||
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
            return (node.InnerText.Length < 200 && (node.OriginalName.Equals("p")  || node.OriginalName.Equals("div") && node.GetAttributeValue("class", "").Contains("su-spoiler-title")) &&
                node.InnerText.ToLower().Contains("stagione") );
        }

        private string Nodes2Text(IEnumerable<HtmlNode> nodes, bool preserveLineBreaks)
        {
            string s = "";
            foreach (var item in nodes)
            {
                if (item.OriginalName.Equals("br") && preserveLineBreaks)
                    s += "\n";
                else if (item.OriginalName.Equals("#text"))
                    s += item.InnerText;
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
