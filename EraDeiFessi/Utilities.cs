using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Data;

namespace EraDeiFessi
{
    static class Utilities
    {
        public static string GetRegistryKey(string subkey, string entry, RegistryHive hive)
        {
            try
            {
                string value64 = string.Empty;
                string value32 = string.Empty;

                RegistryKey localKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
                localKey = localKey.OpenSubKey(subkey);
                if (localKey != null)
                {
                    value64 = localKey.GetValue(entry).ToString();
                    return value64;
                }
                RegistryKey localKey32 = RegistryKey.OpenBaseKey(hive, RegistryView.Registry32);
                localKey32 = localKey32.OpenSubKey(subkey);
                if (localKey32 != null)
                {
                    value32 = localKey32.GetValue(entry).ToString();
                    return value32;
                }
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
            
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
