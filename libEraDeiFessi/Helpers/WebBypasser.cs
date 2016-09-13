using HtmlAgilityPack;
using libEraDeiFessi;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace libEraDeiFessi
{
    public class WebBypasser
    {

        private static string[] supportedDomains = new string[] { "adf.ly", "q.gs", "cineblog01.pw", "swzz.xyz" };

        public static bool IsLinkSupported(string link)
        {
            foreach (var item in supportedDomains)
            {
                if (link.ToLower().Contains(item))
                    return true;
            }
            return false;
        }

        public static string Bypass(string link, string referer)
        {
            if (link.ToLower().Contains("adf.ly") || link.ToLower().Contains("q.gs"))
                return BypassAdfLy(link);
            if (link.ToLower().Contains("cineblog01.pw") || link.ToLower().Contains("swzz.xyz"))
                return BypassCineblog01pw(link, referer);

            return string.Empty;
        }

        public static Task<string> BypassAsync(string link, string referer)
        {
            var task = Task.Factory.StartNew(() => Bypass(link, referer));
            return task;
        }

        public static string BypassAdfLy(string link)
        {
            string resp = string.Empty;

            try
            {
                RequestMaker rm = new RequestMaker();
                resp = rm.MakeRequest(link);

                string ysmm = string.Empty;
                Regex regex = new Regex("var ysmm = '[A-Za-z0-9]+'");
                Match match = regex.Match(resp);
                
                if(match != null)
                {
                    ysmm = match.Captures[0].Value.Replace("var ysmm = '", "").Replace("'", "").Trim();
                }

                string left = string.Empty;
                string right = string.Empty;
                for(int i = 0; i < ysmm.Length; i++)
                {
                    if (i % 2 == 0) {
                        left += ysmm[i];
                    }
                    else
                    {
                        right = ysmm[i] + right;
                    }
                }

                byte[] result_bytes = Convert.FromBase64String(left + right);
                string result = Encoding.UTF8.GetString(result_bytes);

                return result.Substring(2);
            }
            catch
            {
                return string.Empty;
            }


            /*
            var th = new Thread(() =>
            {
                try
                {
                    using (var browser = new IE(link))
                    {
                        browser.ShowWindow(NativeMethods.WindowShowStyle.Hide);
                        var skiplink = browser.Link(Find.ById("skip_button"));
                        int counter = 0;
                        while (counter < 20 && string.IsNullOrWhiteSpace(skiplink.Url))
                        {
                            System.Threading.Thread.Sleep(1000);
                            counter++;
                        }

                        resp = skiplink.Url;
                    }
                }
                catch (Exception)
                {
                    resp = string.Empty;
                }

            });


            th.SetApartmentState(ApartmentState.STA); //necessario per WatIn
            th.Start();
            th.Join();

            return resp;
            */
        }

        public static string BypassCineblog01pw(string link, string referer)
        {
            string resp = string.Empty;
            try
            {
                RequestMaker rm = new RequestMaker();
                rm.Referer = referer;
                resp = rm.MakeRequest(link);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(resp);

                var buttonlink = doc.DocumentNode.SelectSingleNode("descendant::a[contains(@class, 'link') and contains(@class, 'btn-wrapper')]");
                string href = buttonlink.GetAttributeValue("href", string.Empty);

                return href;

                //if (resp.Contains("window.location.href"))
                //{
                //    resp = resp.Substring(resp.IndexOf("window.location.href") + "window.location.href".Length);
                //    resp = resp.Substring(resp.IndexOf("=") + 1, resp.IndexOf(";") - 2);
                //    resp = resp.Replace("\"", "");
                //    return resp;
                //}
            }
            catch (Exception)
            {
                resp = string.Empty;
            }
            return resp;

        }
    }
}