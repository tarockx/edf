using System;

using xEDFlib.Updater;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using UIKit;

namespace xEDF.iOS
{
    public class xEDFiOSUpdater : Updater
    {
        public xEDFiOSUpdater(string currentVersion) : base(currentVersion, Constants.ReleaseManifestUrl)
        {

        }

        public void CheckForUpdatesAndPrompt(UIViewController controller)
        {
            CheckForUpdatesAndPromptAsyncCall(controller);
        }

        private async void CheckForUpdatesAndPromptAsyncCall(UIViewController controller)
        {
            CheckForUpdatesResponse resp = await CheckForUpdatesAsync();

            if (resp != null && resp.IsNewVersionAvailable)
            {
                var latestRel = resp.GetNewVersionInfo();
                string changes = "";
                foreach (var item in resp.ReleaseManifest)
                {
                    if (!item.IsNewer(_currentVersion))
                        break;
                    var itemNotes = item.Notes.Replace("\t", "").Trim().Split('\n');
                    changes += item.Version + "\n";
                    foreach (var note in itemNotes)
                    {
                        changes += "*" + note.Trim() + "\n";
                    }
                    changes += "\n";
                }

                string message = string.Format("Versione attuale: {0}\nUltima versione: {1}\n\nVuoi scaricare l'aggiornamento?\nEcco cosa cambia:\n\n{2}", _currentVersion, latestRel.Version, changes);

                //Create Alert
                var textInputAlertController = UIAlertController.Create("Nuova versione disponibile!", message, UIAlertControllerStyle.Alert);

                //Add Actions
                var updateConfirmAction = UIAlertAction.Create("Aggiorna", UIAlertActionStyle.Default, alertAction => {
                    Helpers.OpenAndShareHelper.OpenUrl(Constants.LatestReleasePackageUrl);
                });
                var updateCancelAction = UIAlertAction.Create("No, non ora", UIAlertActionStyle.Cancel, alertAction => { });
                var updateCancelAndDontAskAgainAction = UIAlertAction.Create("No, e non chiedermelo più", UIAlertActionStyle.Default, alertAction => {
                    Repo.Settings.CheckForUpdates = false;
                });


                textInputAlertController.AddAction(updateConfirmAction);
                textInputAlertController.AddAction(updateCancelAction);
                textInputAlertController.AddAction(updateCancelAndDontAskAgainAction);

                //Present Alert
                controller.PresentViewController(textInputAlertController, true, null);
            }
        }

       
    }
}