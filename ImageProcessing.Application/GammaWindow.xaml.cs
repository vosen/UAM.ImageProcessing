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
    /// Interaction logic for GammaWindow.xaml
    /// </summary>
    public partial class GammaWindow : ChildWindow
    {
        public GammaWindow(Window parent)
            : base(parent)
        {
            InitializeComponent();
        }

        private void ApplyGamma(object sender, RoutedEventArgs e)
        {
            ImageViewModel imgvm = (ImageViewModel)DataContext;
            imgvm.ChangeGamma((float)gammaSlider.Value);
            Close();
        }
    }
}
