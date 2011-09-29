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
using IMP;

namespace Proj1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PBM pbm;
            using (var reader = new StreamReader("test.txt"))
            {
                pbm = new PBM(reader);
            }

            var bitmap = new WriteableBitmap(pbm.Width, pbm.Height, 96, 96, PixelFormats.Rgb24, null);
            bitmap.WritePixels(FullRect(pbm),pbm.Bitmap, pbm.Stride,0);
            var image = new Image() { Source = bitmap, Width = 6, Height = 10, UseLayoutRounding = true };
            panel.Children.Add(image);
        }

        private Int32Rect FullRect(PBM pbm)
        {
            return new Int32Rect(0, 0, pbm.Width, pbm.Height);
        }
    }
}
