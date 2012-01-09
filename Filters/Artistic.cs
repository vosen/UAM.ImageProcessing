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

        public static Func<PNM,int,Pixel> GenerateFishEye(PNM image)
        {
            double maxR = Math.Sqrt(Math.Pow(image.Width / 2d, 2) + Math.Pow(image.Height / 2d, 2));
            return (img, idx) => FishEye(img, idx, maxR);
        }

        private static Pixel FishEye(PNM image, int index, double maxR)
        {
            byte r,g,b;
            double radius, angle;
            image.ToPolar(index, out radius, out angle);
            image.GetPixel((radius * radius) / maxR, angle, out r, out g, out b);
            return new Pixel(r, g, b);
        }

        public static Pixel Mirror(PNM image, int index)
        {
            byte r, g, b;
            int x = index % image.Width;
            int y = index / image.Width;
            if (x < image.Width / 2d)
                image.GetPixel((y * image.Width) + (image.Width - x - 1), out r, out g, out b);
            else
                image.GetPixel((y * image.Width) + x, out r, out g, out b);
            /*
            image.ToPolar(index, out radius, out angle);
            double newX = image.Width * angle/Math.PI*2;
            double newY = image.Height * radius / maxR;
            int newIndex = ((int)newX * image.Width) + (int)newY;
            // correct for pixels outside the image
            if(newIndex < 0 || newIndex >= image.Width * image.Height)
                newIndex = index;
            image.GetPixel(radius, angle + radius, out r, out g, out b);
             * */
            return new Pixel(r, g, b);
        }

        public static Pixel Negative(PNM image, int index)
        {
            byte r, g, b;
            image.GetPixel(index, out r, out g, out b);
            return new Pixel((byte)(255 - r), (byte)(255 - g), (byte)(255 - b));
        }
    }
}
