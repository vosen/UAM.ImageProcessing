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
using System.Windows.Data;

namespace UAM.PTO
{
    static class Commands
    {
        private static Lazy<Window> histogramWindow = new Lazy<Window>(() => new HistogramWindow());

        private static RoutedUICommand exit = new RoutedUICommand();
        private static RoutedUICommand histogram = new RoutedUICommand();

        public static RoutedUICommand Exit { get { return exit; } }
        public static RoutedUICommand Histogram { get { return histogram; } }

        internal static void CanHistogramExecute(Image source, CanExecuteRoutedEventArgs e)
        {
            CanExecuteIfImageOpen(source, e);
        }

        internal static void HistogramExecuted(FrameworkElement parent, ExecutedRoutedEventArgs e)
        {
            if (!histogramWindow.IsValueCreated)
            {
                Binding context = new Binding("DataContext") { Source = parent };
                BindingOperations.SetBinding(histogramWindow.Value, HistogramWindow.DataContextProperty, context);
                histogramWindow.Value.Owner = Application.Current.MainWindow;
            }
            histogramWindow.Value.Show();
            histogramWindow.Value.Activate();
            e.Handled = true;
        }

        internal static void CanOpenExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            CanExecute(e);
        }

        internal static void OpenExecuted(ImageViewModel source, ExecutedRoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog() { AddExtension = true };
            dialog.Filter = "Portable anymap format |*.pbm;*pgm;*ppm";
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                TryReplaceImageSource(source, dialog.OpenFile());
            }
            e.Handled = true;
        }

        internal static void CanSaveExecute(Image source, CanExecuteRoutedEventArgs e)
        {
            CanExecuteIfImageOpen(source, e);
        }

        internal static void SaveExecuted(ImageViewModel source, ExecutedRoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog() { AddExtension = true };
            dialog.Filter = "Portable bitmap format|*.pbm|Portable graymap format|*pgm|Portable pixmap format|*ppm";
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                TrySaveImageFromSource(source.Image, dialog.FileName, (PNMFormat)(dialog.FilterIndex - 1));
            }
            e.Handled = true;
        }

        private static void TrySaveImageFromSource(PNM image, string path, PNMFormat format)
        {
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

        internal static void TryReplaceImageSource(ImageViewModel img, string path)
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

        internal static void TryReplaceImageSource(ImageViewModel img, Stream stream)
        {
            PNM pnm;
            try
            {
                pnm = PNM.LoadFile(stream);
                img.Image = pnm;
            }
            catch (MalformedFileException)
            {
                MessageBox.Show("Provided file is not a valid image.", "Invalid file", MessageBoxButton.OK);
            }
        }

        private static void CanExecuteIfImageOpen(Image img, CanExecuteRoutedEventArgs e)
        {
            ImageViewModel model = img.DataContext as ImageViewModel;
            if (model != null && model.Image != null)
                CanExecute(e);
        }
    }
}
