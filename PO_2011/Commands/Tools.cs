using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Windows.Data;

namespace UAM.PTO.Commands
{
    static class Tools
    {
        private static Lazy<Window> histogramWindow = new Lazy<Window>(() => new HistogramWindow());
        private static RoutedUICommand histogram = new RoutedUICommand();
        public static RoutedUICommand Histogram { get { return histogram; } }

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
    }
}
