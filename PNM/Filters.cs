using System;
using System.Threading.Tasks;

namespace UAM.PTO
{
    public static class Filters
    {
        private static float[] Sobel1 = { -1, 0, 1,
                                          -2, 0, 2,
                                          -1, 0, 1};
        private static float[] Sobel2 = {  1,  2,  1,
                                           0,  0,  0,
                                          -1, -2, -1};
        private static float[] Prewitt1 = { -1, 0, 1,
                                            -1, 0, 1,
                                            -1, 0, 1};
        private static float[] Prewitt2 = {  1,  1,  1,
                                             0,  0,  0,
                                            -1, -1, -1};
        private static float[] Roberts1 = { 0,  0,  1,
                                            0, -1,  0,
                                            0,  0,  0};
        private static float[] Roberts2 = { 0, 1,  0,
                                            0, 0, -1,
                                            0, 0,  0};

        public static Pixel ColorToGrayscale(byte r, byte g, byte b)
        {
            byte value = PNM.RGBToLuminosity(r, g, b);
            return new Pixel(value, value, value);
        }

        // brightness should fall in [-1..1] range and contrast should fall in [-1..1] range
        public static Func<byte, byte, byte, Pixel> ChangeBrightnessContrast(float brigthness, float contrast)
        {
            if (brigthness < -1 || brigthness > 1)
                throw new ArgumentOutOfRangeException("brightness");
            if (contrast < -1 || contrast > 1)
                throw new ArgumentOutOfRangeException("contrast");
            // dark magic here
            if (contrast <= 0)
            {
                float realContrast = contrast + 1;
                // could be optimized by precalculating LUT
                return (r, g, b) =>
                {
                    return new Pixel(Coerce((r - 127.5) * realContrast + (brigthness * 127) + 127.5),
                                     Coerce((g - 127.5) * realContrast + (brigthness * 127) + 127.5),
                                     Coerce((b - 127.5) * realContrast + (brigthness * 127) + 127.5));
                };
            }
            else
            {

                byte[] LUT = BuildBrightnessContrastLUT(brigthness, contrast);
                return (r, g, b) =>
                {
                    return new Pixel(LUT[r], LUT[g], LUT[b]);
                };
            }
        }

        private static byte[] BuildBrightnessContrastLUT(float brightness, float contrast)
        {
            float realBright = brightness * 127;
            float factor = 1 - contrast;
            byte lower = (byte)((contrast/2) * 255);
            byte upper = (byte)((1 - contrast/2) * 255);
            byte[] lut = new byte[256];
            float segment = lower == upper ? 0 : 255f / (upper - lower);
            byte minEnergy = Coerce(realBright);
            for (int i = 0; i < lower; i++)
            {
                lut[i] = minEnergy;
            }
            for (int i = lower, k = 0; i <= upper; i++, k++)
            {
                lut[i] = Coerce((k * segment) + realBright);
            }
            for (int i = upper + 1; i < 256; i++)
            {
                lut[i] = 255;
            }
            return lut;
        }

        public static Func<byte, byte, byte, Pixel> ChangeGamma(float weight)
        {
            return (r, g, b) =>
            {
                return new Pixel(Coerce(Math.Pow(r / 255d, weight) * 255), Coerce(Math.Pow(g / 255d, weight) * 255), Coerce(Math.Pow(b / 255d, weight) * 255));
            };
        }

        public static PNM ApplyConvolution(this PNM image, float[] matrix, float weight, float shift)
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
            int maxHeight = image.Height - padding;
            int maxWidth = image.Width - padding;
            Parallel.For(padding, maxHeight, i =>
            {
                int index = (i - padding) * oldWidth;
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
            });
            return rasters;
        }

        public static PNM ApplyConvolutionFunction(this PNM image, int matrixLength, Func<PNM, int, Pixel> func)
        {
            PNM newImage = PNM.Copy(image);
            int padding = matrixLength / 2;
            Pad(newImage, padding);
            newImage = ApplyConvolutionFunctionCore(newImage, matrixLength, func);
            Trim(newImage, padding);
            return newImage;
        }

        // poor man's pixel shader
        public static PNM ApplyConvolutionFunctionCore(PNM image, int matrixLength, Func<PNM, int, Pixel> func)
        {
            PNM newImage = new PNM(image.Width, image.Height);
            int padding = matrixLength / 2;
            int maxHeight = image.Height - padding;
            int maxWidth = image.Width - padding;
            int width = image.Width;
            Parallel.For(padding, maxHeight, i =>
            {
                for (int j = padding; j < maxWidth; j++)
                {
                    // current index position
                    int position = i * width + j;
                    newImage.SetPixel(position, func(image, position));
                }
            });
            return newImage;
        }

