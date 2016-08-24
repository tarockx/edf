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
using System.Reflection;
using System.ComponentModel;

namespace PropGrid4WPF
{
    /// <summary>
    /// Interaction logic for PropGrid.xaml
    /// </summary>
    public partial class PropGrid : UserControl
    {
        private List<UIElement> controlsToUnbind = new List<UIElement>();

        private object _DataSource;
        public object DataSource
        {
            get { return _DataSource; }
            set
            {
                if (_DataSource != value){
                    _DataSource = value;
                    BindObject();
                }
            }
        }

        public PropGrid()
        {
            InitializeComponent();
        }

        private void BindObject(){
            mg.Children.Clear();
            mg.RowDefinitions.Clear();
            UnbindAll();
            controlsToUnbind.Clear();

            if (DataSource == null)
                return;

            var properties = DataSource.GetType().GetProperties();

            if (properties == null)
                return;

            int row = 0;
            foreach (var p in properties)
            {
                mg.RowDefinitions.Add(new RowDefinition());

                if (p.PropertyType.IsEnum)
                {
                    TextBlock tb = new TextBlock();
                    var att = p.GetCustomAttribute(typeof(DescriptionAttribute), false) as DescriptionAttribute;
                    tb.Text = att.Description;
                    tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tb.Margin = new Thickness(2);
                    tb.SetValue(Grid.ColumnProperty, 0);
                    tb.SetValue(Grid.RowProperty, row);
                    
                    mg.Children.Add(tb);
                    controlsToUnbind.Add(tb);

                    ComboBox cb = new ComboBox();
                    cb.ItemsSource = Enum.GetValues(p.PropertyType); 
                    cb.Margin = new Thickness(2);
                    
                    Binding b = new Binding(p.Name);
                    b.Source = DataSource;
                    b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    BindingOperations.SetBinding(cb, ComboBox.SelectedItemProperty, b);

                    cb.SetValue(Grid.ColumnProperty, 1);
                    cb.SetValue(Grid.RowProperty, row);
                    mg.Children.Add(cb);
                    controlsToUnbind.Add(cb);

                    row++;
                }
                else if(p.PropertyType == typeof(string))
                {
                    TextBlock tb = new TextBlock();
                    var att = p.GetCustomAttribute(typeof(DescriptionAttribute), false) as DescriptionAttribute;
                    tb.Text = att.Description;
                    tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tb.Margin = new Thickness(2);
                    tb.SetValue(Grid.ColumnProperty, 0);
                    tb.SetValue(Grid.RowProperty, row);

                    mg.Children.Add(tb);
                    controlsToUnbind.Add(tb);

                    TextBox txt = new TextBox();
                    tb.Margin = new Thickness(2);

                    Binding b = new Binding(p.Name);
                    b.Source = DataSource;
                    b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    BindingOperations.SetBinding(txt, TextBox.TextProperty, b);

                    txt.SetValue(Grid.ColumnProperty, 1);
                    txt.SetValue(Grid.RowProperty, row);
                    mg.Children.Add(txt);
                    controlsToUnbind.Add(txt);

                    row++;
                }
            }
        }

        private void UnbindAll()
        {
            foreach (var item in controlsToUnbind)
	        {
                BindingOperations.ClearAllBindings(item);
	        }                
        }
    }
}
