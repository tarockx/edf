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

namespace EDFPlugin.RARBG
{
    class RARBGParser
    {
        private string pluginID;
        private CookieAwareWebClient wc = new CookieAwareWebClient();

        public RARBGParser(string pluginID)
        {
            this.pluginID = pluginID;
            //wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246");
        }

        public SearchResult PerformSearch(string searchTerm)
        {
                string searchurl = Constants.RARBGSearchUrl.Replace("$searchterm$", searchTerm.Replace(" ", "+"));
                return GetResultPage(searchurl);
        }

        public SearchResult GetResultPage(string url)
        {
            string qresult = null;
            List<Bookmark> res = new List<Bookmark>();

            try
            {
                wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246";
                wc.Headers[HttpRequestHeader.Accept] = "application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, application/x-shockwave-flash, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*";

                qresult = wc.DownloadString(url);

                //RequestMaker maker = new RequestMaker() { Timeout = 8000 };
                //qresult = maker.MakeRequest(url);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(qresult);

                var rows = doc.DocumentNode.SelectNodes("//tr[@class='lista2']");
                if (rows == null)
                    return null;

                foreach (var row in rows)
                {
                    var tds = row.SelectNodes("td");

                    var linkNode = tds[1].SelectSingleNode("descendant::a");
                    if (linkNode == null)
                        continue;

                    var dateNode = tds[2];
                    if (dateNode == null)
                        continue;

                    var sizeNode = tds[3];
                    if (sizeNode == null)
                        continue;

                    var seedersNode = tds[4];
                    var leechersNode = tds[5];

                    var link = Constants.RARBGBaseUrl + (linkNode.GetAttributeValue("href", string.Empty)).Trim();
                    var title = linkNode.InnerText.Trim();

                    Bookmark bm = new Bookmark(pluginID, title, link);

                    string date = dateNode.InnerText.Trim();
                    string size = sizeNode.InnerText.Trim();
                    string seeders = seedersNode == null ? "N/A" : seedersNode.InnerText;
                    string leechers = leechersNode == null ? "N/A" : leechersNode.InnerText;

                    bm.Description = "Size: " + size + " - seed: " + seeders + " - leechs: " + leechers + " - added: " + date;

                    res.Add(bm);

                }

                var nextPageNode = doc.DocumentNode.SelectSingleNode("//div[@id='pager_links']/descendant::a[@title='next page']");
                string nextPageLink = null;
                if (nextPageNode != null)
                {
                    nextPageLink = Constants.RARBGBaseUrl + nextPageNode.GetAttributeValue("href", string.Empty).Replace("&amp;", "&");
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
            TorrentContent result = new TorrentContent();

            try
            {
                wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246";
                wc.Headers[HttpRequestHeader.Accept] = "application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, application/x-shockwave-flash, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*";

                qresult = wc.DownloadString(url);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(qresult);

                var title = doc.DocumentNode.SelectSingleNode("//h1");
                var magnet = doc.DocumentNode.SelectSingleNode("//a[contains(@href,'magnet:')]");

                if (magnet != null)
                    result.MagnetURI = magnet.GetAttributeValue("href", string.Empty);
                else
                    return null;


                var description = doc.DocumentNode.SelectSingleNode("//td[@id='description']");
                result.HtmlDescription = description.OuterHtml;

                result.Title = title == null ? "[no title]" : title.InnerText.Trim();

                return new ParseResult(result);
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
