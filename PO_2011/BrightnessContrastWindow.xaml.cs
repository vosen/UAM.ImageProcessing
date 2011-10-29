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
    /// Interaction logic for BrightnessContrastWindow.xaml
    /// </summary>
    public partial class BrightnessContrastWindow : ChildWindow
    {
        public BrightnessContrastWindow(Window owner)
            : base(owner)
        {
            InitializeComponent();
        }

        private void ApplyBrightnessContrast(object sender, RoutedEventArgs e)
        {
            ImageViewModel imgvm = DataContext as ImageViewModel;
            float brightness = (float)(brightnessSlider.Value / 127);
            float contrast = (float)(contrastSlider.Value  / 127);
            imgvm.ChangeBrightnessContrast(brightness, contrast);
            Close();
        }
    }
}
