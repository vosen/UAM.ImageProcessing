using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAM.PTO.Filters
{
    public static class EdgeDetection
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
        private static float[] LoG = new float[]{ 0, 1, 1,   2,   2,   2, 1, 1, 0,
                                                  1, 2, 4,   5,   5,   5, 4, 2, 1,
                                                  1, 4, 5,   3,   0,   3, 5, 4, 1,
                                                  2, 5, 3, -12, -24, -12, 3, 5, 2,
                                                  2, 5, 0, -24, -40, -24, 0, 5, 2,
                                                  2, 5, 3, -12, -24, -12, 3, 5, 2,
                                                  1, 4, 5,   3,   0,   3, 5, 4, 1,
                                                  1, 2, 4,   5,   5,   5, 4, 2, 1,
                                                  0, 1, 1,   2,   2,   2, 1, 1, 0};

        public static PNM ApplyZeroCrossingDetector(this PNM image)
        {
            // preprare
            PNM workImage = PNM.Copy(image);
            Filter.Pad(workImage, 4);
            // apply loG
            Tuple<float[], float[], float[]> LoGRaster = Filter.ApplyConvolutionUnbound(workImage, LoG, 9);
            PNM returnImage = new PNM(image.Width, image.Height);
            // Apply zero crossing except last row and last column
            Parallel.For(0, image.Height - 1, i =>
            {
                for (int j = 0; j < image.Width - 1; j++)
                {
                    byte r = 0;
                    byte g = 0;
                    byte b = 0;
                    // current index position
                    int position = i * image.Width + j;
                    float currentR = LoGRaster.Item1[position];
                    float neighbourR = LoGRaster.Item1[position + image.Width + 1];
                    float currentG = LoGRaster.Item2[position];
                    float neighbourG = LoGRaster.Item2[position + image.Width + 1];
                    float currentB = LoGRaster.Item3[position];
                    float neighbourB = LoGRaster.Item3[position + image.Width + 1];
                    if ((currentR * neighbourR) < 0 && (Math.Abs(currentR) < Math.Abs(neighbourR)))
                        r = 255;
                    if ((currentG * neighbourG) < 0 && (Math.Abs(currentG) < Math.Abs(neighbourG)))
                        g = 255;
                    if ((currentB * neighbourB) < 0 && (Math.Abs(currentB) < Math.Abs(neighbourB)))
                        b = 255;

                    returnImage.SetPixel(position, r, g, b);
                }
            });
            return returnImage;
        }

        private static double Module(float g1, float g2)
        {
            return Math.Sqrt((g1 * g1) + (g2 * g2));
        }

        internal static Pixel ConvoluteWithModule(PNM image, int index, float[] matrix1, float[] matrix2, int length, int padding)
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
            return new Pixel(Filter.Coerce(Module(sumR1, sumR2)),
                             Filter.Coerce(Module(sumG1, sumG2)),
                             Filter.Coerce(Module(sumB1, sumB2)));
        }

        public static Pixel Sobel(PNM image, int index)
        {
            return ConvoluteWithModule(image, index, Sobel1, Sobel2, 3, 1);
        }

        public static Pixel Prewitt(PNM image, int index)
        {
            return ConvoluteWithModule(image, index, Prewitt1, Prewitt2, 3, 1);
        }

        public static Pixel Roberts(PNM image, int index)
        {
            return ConvoluteWithModule(image, index, Roberts1, Roberts2, 3, 1);
        }
    }
}
