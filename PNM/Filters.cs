using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UAM.PTO
{
    public static class Filters
    {
        public static Func<ushort,ushort,ushort,Tuple<ushort,ushort,ushort>> HistogramEqualize(PNM pnm)
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
                    return Tuple.Create(Convert.ToUInt16(rlum[r / 256] * 65535), Convert.ToUInt16(glum[g / 256] * 65535), Convert.ToUInt16(blum[b / 256] * 65535));
                };
        }

        public static Func<ushort, ushort, ushort, Tuple<ushort, ushort, ushort>> HistogramStretch(PNM pnm)
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
                return Tuple.Create(Convert.ToUInt16(LUTR[r / 256] * 256), Convert.ToUInt16(LUTG[g / 256] * 256), Convert.ToUInt16(LUTB[b / 256] * 256));
            };
        }

        private static Tuple<int, int> FindPercentiles(double[] cumulative)
        {
            int start = -1;
            int stop = -1;
            for(int i =0; i< 256; i++)
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

        private static int[] GetStretchLUT(double[] histogram, int start, int end)
        {
            int[] stretchLUT = new int[256];
            double stretch = (double)256 / (double)((end - start) + 1);
            for (int i = start; i <= end; i++)
            {
                stretchLUT[i] = Convert.ToInt32((i - start) * stretch);
            }
            for (int i = end +1; i < 256; i++)
            {
                stretchLUT[i] = 255;
            }
            return stretchLUT;
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
    }
}