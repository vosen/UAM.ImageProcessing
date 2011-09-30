using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace UAM.PTO
{
    static class Commands
    {
        private static RoutedUICommand exit = new RoutedUICommand();

        public static RoutedUICommand Exit { get { return exit; } }

        internal static void CanOpenExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            CanExecute(e);
        }

        internal static void OpenExecuted(Image source, ExecutedRoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "PNM|*.pbm;*pgm;*ppm";
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                TryReplaceImageSource(source, dialog.OpenFile());
            }
        }

        internal static void CanSaveExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //CanExecute(e);
        }

        internal static void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        internal static void CanExitExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            CanExecute(e);
        }

        internal static void ExitExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private static void CanExecute(CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        internal static void TryReplaceImageSource(Image img, string path)
        {
            Stream stream;
            try
            {
                stream = File.Open(path, FileMode.Open);
            }
            catch (IOException ex)
            {
                MessageBox.Show("Can't open the file\n" + ex.Message, "IO Error", MessageBoxButton.OK);
                return;
            }
            TryReplaceImageSource(img, stream);
        }

        internal static void TryReplaceImageSource(Image img, Stream stream)
        {
            PNM pnm;
            try
            {
                pnm = PNM.LoadFile(stream);
            }
            catch (MalformedFileException)
            {
                MessageBox.Show("Provided file is not a valid image.", "Invalid file", MessageBoxButton.OK);
                return;
            }
            img.Source = BitmapFromPNM(pnm);
            img.DataContext = pnm;
        }

        public static WriteableBitmap BitmapFromPNM(PNM pnm)
        {
            var bitmap = new WriteableBitmap(pnm.Width, pnm.Height, 96, 96, PixelFormats.Rgb48, null);
            bitmap.WritePixels(new Int32Rect(0,0,pnm.Width, pnm.Height), pnm.Raster, pnm.Stride, 0);
            return bitmap;
        }
    }
}
