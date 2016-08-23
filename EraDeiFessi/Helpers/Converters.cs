using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace EraDeiFessi.Helpers
{

        public class BooleanConverter<T> : IValueConverter
        {
            public BooleanConverter(T trueValue, T falseValue)
            {
                True = trueValue;
                False = falseValue;
            }

            public T True { get; set; }
            public T False { get; set; }

            public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value is bool && ((bool)value) ? True : False;
            }

            public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
            }
        }
    

    public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed) { }
    }

    public sealed class InvertedBooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public InvertedBooleanToVisibilityConverter() :
            base(Visibility.Collapsed, Visibility.Visible) { }
    }

    public class DoubleOffsetter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v = (double)value;
            return (v == 0d || v == double.NaN) ? 0d : v + System.Convert.ToDouble(parameter);
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw  new NotImplementedException();
        }
    }

    public class BandwidthSpeedFormatter : IValueConverter
    {
        string[] ordinals = new[] { "", "K", "M", "G", "T", "P", "E" };
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int bandwidth = (int)value;
            decimal rate = (decimal)bandwidth;
            var ordinal = 0;

            while (rate > 1024)
            {
                rate /= 1024;
                ordinal++;
            }

            return String.Format("{0} {1}b/s", Math.Round(rate, 2, MidpointRounding.AwayFromZero), ordinals[ordinal]);
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
