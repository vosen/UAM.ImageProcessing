using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAM.PTO.Filters
{
    public static class Thresholding
    {
        // pixel_intensity >= threshold == white object
        // pixel_intensity < threshold == black background
        public static Pixel Plain(byte r, byte g, byte b, byte threshold)
        {
            byte lum = PNM.RGBToLuminosity(r,g,b);
            if (lum >= threshold)
                return Pixel.White;
            return Pixel.Black;
        }

        public static Func<byte, byte, byte, Pixel> Otsu(PNM image)
        {
            double[] histogram = image.GetHistogramLuminosity();
            object compareLock = new object();
            double[] variances = new double[256];
            Parallel.For(0, 256, i =>
            {
                double[] background = histogram.Take(i + 1).ToArray();
                double[] foreground = histogram.Skip(i + 1).ToArray();
                double backgroundWeight = background.Sum();
                double foregroundWeight = 1 - backgroundWeight;
                double backgroundMean = background.Select((d, idx) => d * idx).Sum();
                double foregroundMean = foreground.Select((d, idx) => d * idx).Sum();
                double variance = backgroundWeight * foregroundWeight * Math.Pow(foregroundMean - backgroundMean, 2);
                variances[i] = variance;
            });
            byte threshold = 0;
            double maxVariance = Double.NegativeInfinity;
            for(int i =0; i< 256;i++)
            {
                if (variances[i] > maxVariance)
                {
                    maxVariance = variances[i];
                    threshold = (byte)i;
                }
            }

            return (r, g, b) => Plain(r, g, b, threshold);
        }

        public static Func<byte, byte, byte, Pixel> Triangle(PNM image)
        {
            double[] histogram = image.GetHistogramLuminosity();
            int minx=0, maxx=0;
            double minValue = Double.PositiveInfinity, maxValue = Double.NegativeInfinity;
            // find min max
            for (int i = 0; i < 256; i++)
            {
                if (histogram[i] >= maxValue)
                {
                    maxValue = histogram[i];
                    maxx = i;
                }
                if (histogram[i] < minValue)
                {
                    minValue = histogram[i];
                    minx = i;
                }
            }
            // find line
            double a = -(maxValue - minValue) / (maxx - minx);
            double b = 1;
            double c = (minx * (maxValue - minValue) / (maxx - minx)) - minValue;

            // find the furthest point
            double distance, maxDistance = Double.NegativeInfinity;
            byte threshold = 0;
            int from, to;
            if (minx < maxx)
            {
                from = minx + 1;
                to = maxx;
            }
            else
            {
                to = minx + 1;
                from = maxx;
            }
            for(int i = from; i< to; i++)
            {
                distance = PointToLineDistance(a,b,c,i,histogram[i]);
                if (distance >= maxDistance)
                {
                    maxDistance = distance;
                    threshold = (byte)i;
                }
            }
            return (pr, pg, pb) => Plain(pr, pg, pb, threshold);
        }

        private static double PointToLineDistance(double A, double B, double C, double x, double y)
        {
            return Math.Abs(A * x + B * y + C) / Math.Sqrt((A * A) + (B * B));
        }

        // Kapur's method
        public static Func<byte, byte, byte, Pixel> Entropy(PNM image)
        {
            double[] histogram = image.GetHistogramLuminosity();
            double entropySum = Double.NegativeInfinity;
            int threshold = 0;
            for (int i = 0; i < 256; i++)
            {
                double[]  background = histogram.Take(i + 1).ToArray();
                double[]  foreground = histogram.Skip(i + 1).ToArray();
                double sumB = background.Sum();
                double sumF = foreground.Sum();
                double entropyB = -background.Select(p =>
                    {
                        if(p == 0)
                            return 0;
                        return (p / sumB) * Math.Log(p / sumB, 2);
                    })
                    .Sum();
                double entropyF = -foreground.Select(p => 
                    {
                        if(p == 0)
                            return 0;
                        return (p / (1 - sumB)) * Math.Log(p / (1 - sumB), 2);
                    })
                    .Sum();
                double sum = entropyB + entropyF;
                if (sum > entropySum)
                {
                    entropySum = sum;
                    threshold = i;
                }
            }

            return (r, g, b) => Plain(r, g, b, (byte)threshold);
        }

        // assume k = -0.2 and R = 15x15
        // needs 1 pixel width padding
        public static Pixel Niblack(PNM image, int index, double k = -0.2, int length = 15, int padding = 7)
        {
            byte r, g, b;
            float[] surrounding = new float[length * length];
            int i = 0;
            for (int m = 0; m < length; m++)
            {
                for (int n = 0; n < length; n++)
                {
                    image.GetPixel(index - ((padding - m) * image.Width) - (padding - n), out r, out g, out b);
                    surrounding[i] = PNM.RGBToLuminosity(r, g, b);
                    i++;
                }
            }
            float mean = surrounding.Average();
            double threshold = mean + (k* (mean / surrounding.Length));
            image.GetPixel(index, out r, out g, out b);
            byte luminosity = PNM.RGBToLuminosity(r,g,b);
            if (luminosity < threshold)
                return Pixel.Black;
            return Pixel.White;
        }
    }
}
