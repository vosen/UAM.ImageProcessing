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
        private static double HarrisK = 0.13;
        private static double HarrisThreshold = 200;

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
            float[] xraster = Filter.ApplyConvolutionUnbound(workImage, Edges.PrewittX, 3).Item1;
            float[] yraster = Filter.ApplyConvolutionUnbound(workImage, Edges.PrewittY, 3).Item1;
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
            // do suppression
            // BUG: integrate cornerness table not temporary image
            float[] xcorners = Filter.ApplyConvolutionUnbound(workImage, Edges.PrewittX, 3).Item1;
            float[] ycorners = Filter.ApplyConvolutionUnbound(workImage, Edges.PrewittY, 3).Item1;
            var vectorField = xraster.Zip(yraster, (x, y) => Tuple.Create(Edges.Module(x, y), Edges.GetOrientation(x, y))).ToArray();
            byte[] suppressed = Edges.NonMaximumSuppression(vectorField, image.Width, image.Height);
            // draw corners
            PNM newImage = PNM.Copy(image);
            for (int i = 0; i < suppressed.Length; i++)
            {
                if (suppressed[i] == 0)
                    continue;
                newImage.SetPixel(i, 0, 0, 255);
            }
            return newImage;
        }

    }
}
