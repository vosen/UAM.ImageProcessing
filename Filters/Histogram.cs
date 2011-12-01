using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UAM.PTO.Filters
{
    public static class Histogram
    {

        public static Func<byte, byte, byte, Pixel> Equalize(PNM pnm)
        {
            double[] rawDataR = pnm.GetHistogramRed();
            double[] rawDataG = pnm.GetHistogramGreen();
            double[] rawDataB = pnm.GetHistogramBlue();
            double[] rlum = new double[256];
            double[] glum = new double[256];
            double[] blum = new double[256];
            double cumulatedR = 0;
            double cumulatedG = 0;
            double cumulatedB = 0;
            for (int i = 0; i < 256; i++)
            {
                cumulatedR += rawDataR[i];
                cumulatedG += rawDataG[i];
                cumulatedB += rawDataB[i];
                rlum[i] = cumulatedR;
                glum[i] = cumulatedG;
                blum[i] = cumulatedB;
            }
            return (r, g, b) =>
            {
                return new Pixel(Convert.ToByte(rlum[r] * 255), Convert.ToByte(glum[g] * 255), Convert.ToByte(blum[b] * 255));
            };
        }

        public static Func<byte, byte, byte, Pixel> Stretch(PNM pnm)
        {
            double[] histogramR = pnm.GetHistogramRed();
            double[] histogramG = pnm.GetHistogramGreen();
            double[] histogramB = pnm.GetHistogramBlue();
            double[] cumulativeR = CumulativeHistogram(histogramR);
            double[] cumulativeG = CumulativeHistogram(histogramG);
            double[] cumulativeB = CumulativeHistogram(histogramB);

            Tuple<int, int> rangeR = FindPercentiles(cumulativeR);
            Tuple<int, int> rangeG = FindPercentiles(cumulativeG);
            Tuple<int, int> rangeB = FindPercentiles(cumulativeB);

            int[] LUTR = GetStretchLUT(histogramR, rangeR.Item1, rangeR.Item2);
            int[] LUTG = GetStretchLUT(histogramG, rangeG.Item1, rangeG.Item2);
            int[] LUTB = GetStretchLUT(histogramB, rangeB.Item1, rangeB.Item2);

            return (r, g, b) =>
            {
                return new Pixel(Convert.ToByte(LUTR[r]), Convert.ToByte(LUTG[g]), Convert.ToByte(LUTB[b]));
            };
        }

        private static Tuple<int, int> FindPercentiles(double[] cumulative)
        {
            int start = -1;
            int stop = -1;
            for (int i = 0; i < 256; i++)
            {
                if (start == -1 && cumulative[i] >= 0.005)
                    start = i;
                if (stop == -1 && cumulative[i] >= 0.995)
                {
                    stop = i;
                    break;
                }
            }
            return Tuple.Create(start, stop);
        }

        private static double[] CumulativeHistogram(double[] histogram)
        {
            double[] cumulative = new double[256];
            double cumulated = 0;
            for (int i = 0; i < 256; i++)
            {
                cumulated += histogram[i];
                cumulative[i] = cumulated;
            }
            return cumulative;
        }

        private static int[] GetStretchLUT(double[] histogram, int start, int end)
        {
            int[] stretchLUT = new int[256];
            double stretch = (double)256 / (double)((end - start) + 1);
            for (int i = start; i <= end; i++)
            {
                stretchLUT[i] = Convert.ToInt32((i - start) * stretch);
            }
            for (int i = end + 1; i < 256; i++)
            {
                stretchLUT[i] = 255;
            }
            return stretchLUT;
        }
    }
}
