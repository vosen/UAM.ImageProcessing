using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UAM.PTO.Filters
{
    public static class Color
    {
        public static Pixel ToGrayscale(byte r, byte g, byte b)
        {
            byte value = PNM.RGBToLuminosity(r, g, b);
            return new Pixel(value, value, value);
        }

        // brightness should fall in [-1..1] range and contrast should fall in [-1..1] range
        public static Func<byte, byte, byte, Pixel> BrightnessContrast(float brigthness, float contrast)
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
                    return new Pixel(Filter.Coerce((r - 127.5) * realContrast + (brigthness * 127) + 127.5),
                                     Filter.Coerce((g - 127.5) * realContrast + (brigthness * 127) + 127.5),
                                     Filter.Coerce((b - 127.5) * realContrast + (brigthness * 127) + 127.5));
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
            byte minEnergy = Filter.Coerce(realBright);
            for (int i = 0; i < lower; i++)
            {
                lut[i] = minEnergy;
            }
            for (int i = lower, k = 0; i <= upper; i++, k++)
            {
                lut[i] = Filter.Coerce((k * segment) + realBright);
            }
            for (int i = upper + 1; i < 256; i++)
            {
                lut[i] = 255;
            }
            return lut;
        }

        public static Func<byte, byte, byte, Pixel> Gamma(float weight)
        {
            return (r, g, b) =>
            {
                return new Pixel(Filter.Coerce(Math.Pow(r / 255d, weight) * 255),
                                 Filter.Coerce(Math.Pow(g / 255d, weight) * 255),
                                 Filter.Coerce(Math.Pow(b / 255d, weight) * 255));
            };
        }
    }
}
