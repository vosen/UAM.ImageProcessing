using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UAM.PTO.Filters
{
    public static class Blur
    {
        // assume mask 3x3, image is already 1 pixel padded
        public static Pixel Median(PNM image, int index)
        {
            byte[] workArrayR = new byte[9];
            byte[] workArrayG = new byte[9];
            byte[] workArrayB = new byte[9];
            int length = 3;
            int padding = 1;
            byte r, g, b;
            int i = 0;
            for (int m = 0; m < length; m++)
            {
                for (int n = 0; n < length; n++)
                {
                    image.GetPixel(index - ((padding - m) * image.Width) - (padding - n), out r, out g, out b);
                    workArrayR[i] = r;
                    workArrayG[i] = g;
                    workArrayB[i] = b;
                    i++;
                }
            }
            Array.Sort(workArrayR);
            Array.Sort(workArrayG);
            Array.Sort(workArrayB);
            return new Pixel(workArrayR[4], workArrayG[4], workArrayB[4]);
        }
    }
}
