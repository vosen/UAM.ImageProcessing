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

namespace UAM.PTO
{
    /// <summary>
    /// Interaction logic for ConvolutionWindow.xaml
    /// </summary>
    public partial class ConvolutionWindow : Window
    {
        public ConvolutionWindow()
        {
            InitializeComponent();
            matrixBox.DataContext = new ConvolutionViewModel();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ApplyConvolution(object sender, RoutedEventArgs e)
        {
            ImageViewModel imgvm = DataContext as ImageViewModel;
            ConvolutionViewModel convvm = matrixBox.DataContext as ConvolutionViewModel;
            imgvm.ApplyConvolutionMatrix(convvm.Matrix, convvm.Weight, convvm.Shift);
        }
    }
}
