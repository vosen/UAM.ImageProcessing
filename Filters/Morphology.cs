using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UAM.PTO.Filters
{
    public static class Morphology
    {
        public static Pixel Dilation(PNM image, int index)
        {
            int length = 3;
            int padding = 1;
            byte r = 255, g = 255, b = 255;
            byte tempR, tempG, tempB;
            for (int m = 0; m < length; m++)
            {
                for (int n = 0; n < length; n++)
                {
                    image.GetPixel(index - ((padding - m) * image.Width) - (padding - n), out tempR, out tempG, out tempB);
                    // check for "black pixel"
                    if (tempR < 128)
                        r = 0;
                    if (tempG < 128)
                        g = 0;
                    if (tempB < 128)
                        b = 0;
                }

                if (r == 0 && g == 0 && b == 0)
                    break;
            }
            return new Pixel(r, g, b);
        }

        public static Pixel Erosion(PNM image, int index)
        {
            int length = 3;
            int padding = 1;
            byte r = 0, g = 0, b = 0;
            byte tempR, tempG, tempB;
            for (int m = 0; m < length; m++)
            {
                for (int n = 0; n < length; n++)
                {
                    image.GetPixel(index - ((padding - m) * image.Width) - (padding - n), out tempR, out tempG, out tempB);
                    // check for "white pixel"
                    if (tempR > 127)
                        r = 255;
                    if (tempG > 127)
                        g = 255;
                    if (tempB > 127)
                        b = 255;
                }

                if (r != 0 && g != 0 && b != 0)
                    break;
            }
            return new Pixel(r, g, b);
        }
    }
}
