using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace libRealDebrid4DotNet
{
    public class RDAgent
    {
        private string _authCookie = string.Empty;
        public List<string> Hosters { get; set; }

        public Task<RDLoginResponse> LogMeInAsync(string Username, string Password)
        {
            Task<RDLoginResponse> task = Task.Factory.StartNew(() => LogMeIn(Username, Password));
            return task;
        }

        public RDLoginResponse LogMeIn(string Username, string Password)
        {
            WebResponse response = null;
            StreamReader reader = null;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                return RDLoginResponse.MissingLoginData;

            RDLoginResponse rdresp = RDLoginResponse.OK;

            try
            {
                string url = string.Format(Constants.RealDebridAuthenticationLink, Username, Password);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36";
                request.Timeout = 10000;

                response = request.GetResponse();

                JsonSerializer serializer = new JsonSerializer();
                string resultString = (new StreamReader(request.GetResponse().GetResponseStream())).ReadToEnd();
                Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(resultString);
                                

                //DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings();
                //settings.UseSimpleDictionaryFormat = true;
                //var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, object>), settings);
                //var dictionary = (IDictionary<string, object>)serializer.ReadObject(response.GetResponseStream());

                if(dictionary == null || !dictionary.ContainsKey("error"))
                    return RDLoginResponse.UnknownError;

                string error = dictionary["error"].ToString();


                if (error.Equals("0"))
                {
                    _authCookie = (dictionary["cookie"].ToString()).Replace("auth=", "").Trim().Replace(";", "");
                    return RDLoginResponse.OK;
                }
                else
                {
                    switch (error)
                    {
                        case "1":
                            rdresp = RDLoginResponse.UsernameOrPasswordIncorrect;
                            break;
                        case "2":
                            rdresp = RDLoginResponse.AccountDisabledOrBanned;
                            break;
                        case "3":
                            rdresp = RDLoginResponse.TooManyFailedLogins;
                            break;
                        case "4":
                            rdresp = RDLoginResponse.WrongCaptcha;
                            break;
                        default:
                            rdresp = RDLoginResponse.UnknownError;
                            break;
                    }
                    _authCookie = string.Empty;
                    return rdresp;
                }                
            }
            catch (Exception)
            {
                // handle error
                //MessageBox.Show(ex.Message);
                return RDLoginResponse.NetworkError;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (response != null)
                    response.Close();
            }
        }

        public Task<RDResponse> UnrestrictLinkAsync(string url)
        {
            var task = Task.Factory.StartNew(() => UnrestrictLink(url));
            return task;
        }

        public RDResponse UnrestrictLink(string url){
            WebResponse response = null;
            StreamReader reader = null;
            
            try
            {
                if (string.IsNullOrEmpty(_authCookie))
                    return new RDResponse("Errore di Autenticazione: cookie di Real-Debrid non impostato (questo non dovrebbe succedere...)");

                //hack for MEGA (RD API doesn't seem to unlock non-https mega links correctly)
                if (url.Contains("http://mega.co.nz"))
                    url = url.Replace("http://mega.co.nz", "https://mega.co.nz");

                //URLencoding
                url = Uri.EscapeDataString(url);

                string queryurl = string.Format(Constants.RealDebridUnrestrictLinkWithCookie, url);
            
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(queryurl);
                
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36";
                request.Credentials = CredentialCache.DefaultCredentials;
                CookieContainer cc = new CookieContainer();
                var cookie = new Cookie("auth", _authCookie, "/", ".real-debrid.com");
                cookie.Expires = DateTime.Now.AddYears(1);
                cc.Add(cookie);
                request.CookieContainer = cc;
                request.Timeout = 15000;

                response = request.GetResponse();

                JsonSerializer serializer = new JsonSerializer();
                string resultString = (new StreamReader(request.GetResponse().GetResponseStream())).ReadToEnd();
                Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(resultString);

                //DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings();
                //settings.UseSimpleDictionaryFormat = true;
                //var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, object>), settings);
                //var dictionary = (IDictionary<string, object>)serializer.ReadObject(response.GetResponseStream());
                //Dictionary<string, object> dictionary = null;

                response.Close();

                if (dictionary == null || !dictionary.ContainsKey("error"))
                    return null;

                string error = dictionary["error"].ToString();

                if (error.Equals("0"))
                {
                    string filelink = dictionary["main_link"].ToString();
                    string filename = dictionary["file_name"].ToString();
                    string filesize = dictionary["file_size"].ToString();

                    return new RDResponse(filelink, filename, filesize);
                }
                else
                {
                    return new RDResponse(dictionary["message"].ToString());
                }                
            }
            catch (Exception ex)
            {
                return new RDResponse(ex.Message, ex.StackTrace);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (response != null)
                    response.Close();
            }
        }

        public Task LoadHostersAsync()
        {
            var task = Task.Factory.StartNew(() => LoadHosters());
            return task;
        }

        public void LoadHosters()
        {
            WebResponse response = null;
            StreamReader reader = null;

            if (Hosters == null)
                Hosters = new List<string>();
            else
                Hosters.Clear();

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Constants.RealDebridHostListLink);

                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36";
                request.Credentials = CredentialCache.DefaultCredentials;
                CookieContainer cc = new CookieContainer();
                request.Timeout = 25000;

                response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string result = reader.ReadToEnd();

                var hosts = result.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in hosts)
                {
                    Hosters.Add(item.Replace("\"", ""));
                }

                response.Close();

               
            }
            catch (Exception)
            {
                Hosters = null;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (response != null)
                    response.Close();
            }
        }

        public bool IsLinkSupported(string url)
        {
            if (Hosters != null)
            {
                foreach (var item in Hosters)
                {
                    if (url.ToLower().Contains(item.ToLower()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
