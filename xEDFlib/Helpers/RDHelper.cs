using RealDebrid4DotNet;

namespace xEDFlib.Helpers
{
    public class RDHelper
    {
        private IxEDFSettings xSettings;

        public RDHelper(IxEDFSettings settings)
        {
            xSettings = settings;
            if (settings.RDToken != null && settings.RDToken.Valid)
                RDAgent = new RDAgent(settings.RDToken);
            else
                RDAgent = new RDAgent(libEraDeiFessi.Constants.RealDebridEDFClientID);
        }

        public RDAgent RDAgent { get; set; }
        public bool LoggedIntoRD
        {
            get
            {
                return RDAgent != null && RDAgent.Token != null && RDAgent.Authorized;
            }
        }

        public void LogOutRD()
        {
            RDAgent.Logout();
            xSettings.RDToken = null;
        }

    }
}
