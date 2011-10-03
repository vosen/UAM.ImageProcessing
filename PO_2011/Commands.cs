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
            var dialog = new Microsoft.Win32.OpenFileDialog() { AddExtension = true };
            dialog.Filter = "Portable anymap format |*.pbm;*pgm;*ppm";
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                TryReplaceImageSource(source, dialog.OpenFile());
            }
        }

        internal static void CanSaveExecute(Image source, CanExecuteRoutedEventArgs e)
        {
            PNM pnm = source.DataContext as PNM;
            if(pnm != null)
                CanExecute(e);
        }

        internal static void SaveExecuted(Image source, ExecutedRoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog() { AddExtension = true };
            dialog.Filter = "Portable bitmap format|*.pbm|Portable graymap format|*pgm|Portable pixmap format|*ppm";
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                TrySaveImageFromSource(source, dialog.FileName, (PNMFormat)(dialog.FilterIndex - 1));
            }
        }

        private static void TrySaveImageFromSource(Image source, string path, PNMFormat format)
        {
            PNM image = (PNM)source.DataContext;
            try
            {
                PNM.SaveFile(image, path, format);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Can't save the file\n" + ex.Message, "Error", MessageBoxButton.OK);
                return;
            }
            MessageBox.Show("File " + path + " saved successfully." , "File saved", MessageBoxButton.OK);
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
