using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace UAM.PTO.Commands
{

    public static class Filters
    {
        private static RoutedUICommand convolution = new RoutedUICommand();
        public static RoutedUICommand Convolution { get { return convolution; } }

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
    }
}
