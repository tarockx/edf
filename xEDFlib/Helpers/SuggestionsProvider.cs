
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using System.Text;
using System.Threading.Tasks;

namespace xEDFlib.Helpers
{
    public class GoogleSuggestionsProvider
    {
        private static string queybase = "http://suggestqueries.google.com/complete/search?client=firefox&hl=it&q=$";

        public static List<String> DoSearch(string searchTerm)
        {
            List<string> l = new List<string>();
            string query = queybase.Replace("$", searchTerm);

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36";


                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string result = reader.ReadToEnd();
                StringReader sreader = new StringReader(result);

                JsonSerializer s = new JsonSerializer();
                var des = s.Deserialize(new JsonTextReader(sreader));

                //JavaScriptSerializer s = new JavaScriptSerializer();
                //var des = s.DeserializeObject(result);
                var results = des as Newtonsoft.Json.Linq.JArray;
                foreach (var item in results.ElementAt(1))
                {
                    l.Add(item.ToString());
                }
                return l;
                
            }
            catch (Exception)
            {
                return null;
            }

        }

        public static Task<List<string>> DoSearchAsync(string searchTerm)
        {
            var task = Task.Factory.StartNew(() => DoSearch(searchTerm));
            return task;
        }
    }
}
