using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace libEraDeiFessi
{
    public class RequestMaker
    {
        public string UserAgent { get; set; }
        public string Referer { get; set; }
        public int Timeout { get; set; }

        public RequestMaker()
        {
            UserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36";
            Timeout = 0;
        }

        public string MakeRequest(string url, Dictionary<string, string> parameters = null)
        {
            string result = null;
            WebResponse response = null;
            StreamReader reader = null;

            try
            {
                //string url = string.Format(Constants.RealDebridAuthenticationLink, Repo.Settings.RealDebridLogin, Repo.Settings.RealDebridPassword);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.UserAgent = UserAgent;
                if(Timeout != 0)
                    request.Timeout = Timeout;
                request.Referer = Referer;

                response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                result = reader.ReadToEnd();

                return result;
            }
            catch (Exception)
            {
                return string.Empty;
            }


        }
    }
}
