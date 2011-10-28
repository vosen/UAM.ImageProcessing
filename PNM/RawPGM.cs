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
            MaxVal = ParseNumber(ReadToken(reader), 1, 65535);

            float scale = 255f / MaxVal;

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
                SetPixel(i, Convert.ToByte(pixel * scale), Convert.ToByte(pixel * scale), Convert.ToByte(pixel * scale));
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
                byte pixelValue = Convert.ToByte(((pixel1 << 8) | pixel2) * scale);
                SetPixel(i, pixelValue , pixelValue, pixelValue);
            }
        }

        internal static void SaveFile(PNM bitmap, FileStream stream)
        {
            bitmap.WriteLongHeader("P5", stream);
            for (int i = 0; i < bitmap.Height * bitmap.Width; i++)
            {
                byte r,g,b;
                bitmap.GetPixel(i, out r, out g, out b);
                byte pixel = RGBToLuminosity(r, g, b);
                stream.WriteByte(pixel);
            }
        }
    }
}
