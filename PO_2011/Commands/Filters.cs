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
        private static RoutedUICommand grayscale = new RoutedUICommand();
        public static RoutedUICommand Convolution { get { return convolution; } }
        public static RoutedUICommand Grayscale { get { return grayscale; } }

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
            private static RoutedUICommand sobel = new RoutedUICommand();
            private static RoutedUICommand prewitt = new RoutedUICommand();
            private static RoutedUICommand roberts = new RoutedUICommand();
            private static RoutedUICommand log = new RoutedUICommand();
            private static RoutedUICommand dog = new RoutedUICommand();
            private static RoutedUICommand zero = new RoutedUICommand();
            private static RoutedUICommand canny = new RoutedUICommand();
            public static RoutedUICommand Laplace { get { return laplace; } }
            public static RoutedUICommand Gradient { get { return gradient; } }
            public static RoutedUICommand Sobel { get { return sobel; } }
            public static RoutedUICommand Prewitt { get { return prewitt; } }
            public static RoutedUICommand Roberts { get { return roberts; } }
            public static RoutedUICommand LaplacianOfGaussian { get { return log; } }
            public static RoutedUICommand DifferenceOfGaussian { get { return dog; } }
            public static RoutedUICommand ZeroCrossing { get { return zero; } }
            public static RoutedUICommand Canny { get { return canny; } }
        }

        public static class Denoise
        {
            private static RoutedUICommand median = new RoutedUICommand();
            public static RoutedUICommand Median { get { return median; } }
        }

        public static class Morphology
        {
            private static RoutedUICommand erosion = new RoutedUICommand();
            private static RoutedUICommand dilation = new RoutedUICommand();
            private static RoutedUICommand opening = new RoutedUICommand();
            private static RoutedUICommand closing = new RoutedUICommand();
            public static RoutedUICommand Erosion { get { return erosion; } }
            public static RoutedUICommand Dilation { get { return dilation; } }
            public static RoutedUICommand Opening { get { return opening; } }
            public static RoutedUICommand Closing { get { return closing; } }
        }

        public static class Thresholding
        {
            private static ThresholdWindow thresholdWindow;

            private static RoutedUICommand plain = new RoutedUICommand();
            private static RoutedUICommand otsu = new RoutedUICommand();
            private static RoutedUICommand triangl = new RoutedUICommand();
            private static RoutedUICommand entropy = new RoutedUICommand();
            private static RoutedUICommand niblack = new RoutedUICommand();
            public static RoutedUICommand Plain { get { return plain; } }
            public static RoutedUICommand Otsu { get { return otsu; } }
            public static RoutedUICommand Triangle { get { return triangl; } }
            public static RoutedUICommand Entropy { get { return entropy; } }
            public static RoutedUICommand Niblack { get { return niblack; } }

            public static void PlainExecuted(Window parent, ExecutedRoutedEventArgs e)
            {
                if (thresholdWindow == null)
                {
                    thresholdWindow = new ThresholdWindow(parent);
                    thresholdWindow.OnClosedOnce = () => thresholdWindow = null;
                }
                thresholdWindow.Show();
                thresholdWindow.Activate();
                e.Handled = true;
            }
        }

        public static class Artistic
        {
            private static RoutedUICommand oil = new RoutedUICommand();
            private static RoutedUICommand fisheye = new RoutedUICommand();
            private static RoutedUICommand mirror = new RoutedUICommand();
            private static RoutedUICommand negative = new RoutedUICommand();
            private static RoutedUICommand emboss = new RoutedUICommand();
            public static RoutedUICommand Oil { get { return oil; } }
            public static RoutedUICommand FishEye { get { return fisheye; } }
            public static RoutedUICommand Mirror { get { return mirror; } }
            public static RoutedUICommand Negative { get { return negative; } }
            public static RoutedUICommand Emboss { get { return emboss; } }
        }

        public static class Mapping
        {
            private static HorizonWindow horizonWindow;

            private static RoutedUICommand normal = new RoutedUICommand();
            private static RoutedUICommand horizon = new RoutedUICommand();
            public static RoutedUICommand Normal { get { return normal; } }
            public static RoutedUICommand Horizon { get { return horizon; } }

            internal static void HorizonExecuted(Window mainWindow, ExecutedRoutedEventArgs e)
            {
                if (horizonWindow == null)
                {
                    horizonWindow = new HorizonWindow(mainWindow);
                    horizonWindow.OnClosedOnce = () => horizonWindow = null;
                }
                horizonWindow.Show();
                horizonWindow.Activate();
                e.Handled = true;
            }
        }

        public static class Lines
        {
            private static RoutedUICommand hough = new RoutedUICommand();
            public static RoutedUICommand Hough { get { return hough; } }
        }
    }
}
