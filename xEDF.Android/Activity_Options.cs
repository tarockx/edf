using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.OS;
using Android.Preferences;
using libEraDeiFessi.Plugins;

namespace xEDF.Droid
{
    [Activity(Label = "Opzioni")]
    public class Activity_Options : Activity
    {
        class xEDFPreferenceFragment : PreferenceFragment
        {
            private List<string> disabledPlugins = Repo.Settings.DisabledPlugins;
            public override void OnCreate(Bundle savedInstanceState)
            {
                base.OnCreate(savedInstanceState);

                PreferenceScreen screen = PreferenceManager.CreatePreferenceScreen(Activity);

                //*** General options ***
                PreferenceCategory categoryMain = new PreferenceCategory(Activity);
                categoryMain.Title = "Opzioni generali";
                screen.AddPreference(categoryMain);

                //Check for updates
                SwitchPreference checkForUpdatesPreference = new SwitchPreference(Activity);
                checkForUpdatesPreference.Checked = Repo.Settings.CheckForUpdates;
                checkForUpdatesPreference.Title = "Controlla aggiornamenti";
                checkForUpdatesPreference.Summary = "Se attiva, xEDF notificherà la presenza di eventuali versioni aggiornate all'avvio dell'app";
                checkForUpdatesPreference.PreferenceChange += CheckForUpdatesPreference_PreferenceChange;

                categoryMain.AddPreference(checkForUpdatesPreference);

                //Logout RD
                Preference RDlogoutPreference = new Preference(Activity);
                RDlogoutPreference.Title = "Dimentica credenziali Real-Debrid";
                RDlogoutPreference.PreferenceClick += RDlogoutPreference_PreferenceClick;
                RDlogoutPreference.Enabled = Repo.RDHelper.RDAgent != null && Repo.RDHelper.LoggedIntoRD;
                if (RDlogoutPreference.Enabled)
                    RDlogoutPreference.Summary = "Tocca questa opzione per de-autorizzare le tue credenziali Real-Debrid (utile se vuoi cambiare account). Ti verrà chiesto di ri-eseguire la procedura di autorizzazione se tenti sbloccare un link";
                else
                    RDlogoutPreference.Summary = "Non sei al momento autorizzato su Real-Debrid";
                categoryMain.AddPreference(RDlogoutPreference);

                //*** Enabled Plugins ***
                PreferenceCategory categoryPlugins = new PreferenceCategory(Activity);
                categoryPlugins.Title = "Plugin abilitati";
                screen.AddPreference(categoryPlugins);

                List<IEDFPlugin> plugins = PluginRepo.Plugins.Values.ToList();
                plugins.Sort((x, y) => { return x.pluginName.CompareTo(y.pluginName); });

                foreach (var item in plugins)
                {
                    SwitchPreference pluginPreference = new SwitchPreference(Activity);
                    pluginPreference.Checked = !disabledPlugins.Contains(item.pluginID);
                    pluginPreference.Title = item.pluginName;
                    pluginPreference.Summary = string.Format("Autore: {0}\nID: {1}", item.pluginAuthor, item.pluginID);
                    pluginPreference.PreferenceChange += (sender, e) => { PluginPreference_PreferenceChange(item.pluginID, (bool)e.NewValue); };

                    categoryPlugins.AddPreference(pluginPreference);
                }

                PreferenceScreen = screen;
            }

            private void RDlogoutPreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
                builder.SetMessage("Vuoi disconnetterti da Real-Debrid?");
                builder.SetPositiveButton("Sì", (sender2, e2) => {
                    Repo.RDHelper.LogOutRD();
                    e.Preference.Enabled = false;
                    e.Preference.Summary = "Non sei al momento connesso a Real-Debrid";
                });
                builder.SetNegativeButton("No", (sender2, e2) => { });
                builder.Create().Show();
            }

            private void CheckForUpdatesPreference_PreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
            {
                Repo.Settings.CheckForUpdates = (bool)e.NewValue;
            }

            private void PluginPreference_PreferenceChange(string plugin, bool newState)
            {
                if (newState)
                    disabledPlugins.Remove(plugin);
                else
                    disabledPlugins.Add(plugin);
                Repo.Settings.DisabledPlugins = disabledPlugins;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            
            FragmentTransaction mFragmentTransaction = FragmentManager.BeginTransaction();
            xEDFPreferenceFragment mPrefsFragment = new xEDFPreferenceFragment();
            mFragmentTransaction.Replace(Android.Resource.Id.Content, mPrefsFragment);
            mFragmentTransaction.Commit();
        }
    }
}