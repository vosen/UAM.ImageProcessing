using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UAM.PTO.Filters
{
    public static class Artistic
    {
        public static Pixel Oil(PNM image, int index)
        {
            int length = 7;
            int padding = length / 2;
            byte[] rvalues = new byte[length * length];
            byte[] gvalues = new byte[length * length];
            byte[] bvalues = new byte[length * length];
            byte r, g, b;
            int i = 0;
            for (int m = 0; m < length; m++)
            {
                for (int n = 0; n < length; n++)
                {
                    image.GetPixel(index - ((padding - m) * image.Width) - (padding - n), out r, out g, out b);
                    rvalues[i] = r;
                    gvalues[i] = g;
                    bvalues[i] = b;
                    i++;
                }
            }
            return new Pixel(rvalues.GroupBy(p => p).OrderByDescending(gr => gr.Count()).First().Key,
                             gvalues.GroupBy(p => p).OrderByDescending(gr => gr.Count()).First().Key,
                             bvalues.GroupBy(p => p).OrderByDescending(gr => gr.Count()).First().Key);
        }

        public static Pixel FishEye(PNM image, int index)
        {
            double maxR = Math.Sqrt(Math.Pow(image.Width / 2d, 2) + Math.Pow(image.Height / 2d, 2));
            byte r,g,b;
            double radius, angle;
            image.ToPolar(index, out radius, out angle);
            image.GetPixel((radius * radius) / maxR, angle, out r, out g, out b);
            return new Pixel(r, g, b);
        }
    }
}
