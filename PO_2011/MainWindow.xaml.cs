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
            BindFileCommands();
            BindEditCommands();
            BindFiltersCommands();
            BindToolsCommands();
        }

        private void BindFileCommands()
        {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, (s, e) => Commands.File.OpenExecuted(imgvm, e)));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, (s, e) => Commands.File.SaveExecuted(imgvm, e), (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, (s, e) => Commands.File.SaveAsExecuted(imgvm, e), (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.File.Exit, Commands.File.ExitExecuted));
        }

        private void BindEditCommands()
        {

            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, (s, e) => { imgvm.Undo(); }, (s, e) => { e.CanExecute = imgvm.CanUndo; }));
        }

        private void BindFiltersCommands()
        {
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Convolution, (s, e) => Commands.Filters.ConvolutionExecuted(this, e), (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Blur.Gaussian, (s, e) => { imgvm.ApplyGaussianBlur(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Blur.Uniform, (s, e) => { imgvm.ApplyUniformBlur(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Histogram.Equalize, (s, e) => { imgvm.EqualizeHistogram(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Histogram.Stretch, (s, e) => { imgvm.StretchHistogram(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Edges.Gradient, (s, e) => { imgvm.DetectEdgesGradient(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Edges.Laplace, (s, e) => { imgvm.DetectEdgesLaplace(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
        }

        private void BindToolsCommands()
        {
            this.CommandBindings.Add(new CommandBinding(Commands.Tools.Histogram, (s, e) => Commands.Tools.HistogramExecuted(this, e), (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                Commands.File.OpenExecutedInternal(imgvm, ((string[])e.Data.GetData(DataFormats.FileDrop))[0]);
            }
            e.Handled = true;
        }

    }
}
