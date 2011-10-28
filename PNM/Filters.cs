using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UAM.PTO
{
    public static class Filters
    {
        public static PNM ApplyConvolution(this PNM image, double[] matrix, double weight, double shift)
        {
            int length = (int)Math.Sqrt(matrix.Length);
            if (Math.Pow(length, 2) != matrix.Length || (length / 2) * 2 == length)
                throw new ArgumentException("matrix");
            PNM newImage = PNM.Copy(image);
            int padding = length / 2;
            Pad(newImage, padding);
            newImage = ApplyConvolutionMatrixCore(newImage, matrix, length, weight, shift);
            Trim(newImage, padding);
            return newImage;
        }

        private static Tuple<float[], float[], float[]> ApplyConvolutionUnbound(PNM image, float[] matrix, int matrixLength)
        {
            int padding = matrixLength / 2;
            int oldHeight = image.Height - (padding * 2);
            int oldWidth = image.Width - (padding * 2);
            Tuple<float[], float[], float[]> rasters = Tuple.Create(new float[oldHeight * oldWidth],
                                                                    new float[oldHeight * oldWidth],
                                                                    new float[oldHeight * oldWidth]);
            int index = 0;
            int maxHeight = image.Height - padding;
            int maxWidth = image.Width - padding;
            for (int i = padding; i < maxHeight; i++)
            {
                for (int j = padding; j < maxWidth; j++)
                {
                    float sumR = 0;
                    float sumG = 0;
                    float sumB = 0;
                    // current index position
                    int position = i * image.Width + j;
                    for (int m = 0; m < matrixLength; m++)
                    {
                        for (int n = 0; n < matrixLength; n++)
                        {
                            byte r, g, b;
                            image.GetPixel(position - ((padding - m) * image.Width) - (padding - n), out r, out g, out b);
                            float coeff = matrix[(m * matrixLength) + n];
                            sumR += r * coeff;
                            sumG += g * coeff;
                            sumB += b * coeff;
                        }
                    }
                    rasters.Item1[index] = sumR;
                    rasters.Item2[index] = sumG;
                    rasters.Item3[index] = sumB;
                    index++;
                }
            }
            return rasters;
        }

        public static PNM ApplyGradientEdgesDetection(this PNM image)
        {
            PNM workImage = PNM.Copy(image);
            Pad(workImage, 1);
            Tuple<float[], float[], float[]> xraster = ApplyConvolutionUnbound(workImage, new float[] {-1, 0, 1,
                                                                                                       -1, 0, 1,
                                                                                                       -1, 0, 1}, 3);
            Tuple<float[], float[], float[]> yraster = ApplyConvolutionUnbound(workImage, new float[] { 1,  1,  1,
                                                                                                        0,  0,  0,
                                                                                                       -1, -1, -1}, 3);
            PNM newImage = new PNM(image.Width, image.Height);
            for (int i = 0; i < xraster.Item1.Length; i++)
            {
                byte r = Coerce(Math.Sqrt(Math.Pow(xraster.Item1[i], 2) + Math.Pow(yraster.Item1[i], 2)));
                byte g = Coerce(Math.Sqrt(Math.Pow(xraster.Item2[i], 2) + Math.Pow(yraster.Item2[i], 2)));
                byte b = Coerce(Math.Sqrt(Math.Pow(xraster.Item3[i], 2) + Math.Pow(yraster.Item3[i], 2)));
                newImage.SetPixel(i, r, g, b);
            }
            return newImage;
        }

        // just pad with black for now
        private static void Pad(PNM image, int padding)
        {
            int newHeight = image.Height + (2 * padding);
            int newWidth = image.Width + (2 * padding);
            byte[] newRaster = new byte[newHeight * newWidth * 3];
            // skip black rows at the top
            int start = padding * newWidth * 3;
            int oldSize = image.Height * image.Width * 3;
            // copy rows
            for (int i_new = start, i_old = 0; i_old < oldSize; i_new += (newWidth * 3), i_old += (image.Width * 3))
            {
                Buffer.BlockCopy(image.raster, i_old, newRaster, i_new + (padding * 3), image.Width * 3);
            }
            image.raster = newRaster;
            image.Width = newWidth;
            image.Height = newHeight;
        }

        private static void Trim(PNM image, int padding)
        {
            int newHeight = image.Height - (2 * padding);
            int newWidth = image.Width - (2 * padding);
            int newSize = newHeight * newWidth * 3;
            int oldSize = image.Width * image.Height * 3;
            byte[] newRaster = new byte[newSize];
            int start = padding * image.Width * 3;
            for (int i_old = start, i_new = 0; i_new < newSize; i_old += (image.Width * 3), i_new += (newWidth * 3))
            {
                Buffer.BlockCopy(image.raster, i_old + (padding * 3), newRaster, i_new, newWidth * 3);
            }
            image.raster = newRaster;
            image.Width = newWidth;
            image.Height = newHeight;
        }

        internal static byte Coerce(double d)
        {
            if (d <= 0)
                return 0;
            else if (d >= 255)
                return 255;
            else
                return Convert.ToByte(d);
        }

        private static PNM ApplyConvolutionMatrixCore(PNM image, double[] matrix, int matrixLength, double weight, double shift)
        {
            PNM newImage = new PNM(image.Width, image.Height);
            int padding = matrixLength / 2;
            int maxHeight = image.Height - padding;
            int maxWidth = image.Width - padding;
            for (int i = padding; i < maxHeight; i++)
            {
                for (int j = padding; j < maxWidth; j++)
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
                            byte r, g, b;
                            image.GetPixel(position - ((padding - m) * image.Width) - (padding - n), out r, out g, out b);
                            double coeff = matrix[(m * matrixLength) + n];
                            sumR += (r * coeff * weight) + shift;
                            sumG += (g * coeff * weight) + shift;
                            sumB += (b * coeff * weight) + shift;
                        }
                    }
                    newImage.SetPixel(position, Coerce(sumR), Coerce(sumG), Coerce(sumB));
                }
            }
            return newImage;
        }

        public static PNM Apply(this PNM oldImage, Func<byte, byte, byte, Pixel> filter)
        {
            PNM newImage = new PNM(oldImage.Width, oldImage.Height);
            byte r,g,b;
            int size = oldImage.Width * oldImage.Height;
            for (int i = 0; i < size; i++)
            {
                oldImage.GetPixel(i, out r, out g, out b);
                Pixel pixel = filter(r, g, b);
                newImage.SetPixel(i, pixel.Red, pixel.Green, pixel.Blue);
            };
            return newImage;
        }

        public static Func<byte, byte, byte, Pixel> HistogramEqualize(PNM pnm)
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

        public static Func<byte, byte, byte, Pixel> HistogramStretch(PNM pnm)
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