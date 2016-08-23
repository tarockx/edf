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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EraDeiFessi.Controls
{
    /// <summary>
    /// Interaction logic for SearchTabHeader.xaml
    /// </summary>
    public partial class SearchTabHeader : UserControl
    {
        public bool ShowSearchImage { 
            get { return (loadingGIF.Visibility == System.Windows.Visibility.Visible); } 
            set { loadingGIF.Visibility = value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }
        }

        public SearchTabHeader(string title)
        {
            InitializeComponent();
            tbTitle.Text = title;
        }

        public void SetCounter(int count){
            tbCounter.Text = " [" + count.ToString() + "]";
            if (count == 0)
                tbCounter.Foreground = Brushes.Red;
            else
                tbCounter.Foreground = Brushes.Green;
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            loadingGIF.IsEnabled = (loadingGIF.Visibility == System.Windows.Visibility.Visible);
        }
    }
}
