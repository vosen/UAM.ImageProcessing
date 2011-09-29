using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace UAM.PTO
{
    public class RawPBM : PNM
    {
        internal RawPBM(TextReader reader)
        {
            // Read width and height
            Width = ParseNumber(ReadToken(reader));
            Height = ParseNumber(ReadToken(reader));

            // Skip single whitespace character
            reader.Read();

            // Read raster
            InitializeRaster();
            int length = Width * Height;
            int lineLength = (Width + 7) / 8;
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < lineLength; j++)
                {
                    // This is wrong but i don't care
                    int vec = reader.Read();
                    if (vec == -1)
                        throw new MalformedFileException();
                    ProcessPixels(i, (j * 8), vec, PackedBits(j));
                }
            }
        }

        private int PackedBits(int j)
        {
            return (j * 8) > (Width - 8) ? (Width % 8) + 1 : 8;
        }

        private void ProcessPixels(int x, int offset, int vector, int amount)
        {
            int temp = (vector << 1);
            for(int i = 0; i< amount; i++)
            {
                if (((temp >>= 1) & 1) == 0)
                    ColorPixel(x, offset + 7 - i, UInt16.MaxValue, UInt16.MaxValue, UInt16.MaxValue);
            }
        }
    }
}
