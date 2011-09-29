using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace UAM.PTO
{
    public class RawPGM : PNM
    {
        public int MaxVal { get; protected set; }

        internal RawPGM(TextReader reader)
        {
            
            // Read width and height
            Width = ParseNumber(ReadToken(reader));
            Height = ParseNumber(ReadToken(reader));
            MaxVal = ParseNumber(ReadToken(reader), 8, 65535);

            float scale = 65536 / MaxVal;

            // Skip single whitespace character
            reader.Read();

            // Read raster
            InitializeRaster();

            if (MaxVal < 256)
                LoadPayloadSingleByte(reader, scale);
            else
                LoadPayloadDoubleByte(reader, scale);
        }

        private void LoadPayloadSingleByte(TextReader reader, float scale)
        {
            int length = Width * Height;
            int pixel = 0;
            for (int i = 0; i < length; i++)
            {
                pixel = reader.Read();
                if (pixel == -1)
                    throw new MalformedFileException();
                ColorPixel(i, (ushort)(pixel * scale), (ushort)(pixel * scale), (ushort)(pixel * scale));
            }
        }

        private void LoadPayloadDoubleByte(TextReader reader, float scale)
        {
            int length = Width * Height;
            int pixel1 = 0;
            int pixel2 = 0;
            for (int i = 0; i < length; i++)
            {
                pixel1 = reader.Read();
                pixel2 = reader.Read();
                if (pixel1 == -1 || pixel2 == -1)
                    throw new MalformedFileException();
                ushort pixelValue = (ushort)((pixel1 << 8) | pixel2);
                ColorPixel(i, pixelValue, pixelValue, pixelValue);
            }
        }
    }
}
