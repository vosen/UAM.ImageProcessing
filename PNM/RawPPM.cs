using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace UAM.PTO
{
    public class RawPPM : PNM
    {

        public int MaxVal { get; protected set; }

        internal RawPPM(TextReader reader)
        {
            // Read width and height
            Width = ParseNumber(ReadToken(reader));
            Height = ParseNumber(ReadToken(reader));
            MaxVal = ParseNumber(ReadToken(reader), 1, 65535);

            float scale = 65535 / MaxVal;

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
            int pixelr = 0;
            int pixelg = 0;
            int pixelb = 0;
            for (int i = 0; i < length; i++)
            {
                pixelr = reader.Read();
                pixelg = reader.Read();
                pixelb = reader.Read();
                if (pixelr == -1 || pixelg == -1 || pixelb == -1)
                    throw new MalformedFileException();
                ColorPixel(i, Convert.ToUInt16(pixelr * scale), Convert.ToUInt16(pixelg * scale), Convert.ToUInt16(pixelb * scale));
            }
        }

        private void LoadPayloadDoubleByte(TextReader reader, float scale)
        {
            int length = Width * Height;
            int pixelr1 = 0;
            int pixelr2 = 0;
            int pixelg1 = 0;
            int pixelg2 = 0;
            int pixelb1 = 0;
            int pixelb2 = 0;
            for (int i = 0; i < length; i++)
            {
                pixelr1 = reader.Read();
                pixelr2 = reader.Read();
                pixelg1 = reader.Read();
                pixelg2 = reader.Read();
                pixelb1 = reader.Read();
                pixelb2 = reader.Read();
                if (pixelr1 == -1 || pixelr2 == -1 || pixelg1 == -1 || pixelg2 == -1 || pixelb1 == -1 || pixelb2 == -1)
                    throw new MalformedFileException();
                ColorPixel(i, Convert.ToUInt16(((pixelr1 << 8) | pixelr2) * scale), Convert.ToUInt16(((pixelg1 << 8) | pixelg2) * scale), Convert.ToUInt16(((pixelb1 << 8) | pixelb2) * scale));
            }
        }
    }
}
