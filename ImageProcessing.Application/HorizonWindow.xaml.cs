using System;
using System.IO;
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
    /// Interaction logic for CMYKWindow.xaml
    /// </summary>
    public partial class HorizonWindow : ChildWindow
    {
        public HorizonWindow(Window owner)
            : base(owner)
        {
            InitializeComponent();
        }

        private void SaveClicked(object sender, RoutedEventArgs e)
        {
            //
            var dialog = new Microsoft.Win32.SaveFileDialog() { AddExtension = true };
            dialog.Filter = "Portable Network Graphics|*.png";
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                try
                {
                    using(Stream stream = dialog.OpenFile())
                    {
                        var encoder = new TiffBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create((BitmapSource)this.image.Source));
                        encoder.Save(stream);
                    }
                    this.Close();
                }
                catch(SystemException ex)
                {
                    MessageBox.Show("Can't save to the file. " + ex.Message, "File Error", MessageBoxButton.OK);
                }
            }
        }
    }
}
