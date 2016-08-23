using EraDeiFessi.Repository;
using libEraDeiFessi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EraDeiFessi.Helpers
{
    class HistoryManager
    {
        public static bool SharedModeActive = false;
        private static SerializableDictionary<string, HistoryEntry> SharedAccessHistory = new SerializableDictionary<string, HistoryEntry>();

        public static SerializableDictionary<string, HistoryEntry> AccessHistory { get { return SharedModeActive ? SharedAccessHistory : Repo.Settings.AccessHistory; } }

        public static Task<bool> DownloadSharedHistoryAsync()
        {
            var task = Task.Factory.StartNew(() => HistoryManager.DownloadSharedHistory());
            return task;
        }

        public static bool DownloadSharedHistory()
        {
            RequestMaker rm = new RequestMaker();
            string qs = Constants.edfAPIUrl + "?request=gethistory&username="+Repo.Settings.EDFAccount_Username+"&password="+Repo.Settings.EDFAccount_Password;
            string result = rm.MakeRequest(qs);

            if(result.Contains("ERROR") || string.IsNullOrEmpty(result)){
                //failed to retrieve history
                return false;
            }
            else if(result.Contains("EMPTY")){
                //empty history
                SharedAccessHistory = new SerializableDictionary<string, HistoryEntry>();
                return true;
            }
            else if (result.Contains("RESLINE!"))
            {
                SharedAccessHistory = new SerializableDictionary<string, HistoryEntry>();
                //there are results, parse
                    foreach(var l in result.Split(";".ToArray())){
                        string line = l.Trim();
                        if (line != null && line.StartsWith("RESLINE!")){
                            line = line.Replace("RESLINE!", string.Empty);
                            HistoryEntry he = new HistoryEntry();
                            foreach (var kvp in line.Split(",".ToArray()))
                            {
                                if (kvp.StartsWith("link"))
                                    he.link = Encoding.UTF8.GetString( Convert.FromBase64String(kvp.Split("=".ToArray(), 2)[1]));
                                if (kvp.StartsWith("action"))
                                    he.action = (HistoryAction)Enum.Parse(typeof(HistoryAction), Encoding.UTF8.GetString(Convert.FromBase64String(kvp.Split("=".ToArray(), 2)[1])));
                                if (kvp.StartsWith("timestamp"))
                                    he.timestamp = DateTime.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(kvp.Split("=".ToArray(), 2)[1])));
                            }
                            SharedAccessHistory[he.link] = he;
                        }
                    }
                return true;
            }

            SharedAccessHistory = null;
            return false;
        }

        public static Task<bool> AddSharedHistoryEntryAsync(HistoryEntry he)
        {
            var task = Task.Factory.StartNew(() => HistoryManager.AddSharedHistoryEntry(he));
            return task;
        }

        public static bool AddSharedHistoryEntry(HistoryEntry he)
        {
            RequestMaker rm = new RequestMaker();
            string qs = Constants.edfAPIUrl + "?request=addhistory&username=" + Repo.Settings.EDFAccount_Username + "&password=" + Repo.Settings.EDFAccount_Password +
                "&link=" + Base64Encode(he.link) + "&action=" + Base64Encode(he.action.ToString()) + "&timestamp=" + Base64Encode(he.timestamp.ToString());
            string result = rm.MakeRequest(qs);

            if (result.Contains("ERROR") || string.IsNullOrEmpty(result))
            {
                //failed to update history
                return false;
            }

            else if (result.Contains("OK"))
            {
                return true;
            }

            return false;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }


        public static HistoryEntry GetHistory(string link)
        {
            return GetHistory(new string[] { link });
        }

        public static HistoryEntry GetHistory(IEnumerable<string> links)
        {
            HistoryEntry h = null;
            foreach (var item in links)
            {
                if (AccessHistory.ContainsKey(item))
                {
                    if (h == null || h.timestamp < AccessHistory[item].timestamp)
                    {
                        h = AccessHistory[item];
                    }
                }
            }
            return h;
        }

        public static async void SetHistory(string link, HistoryAction action)
        {
            DateTime d = DateTime.Now;
            HistoryEntry he = new HistoryEntry(SharedModeActive ? Repo.Settings.EDFAccount_Username : "[local]", link, d, action);

            if (SharedModeActive)
            {
                bool res = await AddSharedHistoryEntryAsync(he);
                if (!res)
                {
                    MessageBox.Show("Errore: impossibile raggiungere il server di EDF per sincronizzare la cronologia condivisa. La cronologia condivisa verrà disattivata per questa sessione. Puoi fare un nuovo tentativo cliccando il pulsante 'Attiva Cronologia Condivisa'", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                    SharedModeActive = false;
                    HistoryModeToggled(null, new EventArgs());
                }
            }

            AccessHistory[he.link] = he;

            OnHistoryUpdated(new HistoryUpdatedEventArgs(AccessHistory[link]));
        }

        private static void OnHistoryUpdated(HistoryUpdatedEventArgs e)
        {
            if (HistoryUpdated != null)
                HistoryUpdated(null, e);
        }
        public static event HistoryUpdatedEventHandler HistoryUpdated;
        public class HistoryUpdatedEventArgs : EventArgs
        {
            public HistoryEntry Entry { get; set; }

            public HistoryUpdatedEventArgs(HistoryEntry entry) { Entry = entry; }
        }
        public delegate void HistoryUpdatedEventHandler(object sender, HistoryUpdatedEventArgs e);

        public static event HistoryModeToggledEventHandler HistoryModeToggled;
        public delegate void HistoryModeToggledEventHandler(object sender, EventArgs e);

        internal static void ClearLocalHistory()
        {
            Repo.Settings.AccessHistory.Clear();
        }
    }
}
