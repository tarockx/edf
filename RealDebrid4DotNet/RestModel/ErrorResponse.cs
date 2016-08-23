using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RealDebrid4DotNet.RestModel
{
    public class ErrorResponse
    {
        public static string getErrorMessage(int error_code)
        {
            switch (error_code)
            {
                case -1:
                    return "Errore interno di Real-Debrid";
                case 1:
                    return "Parametro mancante";
                case 2:
                    return "Valore parametro non corretto";
                case 3:
                    return "Metodo API sconosciuto";
                case 4:
                    return "Metodo non consentito (l'applicazione corrente non è autorizzata ad accedere a questa funzionalità dell'API)";
                case 5:
                    return "Troppe richieste consecutive: attendere alcuni secondi e riprovare";
                case 6:
                    return "Risorsa non raggiungibile";
                case 7:
                    return "Risorsa inesistente";
                case 8:
                    return "Token Real-Debrid non valid. Provare a ri-eseguire la procedura di autorizzazione dal menu opzioni";
                case 9:
                    return "Permesso negato";
                case 10:
                    return "Account non autorizzato. Eseguire la procedura di autorizzazione";
                case 11:
                    return "Autorizzazione in due passaggi attiva: recarsi sul sito di Real-Debrid per completare l'autorizzazione";
                case 12:
                    return "Autorizzazione in due passaggi attiva: recarsi sul sito di Real-Debrid per completare l'autorizzazione";
                case 13:
                    return "Password non corretta";
                case 14:
                    return "Account bloccato! Recarsi sul sito di Real-Debrid per maggiori dettagli";
                case 15:
                    return "Account non attivo / giorni premium terminati";
                case 16:
                    return "Sito non supportato da Real-Debrid";
                case 17:
                    return "Sito attualmente in manutenzione";
                case 18:
                    return "Limite giornaliero per questo sito raggiunto! Riprovare domani";
                case 19:
                    return "Sito momentaneamente non disponibile";
                case 20:
                    return "Sito disponibile solo agli utenti premium. Recarsi sul sito Real-Debrid per rinnovare l'abbonamento.";
                case 21:
                    return "Troppi download simultanei attivi";
                case 22:
                    return "Il tuo indirizzo IP è bannato";
                case 23:
                    return "Traffico disponibile esaurito";
                case 24:
                    return "File non disponibile";
                case 25:
                    return "Servizio Real-Debrid offline. Riprovare più tardi";
                case 26:
                    return "Dimensione file di upload eccessiva";
                case 27:
                    return "Errore di upload";
                case 28:
                    return "File non consentito";
                case 29:
                    return "Dimensioni del torrent eccessive";
                case 30:
                    return "File torrent non valido";
                case 31:
                    return "Azione già effettuata";
                case 32:
                    return "Risoluzione dell'immagine eccessiva o non supportata";
                default:
                    return "Errore generico:" + error_code;
            }
        }

        public string error { get; set; }
        public int error_code { get; set; }

        public string ErrorMessage
        {
            get
            {
                return getErrorMessage(error_code);
            }
        }

        public ErrorResponse(string jsonResponse)
        {
            JsonSerializer serializer = new JsonSerializer();
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResponse);

            error = dictionary["error"].ToString();
            error_code = int.Parse(dictionary["error_code"].ToString());
        }

        public static ErrorResponse Get(WebResponse webresponse)
        {
            using (var reader = new System.IO.StreamReader(webresponse.GetResponseStream(), Encoding.UTF8))
            {
                try
                {
                    string response = reader.ReadToEnd();
                    ErrorResponse error = new ErrorResponse(response);
                    return error;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }
    }
}
