using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using libEraDeiFessi;
using System.Text.RegularExpressions;

namespace EDFPlugin.FilmPerTutti
{
    class Parser
    {
        private string pluginID;

        public Parser(string pluginID)
        {
            this.pluginID = pluginID;
        }

        public SearchResult PerformSearch(string searchTerm)
        {
            string searchurl = Constants.SearchUrl.Replace("$searchterm$", searchTerm.Trim().Replace(" ", "+"));
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

                var articles = doc.DocumentNode.SelectNodes("//ul[@class='posts']/li/a");
                if (articles == null || articles.Count == 0)
                    return null;

                foreach (var article in articles)
                {
                    var link = article.GetAttributeValue("href", string.Empty);
                    var titleNode = article.SelectSingleNode("descendant::div[@class='title']");
                    string title = titleNode != null ? WebUtility.HtmlDecode(titleNode.InnerText) : null;

                    if (string.IsNullOrWhiteSpace(link) || string.IsNullOrWhiteSpace(title))
                        continue;

                    Bookmark bm = new Bookmark(pluginID, System.Net.WebUtility.HtmlDecode(title), link);
                    res.Add(bm);
                }

                var nextres = doc.DocumentNode.SelectSingleNode("//div[@class='navigation']/ul/li/a[contains(., 'successiva')]");
                string nextpage = nextres != null ? nextres.GetAttributeValue("href", string.Empty) : string.Empty;

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

            try
            {
                RequestMaker rm = new RequestMaker();
                qresult = rm.MakeRequest(url);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(qresult);

                var contentdiv = doc.DocumentNode.SelectSingleNode("//section[@id='content']");

                //remove disquss
                contentdiv.SelectSingleNode("descendant::div[@id='disqus_thread']")?.Remove();

                HtmlNode maindiv = null;
                foreach (var subdiv in contentdiv.SelectNodes("child::div[contains(@class, 'pad')]"))
                {
                    if(maindiv == null || maindiv.InnerHtml.Length < subdiv.InnerHtml.Length)
                    {
                        maindiv = subdiv;
                    }
                }

                if (maindiv != null)
                {
                    //appiattimento dell'albero per tentare di dare una struttura a questa porcheria
                    List<HtmlNode> flattened = new List<HtmlNode>();
                    Flatten(maindiv, flattened);

                    ContentMetadata cm = ExtractMetadata(contentdiv);

                    var seasons = ExtractSeasons(flattened);
                    if (seasons != null && seasons.Count > 0)
                    {
                        NestedContent content = new NestedContent();
                        content.Children.AddRange(seasons);
                        content.CoverImageUrl = cm.CoverImageUrl;
                        content.Description = cm.Description;
                        return new ParseResult(content);
                    }
                    else
                    {
                        HtmlContent content = ParseAsMovie(maindiv);
                        content.CoverImageUrl = cm.CoverImageUrl;
                        content.Description = cm.Description;
                        return new ParseResult(content);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return new ParseResult(ex.Message);
            }
        }

        private HtmlContent ParseAsMovie(HtmlNode maindiv)
        {
            HtmlContent content = new HtmlContent();
            string htmlres = "<html><body>\n";

            var paragraphs = maindiv.SelectNodes("descendant::p");
            foreach (var p in paragraphs)
            {
                var linkcount = p.SelectNodes("descendant::a")?.Count;
                var imgcount = p.SelectNodes("descendant::img")?.Count;

                if (linkcount.HasValue && linkcount > 0 && (!imgcount.HasValue || imgcount == 0))
                {
                    htmlres += (p.OuterHtml + "\n");
                }
            }
            htmlres += "</body></html>";

            content.Content = htmlres;
            return content;
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
                                //if (episodeTitle.Length <= 5)
                                //    episodeTitle += (" " + item.InnerText);
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

        private ContentMetadata ExtractMetadata(HtmlNode section)
        {
            ContentMetadata cm = new ContentMetadata();

            //Title
            var title = section.SelectSingleNode("child::h1");
            if(title != null)
            {
                cm.Title = WebUtility.HtmlDecode(title.InnerText);
            }

            var meta = section.SelectSingleNode("descendant::div[@class='meta']");
            if(meta != null) //New layout
            {
                //Cover image
                var img = meta.SelectSingleNode("descendant::img[contains(@class, 'thumbnail']");
                if(img != null)
                {
                    cm.CoverImageUrl = img.GetAttributeValue("src", string.Empty);
                }

                //Description
                var descsub = meta.SelectSingleNode("descendant::div[@class='subtitle' and contains(., 'Trama')]/following-sibling::div[@class='element']");
                if(descsub != null)
                {
                    cm.Description = WebUtility.HtmlDecode(descsub.InnerText);
                }
            }
            else
            {
                //Old style
            }

            return cm;
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
