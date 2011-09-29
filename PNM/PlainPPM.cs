using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace UAM.PTO
{
    public class PlainPPM : PNM
    {

        public int MaxVal { get; protected set; }

        internal PlainPPM(TextReader reader)
        {
            // Read width, height and range
            Width = ParseNumber(ReadToken(reader));
            Height = ParseNumber(ReadToken(reader));
            MaxVal = ParseNumber(ReadToken(reader), 8, 65535);

            float scale = 65536 / MaxVal;

            // Skip single whitespace character
            reader.Read();

            // Read raster
            InitializeRaster();

            int length = Width * Height;
            for (int i = 0; i < length; i++)
            {
                int pik = reader.Peek();
                ushort r = (ushort)(ParseNumber(ReadToken(reader), 0, MaxVal) * scale);
                ushort g = (ushort)(ParseNumber(ReadToken(reader), 0, MaxVal) * scale);
                ushort b = (ushort)(ParseNumber(ReadToken(reader), 0, MaxVal) * scale);
                ColorPixel(i, r, g, b);
            }
        }
    }
}
