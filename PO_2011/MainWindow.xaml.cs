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
            BindCommands();
        }

        private void BindCommands()
        {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, (s,e) => Commands.OpenExecuted(image,e) , Commands.CanOpenExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, Commands.SaveExecuted , Commands.CanSaveExecute));
            this.CommandBindings.Add(new CommandBinding(Commands.Exit, Commands.ExitExecuted, Commands.CanExitExecute));
        }
    }
}
