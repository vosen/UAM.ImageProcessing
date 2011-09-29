using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace UAM.PTO
{
    // Everything is in rgb24
    public class PlainPBM : PNM
    {
        internal PlainPBM(TextReader reader)
        {
            // Read width and height
            Width = ParseNumber(ReadToken(reader));
            Height = ParseNumber(ReadToken(reader));
            
            // Skip single whitespace character
            reader.Read();

            // Read raster
            InitializeRaster();

            int length = Width * Height;
            for (int i = 0; i < length; i++)
            {
                string token = ReadToken(reader);
                if (token == "0")
                    ColorPixel(i, 65535, 65535, 65535);
                else if (token != "1")
                    throw new MalformedFileException();
            }
        }
    }
}
