using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace EraDeiFessi.WebFormsApp
{
    /// <summary>
    /// Summary description for Suggestions
    /// </summary>
    public class Suggestions : IHttpHandler
    {
        private static string queybase = "http://suggestqueries.google.com/complete/search?client=firefox&hl=it&q=$";

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string searchTerm = context.Request.Params["query"];

            try
            {
                string query = queybase.Replace("$", searchTerm);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36";


                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string result = reader.ReadToEnd();

                JavaScriptSerializer s = new JavaScriptSerializer();
                var des = s.DeserializeObject(result);
                var results = (des as object[])[1] as object[];

                string serializedResult = s.Serialize(results);

                context.Response.Write("[\n");

                for(int i = 0; i < results.Length; i++)
                {
                    string item = results[i] as string;
                    context.Response.Write("{\"value\": \"" + item + "\"}" + (i == results.Length - 1 ? "\n" : ",\n"));
                }

                context.Response.Write("\n]");
            }
            catch (Exception)
            {
                //return null;
            }

            //context.Response.Write("Hello World");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}