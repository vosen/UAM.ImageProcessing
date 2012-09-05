using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;

namespace UAM.PTO
{
    public class ChildWindow : Window
    {
        public ChildWindow()
            : base()
        {
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            SizeToContent= SizeToContent.WidthAndHeight; 
            ResizeMode = System.Windows.ResizeMode.NoResize;
            ShowActivated = true;
            ShowInTaskbar = false;
        }

        public ChildWindow(Window owner)
            : this()
        {
            Binding context = new Binding("DataContext") { Source = owner };
            BindingOperations.SetBinding(this, ConvolutionWindow.DataContextProperty, context);
            this.Owner = owner;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (OnClosedOnce != null)
                OnClosedOnce();
            OnClosedOnce = null;
        }

        protected void CloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public Action OnClosedOnce { get; set; }
    }
}
