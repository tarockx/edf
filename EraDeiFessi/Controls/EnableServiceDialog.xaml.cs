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

namespace EraDeiFessi.Controls
{
    /// <summary>
    /// Interaction logic for EnableServiceDialog.xaml
    /// </summary>
    public partial class EnableServiceDialog : Window
    {
        public EnableServiceDialog()
        {
            InitializeComponent();
        }

        private void ButtonDisable_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonActivate_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
