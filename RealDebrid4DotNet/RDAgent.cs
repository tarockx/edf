using RealDebrid4DotNet.RestModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace RealDebrid4DotNet
{
    public class RDAgent
    {
        object lockObject = new object();

        private WebClient wc = new WebClient();
        private string client_id;

        private Timer authorizationCheckTimer;
        private ElapsedEventHandler authorizationCheckTimerAction;

        private TokenRequestResponse tokenRequestResponse;
        private List<string> supportedHosts;

        public event EventHandler AuthenticationSuccessful;
        public event EventHandler AuthenticationFailed;
        public event EventHandler TokenRefreshed;

        public bool Authorized
        {
            get
            {
                return tokenRequestResponse != null;
            }
        }
        public bool TokenInDate
        {
            get
            {
                return tokenRequestResponse != null && tokenRequestResponse.expirationDate > DateTime.Now;
            }
        }
        public TokenRequestResponse Token { get { return tokenRequestResponse; } }

        public RDAgent(string client_id)
        {
            this.client_id = client_id;
            authorizationCheckTimer = new Timer();
        }

        public RDAgent(TokenRequestResponse t)
        {
            this.client_id = t.client_id;
            tokenRequestResponse = t;
            authorizationCheckTimer = new Timer();
        }

        public void Logout()
        {
            tokenRequestResponse = null;
            supportedHosts = null;
        }

        #region Authorization and Authentication / Token refresh
        public Task<AuthorizationCodeRequestResponse> GetAuthorizationCodeAsync()
        {
            return Task.Factory.StartNew(() => GetAuthorizationCode());
        }

        public AuthorizationCodeRequestResponse GetAuthorizationCode()
        {
            try
            {
                string deviceEndpoint = string.Format(Constants.RDDeviceEndpoint, client_id);
                string result = wc.DownloadString(deviceEndpoint);

                var authenticationRequestResponse = new AuthorizationCodeRequestResponse(result);
                return authenticationRequestResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void StartCheckingForAuthorization(AuthorizationCodeRequestResponse authorizationRequestResponse)
        {
            authorizationCheckTimerAction = delegate (object sender, ElapsedEventArgs e)
            {
                if (System.Threading.Monitor.TryEnter(lockObject))
                {
                    try
                    {
                        AuthorizationCheckTimer_Elapsed(authorizationRequestResponse);
                    }
                    finally
                    {
                        System.Threading.Monitor.Exit(lockObject);
                    }
                }
            };
            authorizationCheckTimer.Elapsed += authorizationCheckTimerAction;

            authorizationCheckTimer.Interval = authorizationRequestResponse.interval * 1000;
            authorizationCheckTimer.Start();
        }

        public void StopCheckingForAuthorization()
        {
            authorizationCheckTimer.Stop();
            authorizationCheckTimer.Elapsed -= authorizationCheckTimerAction;
        }

        private void AuthorizationCheckTimer_Elapsed(AuthorizationCodeRequestResponse authorizationRequestResponse)
        {
            //Authorization window expired, the user took to long to authorize the app
            if (DateTime.Now > authorizationRequestResponse.AuthorizationWindow)
            {
                StopCheckingForAuthorization();
                AuthenticationFailed(this, new EventArgs());
                return;
            }

            try
            {
                //Check if we're authorized
                string credentialsEndpoint = string.Format(Constants.RDCredentialsEndpoint, client_id, authorizationRequestResponse.device_code);
                string result = wc.DownloadString(credentialsEndpoint);

                var credentialsRequestResponse = new CredentialsRequestResponse(result);
                if (credentialsRequestResponse != null)
                {
                    StopCheckingForAuthorization();
                    ObtainAuthorizationToken(
                        credentialsRequestResponse.client_id,
                        credentialsRequestResponse.client_secret,
                        authorizationRequestResponse.device_code
                        );
                    return;
                }

            }
            catch (Exception)
            {
                return;
            }
        }

        private void ObtainAuthorizationToken(string client_id, string client_secret, string code)
        {
            tokenRequestResponse = null;
            bool success = false;
            int failedTries = 0;
            while (!success && failedTries < 2)
            {
                try
                {
                    string tokenEndpoint = Constants.RDTokenEndpoint;
                    NameValueCollection parameters = new NameValueCollection();
                    parameters.Add("client_id", client_id);
                    parameters.Add("client_secret", client_secret);
                    parameters.Add("code", code);
                    parameters.Add("grant_type", "http://oauth.net/grant_type/device/1.0");

                    string response = Encoding.UTF8.GetString(wc.UploadValues(tokenEndpoint, "POST", parameters));
                    var newToken = new TokenRequestResponse(response);
                    newToken.client_id = client_id;
                    newToken.client_secret = client_secret;
                    tokenRequestResponse = newToken;

                    success = true;
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(WebException))
                    {
                        ErrorResponse error = ErrorResponse.Get((ex as WebException).Response);
                        Console.WriteLine(error.ErrorMessage);
                    }
                    failedTries++;
                }
            }

            if (success)
            {
                if (AuthenticationSuccessful != null)
                    AuthenticationSuccessful(this, new EventArgs());
                LoadSupportedHostsAsync();
            }
            else
            {
                if (AuthenticationFailed != null)
                    AuthenticationFailed(this, new EventArgs());
            }

        }

        public void RefreshAuthorizationToken()
        {
            if (tokenRequestResponse != null)
            {
                ObtainAuthorizationToken(tokenRequestResponse.client_id, tokenRequestResponse.client_secret, tokenRequestResponse.refresh_token);
                if (TokenRefreshed != null)
                    TokenRefreshed(this, new EventArgs());
            }
        }

        #endregion


        #region RD Functionality
        public Task<LinkUnrestrictRequestResponse> UnrestrictLinkAsync(string url)
        {
            return Task.Factory.StartNew(() => UnrestrictLink(url));
        }

        public LinkUnrestrictRequestResponse UnrestrictLink(string url)
        {
            try
            {
                if (!TokenInDate)
                    RefreshAuthorizationToken();
                string unrestrictEndpoint = string.Format(Constants.RDUnrestrictLink, tokenRequestResponse.access_token);
                NameValueCollection parameters = new NameValueCollection();
                parameters.Add("link", url);

                string response = Encoding.UTF8.GetString(wc.UploadValues(unrestrictEndpoint, "POST", parameters));
                LinkUnrestrictRequestResponse linkUnrestrictResponse = new LinkUnrestrictRequestResponse(response);
                return linkUnrestrictResponse;
            }
            catch (WebException ex)
            {
                try
                {
                    ErrorResponse error = ErrorResponse.Get(ex.Response);
                    return new LinkUnrestrictRequestResponse(error.error_code, error.ErrorMessage);
                }
                catch (Exception)
                {
                    throw ex;
                }

            }
            catch (Exception ex)
            {
                return new LinkUnrestrictRequestResponse(0, "Errore di rete o altra eccezione non gestita: " + ex.Message);
            }
        }

        private Task LoadSupportedHostsAsync()
        {
            return Task.Factory.StartNew(() => LoadSupportedHosts());
        }

        private void LoadSupportedHosts()
        {
            try
            {
                if (!TokenInDate)
                    RefreshAuthorizationToken();
                string supportedHostsEndpoint = string.Format(Constants.RDSupportedHostsEndpoint, tokenRequestResponse.access_token);
                string response = wc.DownloadString(supportedHostsEndpoint);
                SupportedDomainsRequestResponse supportedHostsRequestResponse = new SupportedDomainsRequestResponse(response);
                supportedHosts = supportedHostsRequestResponse.SupportedHosters;
            }
            catch (Exception)
            {
            }
        }

        public bool LinkSupported(string url)
        {
            if (supportedHosts == null)
                LoadSupportedHosts();

            if (supportedHosts != null && url != null)
            {
                foreach (var item in supportedHosts)
                {
                    if (url.Contains(item.ToLower()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion
    }
}
