using System;

using xEDFlib.Updater;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Android.Content;
using Android.App;
using Android.Widget;
using Android.Views;

namespace xEDF.Droid
{
    public class xEDFDroidUpdater : Updater
    {
        Context _context;
        public xEDFDroidUpdater(Context context, string currentVersion, string releaseManifestUrl) : base(currentVersion, releaseManifestUrl)
        {
            _context = context;
        }

        public void CheckForUpdatesAndPrompt(Activity activity)
        {
            CheckForUpdatesAndPromptAsyncCall(activity);
        }

        private ProgressDialog downloadProgress;
        private AlertDialog updateAlert;
        private async void CheckForUpdatesAndPromptAsyncCall(Activity context)
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

                AlertDialog.Builder builder = new AlertDialog.Builder(context);
                LayoutInflater inflater = context.LayoutInflater;
                View updateDlg = inflater.Inflate(Resource.Layout.UpdatePopup, null);
                updateDlg.FindViewById<TextView>(Resource.Id.txtUpdateMessage).Text = message;
                updateDlg.FindViewById<Button>(Resource.Id.btnUpdateNow).Click += async (sender, e) => {
                    updateAlert.Dismiss();

                    //open update
                    //Helpers.OpenAndShareHelper.Open(Constants.LatestReleasePackageUrl);
                    downloadProgress = new ProgressDialog(context);
                    downloadProgress.SetTitle("Download aggiornamento");
                    downloadProgress.SetMessage("Download dell'aggiornamento in corso...");
                    //downloadProgress.SetProgressStyle(ProgressDialogStyle.Horizontal);
                    //downloadProgress.Max = 100;
                    //downloadProgress.Progress = 0;
                    downloadProgress.Show();

                    bool success = await DownloadUpdateAsync();

                    downloadProgress.Dismiss();

                    if (success)
                    {
                        Intent intent = new Intent(Intent.ActionView);
                        var externalPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).Path;
                        var fileDownloadLocation = externalPath + "/xEDF.apk";
                        intent.SetDataAndType(Android.Net.Uri.FromFile(new Java.IO.File(fileDownloadLocation)), "application/vnd.android.package-archive");
                        intent.SetFlags(ActivityFlags.NewTask);
                        context.StartActivity(intent);
                    }
                    else
                    {
                        Toast.MakeText(context, "ERRORE: impossibile scaricare l'aggiornamento.", ToastLength.Long).Show();
                    }
                };

                updateDlg.FindViewById<Button>(Resource.Id.btnUpdateNotNow).Click += (sender, e) =>
                {
                    updateAlert.Dismiss();
                };

                updateDlg.FindViewById<Button>(Resource.Id.btnUpdateDontAskAgain).Click += (sender, e) =>
                {
                    Repo.Settings.CheckForUpdates = false;
                    updateAlert.Dismiss();
                };

                builder.SetView(updateDlg);

                updateAlert = builder.Create();
                updateAlert.Show();

            }
        }

        public Task<bool> DownloadUpdateAsync()
        {
            var task = Task.Factory.StartNew(() => DownloadUpdate());
            return task;
        }

        public bool DownloadUpdate()
        {
            try
            {
                using (WebClient Client = new WebClient())
                {
                    var externalPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).Path;
                    var fileDownloadLocation = externalPath + "/xEDF.apk";
                    if (File.Exists(fileDownloadLocation))
                        File.Delete(fileDownloadLocation);

                    //Client.DownloadProgressChanged += (sender, e) =>
                    //{
                    //    try
                    //    {
                    //        (downloadProgress.Context as Activity).RunOnUiThread(
                    //            () =>
                    //            {
                    //                downloadProgress.Progress = e.ProgressPercentage;
                    //            });
                            
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Console.WriteLine(ex.Message);
                    //    }
                    //};

                    Client.DownloadFile(Constants.LatestReleasePackageUrl, fileDownloadLocation);

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}