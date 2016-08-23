using HtmlAgilityPack;
using libEraDeiFessi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WatiN.Core;
using WatiN.Core.Native.Windows;

namespace EraDeiFessi.Parser
{
    class WebBypasser
    {
        static WebBypasser()
        {
            Settings.Instance.MakeNewIeInstanceVisible = false;
            Settings.Instance.AutoMoveMousePointerToTopLeft = false;
        }

        private static string[] supportedDomains = new string[] { "adf.ly", "q.gs", "cineblog01.pw" };

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
            if (link.ToLower().Contains("cineblog01.pw"))
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

                var buttonlink = doc.DocumentNode.SelectSingleNode("descendant-or-self::a[@class='btn-wrapper']");
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