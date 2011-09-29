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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace UAM.PTO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PNM pnm = PNM.LoadFile("test.txt");

            var bitmap = new WriteableBitmap(pnm.Width, pnm.Height, 96, 96, PixelFormats.Rgb24, null);
            bitmap.WritePixels(FullRect(pnm),pnm.Bitmap, pnm.Stride,0);
            var image = new Image() { Source = bitmap, Width = bitmap.Width, Height = bitmap.Height, UseLayoutRounding = true };
            panel.Children.Add(image);
        }

        private Int32Rect FullRect(PNM pnm)
        {
            return new Int32Rect(0, 0, pnm.Width, pnm.Height);
        }
    }
}
