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
    public class PNMToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PNM pnm = value as PNM;
            if (pnm == null)
                return null;

            var bitmap = new WriteableBitmap(pnm.Width, pnm.Height, 96, 96, PixelFormats.Rgb24, null);
            bitmap.WritePixels(new Int32Rect(0, 0, pnm.Width, pnm.Height), pnm.Raster, pnm.Stride, 0);
            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
