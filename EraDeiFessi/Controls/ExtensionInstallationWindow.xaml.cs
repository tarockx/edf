using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace EraDeiFessi.Controls
{
    /// <summary>
    /// Interaction logic for ExtensionInstallationWindow.xaml
    /// </summary>
    public partial class ExtensionInstallationWindow : Window
    {
        public ExtensionInstallationWindow()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void showExtension_Click(object sender, RoutedEventArgs e)
        {
            string args = "/select,\"" + Constants.ExtensionLocation + "\"";
            Process.Start("explorer.exe", args);
        }

    }
}
