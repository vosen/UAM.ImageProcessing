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
    public class PNMToHistogramGeometryConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PNM pnm = values[0] as PNM;
            int? selection = values[1] as int?;
            Visibility? visibility = values[2] as Visibility?;
            if(pnm == null || selection == null || visibility != Visibility.Visible)
                return Binding.DoNothing;

            double[] rawData;
            switch (selection)
            {
                case 0:
                    rawData = pnm.GetHistogramLuminosity();
                    break;
                case 1:
                    rawData = pnm.GetHistogramRed();
                    break;
                case 2:
                    rawData = pnm.GetHistogramGreen();
                    break;
                case 3:
                    rawData = pnm.GetHistogramBlue();
                    break;
                default:
                    return Binding.DoNothing;
            }

            return BuildHistogramGeometry(rawData);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        private static Geometry BuildHistogramGeometry(double[] data)
        {
            CombinedGeometry aggregateGeometry = data.Aggregate(Tuple.Create(0, new CombinedGeometry()), (geo, val) =>
            {
                return Tuple.Create(
                                geo.Item1 + 1,
                                new CombinedGeometry(geo.Item2, ChartBlock(geo.Item1, val)));
            }).Item2;
            ScaleTransform transform = new ScaleTransform(1, 128 / aggregateGeometry.GetRenderBounds(null).Height, 0, 128);
            PathGeometry scaledGeometry = Geometry.Combine(Geometry.Empty, aggregateGeometry, GeometryCombineMode.Union, transform);
            return scaledGeometry.GetFlattenedPathGeometry();
        }

        private static RectangleGeometry ChartBlock(int index, double val)
        {
            double height = val * 128;
            return new RectangleGeometry(new Rect(index, 128 - height, 1, height));
        }
    }
}
