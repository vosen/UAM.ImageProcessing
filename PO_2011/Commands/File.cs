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

namespace UAM.PTO.Commands
{
    public static class File
    {
        private static RoutedUICommand exit = new RoutedUICommand();
        public static RoutedUICommand Exit { get { return exit; } }

        internal static void OpenExecuted(ImageViewModel source, ExecutedRoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog() { AddExtension = true };
            dialog.Filter = "Portable anymap format |*.pbm;*.pgm;*.ppm";
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                OpenExecutedInternal(source, dialog.FileName);
            }
            e.Handled = true;
        }

        internal static void OpenExecutedInternal(ImageViewModel source, string path)
        {
            try
            {
                source.ReplaceImage(path);
            }
            catch (MalformedFileException)
            {
                MessageBox.Show("Provided file is not a valid image.", "Invalid file", MessageBoxButton.OK);
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Can't open the file. " + ex.Message, "File Error", MessageBoxButton.OK);
            }
        }

        internal static void SaveExecuted(ImageViewModel source, ExecutedRoutedEventArgs e)
        {
            try
            {
                source.SaveImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't save the file. " + ex.Message, "Error", MessageBoxButton.OK);
                return;
            }
            e.Handled = true;
        }

        internal static void SaveAsExecuted(ImageViewModel source, ExecutedRoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog() { AddExtension = true };
            dialog.Filter = "Portable bitmap format|*.pbm|Portable graymap format|*.pgm|Portable pixmap format|*.ppm";
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                try
                {
                    source.SaveImage(dialog.FileName, (PNMFormat)(dialog.FilterIndex));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Can't save the file\n" + ex.Message, "Error", MessageBoxButton.OK);
                    return;
                }
            }

            e.Handled = true;
        }

        internal static void ExitExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
