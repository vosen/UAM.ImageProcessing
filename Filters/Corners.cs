using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UAM.PTO.Filters;
using System.Threading.Tasks;

namespace UAM.PTO
{
    public static class Corners
    {
        private static float[] weirdY = new float[] { 0,  0.5f, 0,
                                                      0,     0, 0,
                                                      0, -0.5f, 0};
        private static float[] weirdX = new float[] {    0, 0,     0,
                                                      0.5f, 0, -0.5f,
                                                         0, 0,     0};
        private static double HarrisK = 0.05;
        private static double HarrisThreshold = 0.001;

        public static PNM ApplyHarrisDetector(this PNM image)
        {
            // gaussian window
            float[] gaussian = new float[]{     0, 0.01F, 0.02F, 0.01F,    0,
                                            0.01F, 0.06F,  0.1F, 0.06F, 0.01F,
                                            0.02F,  0.1F, 0.16F,  0.1F, 0.02F,
                                            0.01F, 0.06F,  0.1F, 0.06F, 0.01F,
                                                0, 0.01F, 0.02F, 0.01F,    0};
            // greyscale
            PNM workImage = PNM.Copy(image).ApplyPointProcessing(Color.ToGrayscale);
            Filter.Pad(workImage, 1);
            // integrate with prewitt
            double[] normalizedWorkImage = workImage.raster.Where((b, idx) => idx % 3 ==0).Select(b => b / 255d).ToArray();
            double[] xraster = RunConvolution(normalizedWorkImage, workImage.Width, workImage.Height, Edges.SobelX, 3);
            double[] yraster = RunConvolution(normalizedWorkImage, workImage.Width, workImage.Height, Edges.SobelY, 3);
            double[] rasterA = new double[xraster.Length];
            double[] rasterB = new double[xraster.Length];
            double[] rasterC = new double[xraster.Length];
            for (int i = 0; i < xraster.Length; i++)
            {
                rasterA[i] = Math.Pow(xraster[i], 2);
                rasterB[i] = Math.Pow(yraster[i], 2);
                rasterC[i] = xraster[i] * yraster[i];
            }
            rasterA = Filter.PadWithZeros(rasterA, image.Width, image.Height, 2, 2);
            rasterB = Filter.PadWithZeros(rasterB, image.Width, image.Height, 2, 2);
            rasterC = Filter.PadWithZeros(rasterC, image.Width, image.Height, 2, 2);
            // calculate the matrices
            double[][] matrices = new double[image.Width * image.Height][];
            int maxHeight = image.Height + 2;
            int maxWidth = image.Width + 2;
            int newWidth = image.Width + 4;
            int width = image.Width;
            for (int i = 2; i < maxHeight; i++)
            {
                //Parallel.For(2, maxHeight, (i) =>
                //{
                for (int j = 2; j < maxWidth; j++)
                {
                    // apply convolution
                    double accumulatorA = 0;
                    double accumulatorB = 0;
                    double accumulatorC = 0;
                    int mi = 0;
                    for (int x0 = -2; x0 <= 2; x0++)
                    {
                        for (int y0 = -2; y0 <= 2; y0++)
                        {
                            accumulatorA += gaussian[mi] * rasterA[(i + x0) * newWidth + j + y0];
                            accumulatorB += gaussian[mi] * rasterB[(i + x0) * newWidth + j + y0];
                            accumulatorC += gaussian[mi] * rasterC[(i + x0) * newWidth + j + y0];
                            mi++;
                        }
                    }
                    int realPosition = (i - 2) * width + (j - 2);
                    matrices[realPosition] = new double[] { accumulatorA, accumulatorB, accumulatorC };
                }
                //});
            }
            // do the part 2 and 3
            double[] cornerness = new double[image.Width * image.Height];
            for (int i = 0; i < matrices.Length; i++)
            {
                double det = (matrices[i][0] * matrices[i][1]) - (matrices[i][2] * matrices[i][2]);
                double trace = matrices[i][0] + matrices[i][1];
                double corner = det - (HarrisK * (trace * trace));
                if (corner >= HarrisThreshold)
                    cornerness[i] = corner;
            }
            //return ToPNM(cornerness, image.Width, image.Height);
            CenteredNonMaximumSuppression(cornerness, image.Width, image.Height, 3);
            // draw corners
            PNM newImage = PNM.Copy(image);
            for (int i = 0; i < cornerness.Length; i++)
            {
                if (cornerness[i] == 0)
                    continue;
                MarkPixel(newImage, i);
            }
            return newImage;
        }

        private static void MarkPixel(PNM image, int index)
        {
            int x = index % image.Width;
            int y = index / image.Width;
            for (int x0 = -1; x0 <= 1; x0++)
            {
                for (int y0 = -1; y0 <= 1; y0++)
                {
                    if(x0 == 0 && y0 == 0)
                        continue;
                    int currentY = y + y0;
                    int currentX = x + x0;
                    if(currentX < 0 || currentX >= image.Width || currentY < 0 || currentY >= image.Height)
                        continue;
                    image.SetPixel(x + x0, y + y0, 0, 255, 0);
                }
            }
        }

        internal static double[] RunConvolution(double[] array, int width, int height, float[] matrix, int matrixLength)
        {
            int padding = matrixLength / 2;
            int oldHeight = height - (padding * 2);
            int oldWidth = width - (padding * 2);
            double[] raster = new double[oldHeight * oldWidth];
            int maxHeight = height - padding;
            int maxWidth = width - padding;
            Parallel.For(padding, maxHeight, i =>
            {
                int index = (i - padding) * oldWidth;
                for (int j = padding; j < maxWidth; j++)
                {
                    double sum = 0;
                    // current index position
                    int position = i * width + j;
                    for (int m = 0; m < matrixLength; m++)
                    {
                        for (int n = 0; n < matrixLength; n++)
                        {
                            double value = array[(position - ((padding - m) * width) - (padding - n))];
                            float coeff = matrix[(m * matrixLength) + n];
                            sum += value * coeff;
                        }
                    }
                    raster[index] = sum;
                    index++;
                }
            });
            return raster;
        }

        private static void CenteredNonMaximumSuppression(double[] array, int width, int height, int radius)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    double center = array[y * width + x];
                    bool isMax = true;
                    for (int x0 = -radius; x0 <= radius; x0++)
                    {
                        if (!isMax)
                            break;

                        for (int y0 = -radius; y0 <= radius; y0++)
                        {
                            int position = ((y + y0) * width) + x + x0 ;
                            if(position < 0 || position >= array.Length)
                                continue;

                            if(center < array[position])
                            {
                                isMax = false;
                                break;
                            }
                        }
                    }

                    if (!isMax)
                        array[y * width + x] = 0;
                }
            }
        }

    }
}
