using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAM.PTO.Filters
{
    enum Orientation
    {
        WE = 1,
        NS = 2,
        NWSE = 3,
        NESW = 4
    }


    public static class Edges
    {
        internal static float[] SobelX = { -1, 0, 1,
                                          -2, 0, 2,
                                          -1, 0, 1};
        internal static float[] SobelY = {  1,  2,  1,
                                           0,  0,  0,
                                          -1, -2, -1};
        internal static float[] PrewittX = { -1, 0, 1,
                                            -1, 0, 1,
                                            -1, 0, 1};
        internal static float[] PrewittY = {  1,  1,  1,
                                             0,  0,  0,
                                            -1, -1, -1};
        private static float[] RobertsX = { 0,  0,  1,
                                            0, -1,  0,
                                            0,  0,  0};
        private static float[] RobertsY = { 0, 1,  0,
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

        internal static double Module(double g1, double g2)
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
            return ConvoluteWithModule(image, index, SobelX, SobelY, 3, 1);
        }

        public static Pixel Prewitt(PNM image, int index)
        {
            return ConvoluteWithModule(image, index, PrewittX, PrewittY, 3, 1);
        }

        public static Pixel Roberts(PNM image, int index)
        {
            return ConvoluteWithModule(image, index, RobertsX, RobertsY, 3, 1);
        }

        public static PNM ApplyCannyDetector(this PNM image)
        {
            PNM workImage = image.ApplyPointProcessing(Color.ToGrayscale)
                                 .ApplyConvolutionMatrix(new float[]{     0, 0.01F, 0.02F, 0.01F,    0,
                                                                      0.01F, 0.06F,  0.1F, 0.06F, 0.01F,
                                                                      0.02F,  0.1F, 0.16F,  0.1F, 0.02F,
                                                                      0.01F, 0.06F,  0.1F, 0.06F, 0.01F,
                                                                          0, 0.01F, 0.02F, 0.01F,     0}, 1, 0);
            Filter.Pad(workImage, 1);
            float[] xraster = Filter.ApplyConvolutionUnbound(workImage, SobelX, 3).Item1;
            float[] yraster = Filter.ApplyConvolutionUnbound(workImage, SobelY, 3).Item1;
            var vectorField = xraster.Zip(yraster,  (x, y) => Tuple.Create(Module(x, y), GetOrientation(x,y))).ToArray();
            byte[] suppressed = NonMaximumSuppression(vectorField, image.Width, image.Height);
            return ApplyHysteresis(suppressed, image.Width, image.Height, 0.05, 0.2);
        }

        private static PNM ApplyHysteresis(byte[] suppressed, int width, int height, double low, double high)
        {
            PNM image = new PNM(width, height);
            byte lowValue = (byte)(low*255);
            byte highValue = (byte)(high*255);
            for (int i = 0; i < suppressed.Length; i++)
            {
                if (suppressed[i] >= highValue)
                {
                    image.SetPixel(i, 255, 255, 255);
                    HysteresisConnect(image, i, width, height, lowValue, highValue);
                }
            }
            return image;
        }

        private static void HysteresisConnect(PNM image, int index, int width, int height, byte lowValue, byte highValue)
        {
            int x = index % width;
            int y = index / height;
            for (int x0 = x - 1; x0 < x + 1; x0++)
            {
                for (int y0 = y - 1; y0 < y + 1; y0++)
                {
                    int currentIndex = (width * y0) + x0;
                    if (!IsPixelOnEdge(currentIndex, width, height))
                    {
                        byte l;
                        image.GetPixel(currentIndex, out l, out l, out l);
                        if (l != 255)
                        {
                            if (l >= lowValue)
                            {
                                image.SetPixel(currentIndex, 255, 255, 255);
                                HysteresisConnect(image, currentIndex, width, height, lowValue, highValue);
                            }
                        }
                    }
                }
            }
        }

        internal static Orientation GetOrientation(double x, double y)
        {
            double atan = Math.Atan(x / y);
            if (atan <= Math.PI /8 && atan > -Math.PI/8)
                return Orientation.NS;
            if (atan <= 3 / 8 * Math.PI && atan > Math.PI / 8)
                return Orientation.NESW;
            if (atan <= -Math.PI/8 && atan > -3/8 * Math.PI)
                return Orientation.NESW;
            return Orientation.WE;
        }

        internal static byte[] NonMaximumSuppression(Tuple<double, Orientation>[] vectorField, int width, int height)
        {
            return vectorField.Select((tuple,idx) => SuppressedValue(vectorField, idx, width, height)).ToArray();
        }

        private static byte SuppressedValue(Tuple<double, Orientation>[] vectorField, int index, int width, int height)
        {
            switch(vectorField[index].Item2)
            {
                case Orientation.WE:
                    return SuppressedValueWE(vectorField, index, width, height);
                case Orientation.NS:
                    return SuppressedValueNS(vectorField, index, width, height);
                case Orientation.NWSE:
                    return SuppressedValueNWSE(vectorField, index, width, height);
                case Orientation.NESW:
                    return SuppressedValueNESW(vectorField, index, width, height);
            }
            return 0;
        }

        private static byte SuppressedValueWE(Tuple<double, Orientation>[] vectorField, int index, int width, int height)
        {
            if (IsPixelOnWestEdge(index, width, height))
            {
                if (vectorField[index].Item1 > vectorField[index + 1].Item1)
                    return Filter.Coerce(vectorField[index].Item1);
                return 0;
            }

            if (IsPixelOnEastEdge(index, width, height))
            {
                if (vectorField[index].Item1 > vectorField[index - 1].Item1)
                    return Filter.Coerce(vectorField[index].Item1);
                return 0;
            }

            if (vectorField[index].Item1 > vectorField[index + 1].Item1 && vectorField[index].Item1 > vectorField[index - 1].Item1)
                return Filter.Coerce(vectorField[index].Item1);
            return 0;
        }

        private static byte SuppressedValueNS(Tuple<double, Orientation>[] vectorField, int index, int width, int height)
        {
            if (IsPixelOnNorthEdge(index, width, height))
            {
                if (vectorField[index].Item1 > vectorField[index + width].Item1)
                    return Filter.Coerce(vectorField[index].Item1);
                return 0;
            }

            if (IsPixelOnSouthEdge(index, width, height))
            {
                if (vectorField[index].Item1 > vectorField[index - width].Item1)
                    return Filter.Coerce(vectorField[index].Item1);
                return 0;
            }

            if (vectorField[index].Item1 > vectorField[index + width].Item1 && vectorField[index].Item1 > vectorField[index - width].Item1)
                return Filter.Coerce(vectorField[index].Item1);
            return 0;
        }

        private static byte SuppressedValueNWSE(Tuple<double, Orientation>[] vectorField, int index, int width, int height)
        {
            if (IsPixelOnNorthEdge(index, width, height) && IsPixelOnWestEdge(index, width, height))
            {
                if (vectorField[index].Item1 > vectorField[index + width + 1].Item1)
                    return Filter.Coerce(vectorField[index].Item1);
                return 0;
            }

            if (IsPixelOnSouthEdge(index, width, height) && IsPixelOnEastEdge(index, width, height))
            {
                if (vectorField[index].Item1 > vectorField[index - width -1].Item1)
                    return Filter.Coerce(vectorField[index].Item1);
                return 0;
            }

            if (vectorField[index].Item1 > vectorField[index + width +1].Item1 && vectorField[index].Item1 > vectorField[index - width -1].Item1)
                return Filter.Coerce(vectorField[index].Item1);
            return 0;
        }

        private static byte SuppressedValueNESW(Tuple<double, Orientation>[] vectorField, int index, int width, int height)
        {
            if (IsPixelOnNorthEdge(index, width, height) && IsPixelOnEastEdge(index, width, height))
            {
                if (vectorField[index].Item1 > vectorField[index + width - 1].Item1)
                    return Filter.Coerce(vectorField[index].Item1);
                return 0;
            }

            if (IsPixelOnSouthEdge(index, width, height) && IsPixelOnWestEdge(index, width, height))
            {
                if (vectorField[index].Item1 > vectorField[index - width + 1].Item1)
                    return Filter.Coerce(vectorField[index].Item1);
                return 0;
            }

            if (vectorField[index].Item1 > vectorField[index + width - 1].Item1 && vectorField[index].Item1 > vectorField[index - width + 1].Item1)
                return Filter.Coerce(vectorField[index].Item1);
            return 0;
        }

        private static bool IsPixelOnEdge(int index, int width, int height)
        {
            return IsPixelOnNorthEdge(index, width, height)
                   || IsPixelOnSouthEdge(index, width, height)
                   || IsPixelOnWestEdge(index, width, height)
                   || IsPixelOnEastEdge(index, width, height);
        }

        private static bool IsPixelOnNorthEdge(int index, int width, int height)
        {
            return index < width;
        }

        private static bool IsPixelOnSouthEdge(int index, int width, int height)
        {
            return index >= width * (height-1);
        }

        private static bool IsPixelOnWestEdge(int index, int width, int height)
        {
            return index % width == 0;
        }

        private static bool IsPixelOnEastEdge(int index, int width, int height)
        {
            return IsPixelOnWestEdge(index + 1, width, height);
        }
    }
}
