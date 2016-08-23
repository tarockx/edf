using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace EraDeiFessi.Controls
{
    /// <summary>
    /// Interaction logic for LoadingPanel.xaml
    /// </summary>
    public partial class LoadingPanel : UserControl
    {
        public LoadingPanel()
        {
            InitializeComponent();
        }

        public int Size
        {
            get
            {
                return (int)GetValue(SizeProperty);
            }
            set
            {
                SetValue(SizeProperty, value);
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException();
                }
                else
                {
                    loadingGIF.Height = loadingGIF.Width = value;
                    transform.CenterX = transform.CenterY = value / 2;
                }
            }
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(int), typeof(LoadingPanel));

        private void UserControl_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            loadingGIF.IsEnabled = (Visibility == System.Windows.Visibility.Visible);
        }

    }
}