        private static double Module(float g1, float g2)
        {
            return Math.Sqrt((g1 * g1) + (g2 * g2));
        }

        public static Pixel Sobel(PNM image, int index)
        {
            return ConvoluteWithModule(image, index, Sobel1, Sobel2,3,1);
        }

        public static Pixel Prewitt(PNM image, int index)
        {
            return ConvoluteWithModule(image, index, Prewitt1, Prewitt2,3,1);
        }

        public static Pixel Roberts(PNM image, int index)
        {
            return ConvoluteWithModule(image, index, Roberts1, Roberts2, 3, 1);
        }

        private static Pixel ConvoluteWithModule(PNM image, int index, float[] matrix1, float[] matrix2, int length, int padding)
        {
            float sumR1 = 0;
            float sumR2 = 0;
            float sumG1 = 0;
            float sumG2 = 0;
            float sumB1 = 0;
            float sumB2 = 0;
            byte r, g, b;
            for (int m = 0; m < length; m++)
            {
                for (int n = 0; n < length; n++)
                {
                    image.GetPixel(index - ((padding - m) * image.Width) - (padding - n), out r, out g, out b);
                    float coeff1 = matrix1[(m * length) + n];
                    float coeff2 = matrix2[(m * length) + n];
                    sumR1 += r * coeff1;
                    sumR2 += r * coeff2;
                    sumG1 += g * coeff1;
                    sumG2 += g * coeff2;
                    sumB1 += b * coeff1;
                    sumB2 += b * coeff2;
                }
            }
            return new Pixel(Coerce(Module(sumR1, sumR2)),
                             Coerce(Module(sumG1, sumG2)),
                             Coerce(Module(sumB1, sumB2)));
        }

        public static Pixel SingleConvolutionMatrix(PNM image, int index, float[] matrix, int matrixLength, float weight, float shift)
        {
            int padding = matrixLength / 2;
            float sumR = 0;
            float sumG = 0;
            float sumB = 0;
            byte r, g, b;
            for (int m = 0; m < matrixLength; m++)
            {
                for (int n = 0; n < matrixLength; n++)
                {
                    image.GetPixel(index - ((padding - m) * image.Width) - (padding - n), out r, out g, out b);
                    float coeff = matrix[(m * matrixLength) + n];
                    sumR += (r * coeff * weight) + shift;
                    sumG += (g * coeff * weight) + shift;
                    sumB += (b * coeff * weight) + shift;
                }
            }
            return new Pixel(Coerce(sumR), Coerce(sumG), Coerce(sumB));
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
            int size = image.Width * image.Height;
            Parallel.For(0, size, i =>
            {
                byte r = Coerce(Math.Sqrt(Math.Pow(xraster.Item1[i], 2) + Math.Pow(yraster.Item1[i], 2)));
                byte g = Coerce(Math.Sqrt(Math.Pow(xraster.Item2[i], 2) + Math.Pow(yraster.Item2[i], 2)));
                byte b = Coerce(Math.Sqrt(Math.Pow(xraster.Item3[i], 2) + Math.Pow(yraster.Item3[i], 2)));
                newImage.SetPixel(i, r, g, b);
            });
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

        internal static byte Coerce(float f)
        {
            if (f <= 0)
                return 0;
            else if (f >= 255)
                return 255;
            else
                return Convert.ToByte(f);
        }

        internal static byte Coerce(double f)
        {
            if (f <= 0)
                return 0;
            else if (f >= 255)
                return 255;
            else
                return Convert.ToByte(f);
        }

        private static PNM ApplyConvolutionMatrixCore(PNM image, float[] matrix, int matrixLength, float weight, float shift)
        {
            PNM newImage = new PNM(image.Width, image.Height);
            int padding = matrixLength / 2;
            int maxHeight = image.Height - padding;
            int maxWidth = image.Width - padding;
            int width = image.Width;
            Parallel.For(padding, maxHeight, i =>
            {
                for (int j = padding; j < maxWidth; j++)
                {
                    float sumR = 0;
                    float sumG = 0;
                    float sumB = 0;
                    // current index position
                    int position = i * width + j;
                    for (int m = 0; m < matrixLength; m++)
                    {
                        for (int n = 0; n < matrixLength; n++)
                        {
                            byte r, g, b;
                            image.GetPixel(position - ((padding - m) * width) - (padding - n), out r, out g, out b);
                            float coeff = matrix[(m * matrixLength) + n];
                            sumR += (r * coeff * weight) + shift;
                            sumG += (g * coeff * weight) + shift;
                            sumB += (b * coeff * weight) + shift;
                        }
                    }
                    newImage.SetPixel(position, Coerce(sumR), Coerce(sumG), Coerce(sumB));
                }
            });
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