using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;

namespace UAM.PTO
{
    public class LogicalNoneConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            foreach (object obj in values)
            {
                if (!(obj is bool))
                    return false;
                if ((bool)obj)
                    return false;
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
