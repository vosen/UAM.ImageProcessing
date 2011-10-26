using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UAM.PTO
{
    public static class Filters
    {
        public static PNM ApplyConvolution(this PNM image, double[] matrix, int length)
        {
            PNM newImage = PNM.Copy(image);
            int padding = length / 2;
            Pad(newImage, padding);
            newImage = ApplyConvolutionMatrixCore(newImage, matrix, length);
            Trim(newImage, padding);
            return newImage;
        }

        // just pad with black for now
        private static void Pad(PNM image, int padding)
        {
            int newHeight = image.Height + (2 * padding);
            int newWidth = image.Width + (2 * padding);
            byte[] newRaster = new byte[newHeight * newWidth * 6];
            // skip black rows at the top
            int start = padding * newWidth * 6;
            int oldSize = image.Height * image.Width * 6;
            // copy rows
            for (int i_new = start, i_old = 0; i_old < oldSize; i_new += (newWidth * 6), i_old += (image.Width * 6))
            {
                Buffer.BlockCopy(image.raster, i_old, newRaster, i_new + (padding * 6), image.Width * 6);
            }
            image.raster = newRaster;
            image.Width = newWidth;
            image.Height = newHeight;
        }

        private static void Trim(PNM image, int length)
        {

        }

        private static PNM ApplyConvolutionMatrixCore(PNM image, double[] matrix, int matrixLength)
        {
            PNM newImage = new PNM(image.Width, image.Height);
            int padding = matrixLength / 2;
            int oldHeight = image.Height - (padding * 2);
            int oldWidth = image.Width - (padding * 2);
            for (int i = padding; i < oldHeight; i++)
            {
                for (int j = padding; j < oldWidth; j++)
                {
                    double sumR = 0;
                    double sumG = 0;
                    double sumB = 0;
                    // current index position
                    int position = i * image.Width + j;
                    for (int m = 0; m < matrixLength; m++)
                    {
                        for (int n = 0; n < matrixLength; n++)
                        {
                            ushort r, g, b;
                            image.GetPixel(position - ((padding - m) * image.Width) - (padding - n), out r, out g, out b);
                            double coeff = matrix[(m * matrixLength) + n];
                            sumR += r * coeff;
                            sumG += g * coeff;
                            sumB += b * coeff;
                        }
                    }
                    newImage.SetPixel(position, Convert.ToUInt16(sumR), Convert.ToUInt16(sumG), Convert.ToUInt16(sumB));
                }
            }
            return newImage;
        }

        public static PNM Apply(this PNM oldImage, Func<ushort,ushort,ushort,Tuple<ushort,ushort,ushort>> filter)
        {
            PNM newImage = new PNM(oldImage.Width, oldImage.Height);
            ushort r,g,b;
            int size = oldImage.Width * oldImage.Height;
            for (int i = 0; i < size; i++)
            {
                oldImage.GetPixel(i, out r, out g, out b);
                Tuple<ushort, ushort, ushort> pixel = filter(r, g, b);
                newImage.SetPixel(i, pixel.Item1, pixel.Item2, pixel.Item3);
            };
            return newImage;
        }

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