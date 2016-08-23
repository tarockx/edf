using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Net;
using System.IO;
using WpfControls;

namespace EraDeiFessi.Helpers
{
    public class GoogleSuggestionsProvider : ISuggestionProvider
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

                JavaScriptSerializer s = new JavaScriptSerializer();
                var des = s.DeserializeObject(result);
                var results = des as object[];
                foreach (var item in (results[1] as object[]))
                {
                    l.Add(item as string);
                }
                return l;
            }
            catch (Exception)
            {
                return null;
            }
            
        }

        
        public System.Collections.IEnumerable GetSuggestions(string filter)
        {
            return DoSearch(filter);
        }
    }
}
