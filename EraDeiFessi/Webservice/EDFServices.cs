using System;

namespace EraDeiFessi.Webservice
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "EDFServices" in both code and config file together.
    public class EDFServices : IEDFServices
    {
        public string UnblockURL(string url)
        {
            if (!(Repository.Repo.RDAgent.Authorized))
                return "KO$Non sei loggato su Real-Debrid. Utilizza il menu opzioni di EraDeiFessi per loggarti";

            OnRequestReceived(new RequestReceivedEventArgs(url));
            return "OK$Link ricevuto";
        }

        //request received event and handlers
        protected virtual void OnRequestReceived(RequestReceivedEventArgs e)
        {
            if (RequestReceived != null)
                RequestReceived(this, e);
        }
        public static event RequestReceivedEventHandler RequestReceived;


        public class RequestReceivedEventArgs : EventArgs
        {
            public string Link { get; set; }

            public RequestReceivedEventArgs(string link) { Link = link; }
        }
        public delegate void RequestReceivedEventHandler(object sender, RequestReceivedEventArgs e);
    }
}
