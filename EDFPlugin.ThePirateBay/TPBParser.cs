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

namespace EDFPlugin.ThePirateBay
{
    class TPBParser
    {
        private string pluginID;

        public TPBParser(string pluginID)
        {
            this.pluginID = pluginID;
        }

        private void RetrieveTPBProxy()
        {
            Constants.TPBBaseUrl = "https://pirateproxy.pl";
            return;
        }

        public SearchResult PerformSearch(string searchTerm)
        {
            if (string.IsNullOrEmpty(Constants.TPBBaseUrl))
                RetrieveTPBProxy();

            if (!string.IsNullOrEmpty(Constants.TPBBaseUrl))
            {
                string searchurl = Constants.TPBSearchUrl.Replace("$proxy$", Constants.TPBBaseUrl).Replace("$searchterm$", Uri.EscapeDataString(searchTerm));
                return GetResultPage(searchurl);
            }
            else
            {
                return new SearchResult() { Error = "Impossibile raggiungere un proxy valido per accedere a ThePirateBay" };
            }
        }

        public SearchResult GetResultPage(string url)
        {
            string qresult = null;
            List<Bookmark> res = new List<Bookmark>();

            try
            {
                RequestMaker maker = new RequestMaker() { Timeout = 8000 };
                qresult = maker.MakeRequest(url);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(qresult);

                var rows = doc.DocumentNode.SelectNodes("//table[@id='searchResult']/tr");
                if (rows == null)
                    return null;

                foreach (var row in rows)
                {
                    var tds = row.SelectNodes("td");

                    var categoryNode = tds[0].SelectSingleNode("(descendant::a)[2]");
                    if (categoryNode == null)
                        continue;

                    var linkNode = tds[1].SelectSingleNode("descendant::div[@class='detName']/a");
                    if (linkNode == null)
                        continue;

                    var subtitleNode = tds[1].SelectSingleNode("descendant::font[@class='detDesc']");
                    if (subtitleNode == null)
                        continue;

                    var seedersNode = tds[2];
                    var leechersNode = tds[3];

                    var link = Constants.TPBBaseUrl + (linkNode.GetAttributeValue("href", string.Empty)).Trim();
                    var title = linkNode.InnerText.Trim();

                    Bookmark bm = new Bookmark(pluginID, title, link);

                    string category = categoryNode.InnerText.Trim();
                    string subtitle = ParsingHelpers.ConvertHtml(subtitleNode.InnerText.Trim());
                    string seeders = seedersNode == null ? "N/A" : seedersNode.InnerText;
                    string leechers = leechersNode == null ? "N/A" : leechersNode.InnerText;

                    bm.Description = "Category: " + category + " - seed: " + seeders + " - leechs: " + leechers + "description: " + subtitle;

                    res.Add(bm);

                }

                var nextPageNode = doc.DocumentNode.SelectSingleNode("//div[@id='content']/descendant::img[@alt='Next']/..");
                string nextPageLink = null;
                if (nextPageNode != null)
                {
                    nextPageLink = Constants.TPBBaseUrl + nextPageNode.GetAttributeValue("href", string.Empty);
                }

                return new SearchResult() { Result = res, NextPageUrl = nextPageLink };
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

            TorrentContent result = new TorrentContent();

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

                var title = doc.DocumentNode.SelectSingleNode("//div[@id='title']");
                var magnet = doc.DocumentNode.SelectSingleNode("//div[@class='download']/a");

                if (magnet != null)
                    result.MagnetURI = magnet.GetAttributeValue("href", string.Empty);
                else
                    return null;

                foreach (var item in doc.DocumentNode.SelectNodes("//div[@class='download']"))
                {
                    item.Remove();
                }


                var description = doc.DocumentNode.SelectSingleNode("//div[@class='nfo']");
                description.SetAttributeValue("style", "white-space:pre-wrap; word-wrap: break-word;");
                result.HtmlDescription = description.OuterHtml;

                //var description = doc.DocumentNode.SelectSingleNode("//div[@id='details']");
                //if (description != null)
                //{
                //    var items = doc.DocumentNode.SelectSingleNode("//body").ChildNodes.ToList();
                //    foreach (var item in items)
                //    {
                //        if (!item.Id.Equals("content"))
                //            item.Remove();
                //        else
                //        {
                //            var subitems = item.SelectSingleNode("descendant::div[@id='main-content']/div").ChildNodes.ToList();
                //            foreach (var subitem in subitems)
                //            {
                //                if (!subitem.Id.Equals("detailsouterframe"))
                //                    subitem.Remove();
                //            }
                //        }
                //    }
                //    result.HtmlDescription = doc.DocumentNode.InnerHtml;
                //}

                result.Title = title == null ? "[no title]" : title.InnerText.Trim();

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
