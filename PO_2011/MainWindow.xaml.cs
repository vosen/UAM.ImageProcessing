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
        private ImageViewModel imgvm = new ImageViewModel();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = imgvm;
            BindCommands();
        }

        private void BindCommands()
        {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, (s, e) => Commands.OpenExecuted(imgvm, e), Commands.CanOpenExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, (s, e) => Commands.SaveExecuted(imgvm, e), (s, e) => Commands.CanSaveExecute(image, e)));
            this.CommandBindings.Add(new CommandBinding(Commands.Exit, Commands.ExitExecuted, Commands.CanExitExecute));
            this.CommandBindings.Add(new CommandBinding(Commands.Histogram, (s, e) => Commands.HistogramExecuted(this, e), (s, e) => Commands.CanHistogramExecute(image, e)));
            this.CommandBindings.Add(new CommandBinding(Commands.BlurGaussian, (s,e) => Commands.BlurGaussianExecuted(imgvm, e), (s,e) => Commands.CanBlurGaussianExecute(imgvm,e)));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, (s, e) => { imgvm.Undo(); }, (s, e) => { e.CanExecute = imgvm.CanUndo; }));
            this.CommandBindings.Add(new CommandBinding(Commands.BlurUniform, (s,e) => { imgvm.ApplyUniformBlur(); }, (s,e) => { e.CanExecute = imgvm.IsImageOpen; }));
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                Commands.TryReplaceImageSource(imgvm, ((string[])e.Data.GetData(DataFormats.FileDrop))[0]);
            }
            e.Handled = true;
        }

    }
}
