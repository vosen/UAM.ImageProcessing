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
        private static HistogramWindow histogramWindow;
        private static BrightnessContrastWindow bcWindow;
        private static GammaWindow gammaWindow;
        private static CMYKWindow cmykWindow;

        private static RoutedUICommand histogram = new RoutedUICommand();
        private static RoutedUICommand bc = new RoutedUICommand();
        private static RoutedUICommand gamma = new RoutedUICommand();
        private static RoutedUICommand cmyk = new RoutedUICommand();
        public static RoutedUICommand Histogram { get { return histogram; } }
        public static RoutedUICommand BrightnessContrast { get { return bc; } }
        public static RoutedUICommand Gamma { get { return gamma; } }
        public static RoutedUICommand CMYK { get { return cmyk; } }

        internal static void HistogramExecuted(Window parent, ExecutedRoutedEventArgs e)
        {
            if (histogramWindow == null)
            {
                histogramWindow = new HistogramWindow(parent);
            }
            histogramWindow.Show();
            histogramWindow.Activate();
            e.Handled = true;
        }

        internal static void BrightnessContrastExecuted(Window mainWindow, ExecutedRoutedEventArgs e)
        {
            if (bcWindow == null)
            {
                bcWindow = new BrightnessContrastWindow(mainWindow);
                bcWindow.OnClosedOnce = () => bcWindow = null;
            }
            bcWindow.Show();
            bcWindow.Activate();
            e.Handled = true;
        }

        internal static void GammaExecuted(Window mainWindow, ExecutedRoutedEventArgs e)
        {
            if (gammaWindow == null)
            {
                gammaWindow = new GammaWindow(mainWindow);
                gammaWindow.OnClosedOnce = () => gammaWindow = null;
            }
            gammaWindow.Show();
            gammaWindow.Activate();
            e.Handled = true;
        }

        internal static void CMYKExecuted(Window mainWindow, ExecutedRoutedEventArgs e)
        {
            if (cmykWindow == null)
            {
                cmykWindow = new CMYKWindow(mainWindow);
                cmykWindow.OnClosedOnce = () => cmykWindow = null;
            }
            cmykWindow.Show();
            cmykWindow.Activate();
            e.Handled = true;
        }
    }
}
