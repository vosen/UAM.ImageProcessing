using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Windows.Data;

namespace UAM.PTO.Commands
{

    public static class Filters
    {
        private static ConvolutionWindow convolutionWindow;
        private static RoutedUICommand convolution = new RoutedUICommand();
        public static RoutedUICommand Convolution { get { return convolution; } }

        internal static void ConvolutionExecuted(Window parent, ExecutedRoutedEventArgs e)
        {
            if (convolutionWindow == null)
            {
                convolutionWindow = new ConvolutionWindow(parent);
                convolutionWindow.OnClosedOnce = () => convolutionWindow = null;
            }
            convolutionWindow.Show();
            convolutionWindow.Activate();
            e.Handled = true;
        }

        public static class Histogram
        {
            private static RoutedUICommand equalize = new RoutedUICommand();
            private static RoutedUICommand stretch = new RoutedUICommand();
            public static RoutedUICommand Equalize { get { return equalize; } }
            public static RoutedUICommand Stretch { get { return stretch; } }
        }

        public static class Blur
        {
            private static RoutedUICommand uniform = new RoutedUICommand();
            private static RoutedUICommand gaussian = new RoutedUICommand();
            public static RoutedUICommand Uniform { get { return uniform; } }
            public static RoutedUICommand Gaussian { get { return gaussian; } }
        }

        public static class Edges
        {
            private static RoutedUICommand laplace = new RoutedUICommand();
            private static RoutedUICommand gradient = new RoutedUICommand();
            public static RoutedUICommand Laplace { get { return laplace; } }
            public static RoutedUICommand Gradient { get { return gradient; } }
        }
    }
}
