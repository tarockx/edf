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

namespace EDFPlugin.IlCorsaroNero
{
    class ICNParser
    {
        private string pluginID;

        public ICNParser(string pluginID)
        {
            this.pluginID = pluginID;
        }

        public SearchResult PerformSearch(string searchTerm)
        {
            string searchurl = Constants.ICNSearchUrl.Replace("$searchterm$", searchTerm.Replace(' ', '+'));
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

                var tabheader = doc.DocumentNode.SelectNodes("//tr[contains(@class, 'bordo')]").ElementAt(0);
                if (tabheader == null || !tabheader.InnerText.Contains("Cat") || !tabheader.InnerText.Contains("Name") || !tabheader.InnerText.Contains("Size") || !tabheader.InnerText.Contains("Azione"))
                    return null;

                var rows = tabheader.SelectNodes("following-sibling::tr");
                if (rows == null)
                    return null;

                foreach (var row in rows)
                {

                    var categoryNode = row.SelectSingleNode("descendant::a[contains(@href, '/cat/')]");
                    if (categoryNode == null)
                        continue;

                    var linkNode = row.SelectSingleNode("descendant::a[contains(@class, 'tab')]");
                    if (linkNode == null)
                        continue;

                    var sizeNode = row.SelectSingleNode("descendant::font[@color='#FF6600']");
                    if (sizeNode == null)
                        continue;

                    var seedersNode = row.SelectSingleNode("descendant::font[@color='#00CC00']");
                    var leechersNode = row.SelectSingleNode("descendant::font[@color='#0066CC']");

                    var link = (linkNode.GetAttributeValue("href", string.Empty)).Trim();
                    var title = linkNode.InnerText.Trim();

                    Bookmark bm = new Bookmark(pluginID, title, link);

                    string category = categoryNode.InnerText;
                    string size = sizeNode.InnerText;
                    string seeders = seedersNode == null ? "N/A" : seedersNode.InnerText;
                    string leechers = leechersNode == null ? "N/A" : leechersNode.InnerText;

                    bm.Description = "Category: " + category + " - size: " + size + " - seed: " + seeders + " - leechs: " + leechers;

                    res.Add(bm);
                    
                }

                return new SearchResult() { Result = res, NextPageUrl = string.Empty };
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

                var title = doc.DocumentNode.SelectSingleNode("//div[@id='body']/descendant::center/b/font[@color='#FFFFFF']");
                var magnet = doc.DocumentNode.SelectSingleNode("//a[contains(@href,'magnet') and contains(@class,'forbtn')]");

                if (magnet != null)
                    result.MagnetURI = magnet.GetAttributeValue("href", string.Empty);
                else
                    return null;

                    var descHeader = doc.DocumentNode.SelectSingleNode("//td[text() = 'Descrizione']");
                    if(descHeader != null)
                    {
                        var description = descHeader.SelectSingleNode("following-sibling::td[1]");
                        if (description != null)
                            result.HtmlDescription = description.InnerHtml.Replace("width:390px;", "");
                    }

                result.Title = title == null ? "[no title]" : title.InnerText;
                
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
