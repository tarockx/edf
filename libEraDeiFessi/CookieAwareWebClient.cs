namespace System.Net
{
    using System.Text;
    using System.Collections.Specialized;
    using IO;
    using HtmlAgilityPack;

    public class CookieAwareWebClient : WebClient
    {
        
        public CookieAwareWebClient(CookieContainer container)
        {
            CookieContainer = container;
        }

        public CookieAwareWebClient() : this(new CookieContainer()) { }

        public CookieContainer CookieContainer { get; private set; }
        public string SessionID { get; private set; }

        public void MergeCookies(CookieCollection cookies)
        {
            foreach (Cookie item in cookies)
            {
                CookieContainer.Add(item);
            }
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);

            request.CookieContainer = CookieContainer;
            return request;
        }
    }
}