using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;

namespace UAM.PTO
{
    /// <summary>
    /// Interaction logic for HistogramWindow.xaml
    /// </summary>
    public partial class HistogramWindow : Window
    {
        public HistogramWindow()
        {
            DependencyPropertyDescriptor.FromProperty(Window.VisibilityProperty, typeof(HistogramWindow)).AddValueChanged(this, VisibilityChanged);
            InitializeComponent();
        }

        private void VisibilityChanged(object src, EventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
                DrawHistogram();
        }

        private void DrawHistogram()
        {
            if (HistogramImage == null)
                return;

            HistogramPath.Data = BuildHistogramGeometry(GetHistogram());
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DrawHistogram();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        // builds path 256 px wide, 128 pix tall
        private Geometry BuildHistogramGeometry(double[] data)
        {
            CombinedGeometry aggregateGeometry = data.Aggregate(Tuple.Create(0,new CombinedGeometry()), (geo,val) =>
                {
                    return Tuple.Create(
                                    geo.Item1 + 1,
                                    new CombinedGeometry(geo.Item2, ChartBlock(geo.Item1, val)));
                }).Item2;
            /*
            CombinedGeometry geometry = new CombinedGeometry()
            Path path = new Path();
            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure() { IsFilled = true, IsClosed = true, StartPoint = new Point(0,127) };
            for (int i = 0; i < data.Length; i++)
            {
                figure.Segments.Add(new LineSegment(new Point(i, 127 - (data[i] * 127)), true));
            }
            figure.Segments.Add(new LineSegment(new Point(255, 127), true));
            geometry.Figures.Add(figure);
             * */
            ScaleTransform transform = new ScaleTransform(1, 128/aggregateGeometry.GetRenderBounds(null).Height,0,128);
            PathGeometry scaledGeometry = Geometry.Combine(Geometry.Empty, aggregateGeometry, GeometryCombineMode.Union, transform);
            return scaledGeometry.GetFlattenedPathGeometry();
        }

        private static RectangleGeometry ChartBlock(int index, double val)
        {
            double height = val * 128;
            return new RectangleGeometry(new Rect(index, 128 - height, 1, height));
        }

        private double[] GetHistogram()
        {
            PNM pnm = this.DataContext as PNM;
            switch(ComboBox.SelectedIndex)
            {
                case 0:
                    return pnm.GetHistogramLuminosity();
                case 1:
                    return pnm.GetHistogramRed();
                case 2:
                    return pnm.GetHistogramGreen();
                case 3:
                    return pnm.GetHistogramBlue();
                default:
                    throw new InvalidOperationException();
            }
        }

        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is PNM && Visibility == Visibility.Visible)
                DrawHistogram();
        }

    }
}
