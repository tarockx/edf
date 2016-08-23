using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EraDeiFessi.Updater;

namespace EraDeiFessi.Controls
{
    /// <summary>
    /// Interaction logic for UpdateDialog.xaml
    /// </summary>
    public partial class UpdateDialog : Window
    {
        public bool DontAskAgain { get { return chkDontAskAgain.IsChecked.Value; } }

        public UpdateDialog(CheckForUpdatesResponse resp)
        {
            InitializeComponent();

            var latestRel = resp.GetNewVersionInfo();
            string changes = "";
            foreach (var item in resp.ReleaseManifest)
            {
                if (!item.IsNewer(Constants.Version))
                    break;
                var itemNotes = item.Notes.Replace("\t", "").Trim().Split('\n');
                changes += item.Version + "\n";
                foreach (var note in itemNotes)
	            {
                    changes += "   " + note.Trim() + "\n";
	            }
                changes += "\n";
            }

            tbNew.Text = latestRel.Version;
            tbOld.Text = Constants.Version;
            tbChanges.Text = changes;
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }


    }
}
