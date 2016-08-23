namespace libRealDebrid4DotNet
{

    public enum RDLoginResponse
    {
        OK, //all went ok, you're logged in
        MissingLoginData, //username, password or both not supplied
        UsernameOrPasswordIncorrect,
        AccountDisabledOrBanned,
        WrongCaptcha,
        TooManyFailedLogins,
        NetworkError,
        UnknownError
    }

    public class RDLoginResponseHelper
    {
        public static string GetErrorMessage(RDLoginResponse r)
        {
            switch (r)
            {
                case RDLoginResponse.OK:
                    return "";
                case RDLoginResponse.MissingLoginData:
                    return "username e/o password mancanti";
                case RDLoginResponse.UsernameOrPasswordIncorrect:
                    return "username e/o password errati";
                case RDLoginResponse.AccountDisabledOrBanned:
                    return "account disabilitato o bloccato";
                case RDLoginResponse.WrongCaptcha:
                    return "verifica richiesta. Recarsi su real-debrid.com e loggarsi con il browser";
                case RDLoginResponse.TooManyFailedLogins:
                    return "verifica richiesta. Recarsi su real-debrid.com e loggarsi con il browser"; ;
                case RDLoginResponse.NetworkError:
                    return "errore di rete (Real-Debrid non raggiungibile o connessione internet assente)"; ;
                case RDLoginResponse.UnknownError:
                    return "errore sconosciuto";
                default:
                    return "";
            }
        }
    }
}