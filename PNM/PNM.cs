using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace UAM.PTO
{
    public class PNM
    {
        internal byte[] raster;

        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public byte[] Raster 
        { 
            get { return raster;}
            private set { raster = value;} 
        }
        public int Stride { get { return Width * 3; } }

        protected PNM() { }

        internal PNM(int width, int height)
        {
            Width = width;
            Height = height;
            raster = new byte[Width * Height * 3];
        }

        public static PNM LoadFile(string path)
        {
            return LoadFile(File.Open(path, FileMode.Open));
        }

        public static PNM LoadFile(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.GetEncoding(28591)))
            {
                string header = ReadToken(reader);
                switch (header)
                {
                    case "P1":
                        return new PlainPBM(reader);
                    case "P2":
                        return new PlainPGM(reader);
                    case "P3":
                        return new PlainPPM(reader);
                    case "P4":
                        return new RawPBM(reader);
                    case "P5":
                        return new RawPGM(reader);
                    case "P6":
                        return new RawPPM(reader);
                    default:
                        throw new MalformedFileException("Malformed header");
                }
            }
        }

        public static void SaveFile(PNM bitmap, string path, PNMFormat format)
        {
            using (FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write))
            {
                switch (format)
                {
                    case PNMFormat.PBM:
                        RawPBM.SaveFile(bitmap, stream);
                        break;
                    case PNMFormat.PGM:
                        RawPGM.SaveFile(bitmap, stream);
                        break;
                    case PNMFormat.PPM:
                        RawPPM.SaveFile(bitmap, stream);
                        break;
                    default:
                        throw new ArgumentException("format");
                }
            }

        }

        // read token, ignore comments, throw MalformedFileException if there is no token to read
        // comment hash in ascii is 35
        protected static string ReadToken(TextReader reader)
        {
            StringBuilder builder = new StringBuilder();
            // Skip starting whitespace
            int temp;
            while(true)
            {
                temp = reader.Peek();
                if(temp == -1)
                    throw new MalformedFileException();
                if (temp == 35)
                {
                    reader.ReadLine();
                    continue;
                }
                if(!char.IsWhiteSpace((char)temp))
                    break;
                //comment - read to the end of line
                reader.Read();
            }
            // Read actual token
            builder.Append((char)reader.Read());
            while (true)
            {
                temp = reader.Peek();
                if (temp == -1 || char.IsWhiteSpace((char)temp))
                    break;
                builder.Append((char)reader.Read());
            }
            // Return
            return builder.ToString();
        }

        protected void InitializeRaster()
        {
            Raster = new byte[Width * Height * 3];
        }

        protected int ParseNumber(string token)
        {
            int result;
            if (!Int32.TryParse(token, System.Globalization.NumberStyles.None, NumberFormatInfo.InvariantInfo, out result))
                throw new MalformedFileException();
            return result;
        }

        protected int ParseNumber(string token, int min, int maxval)
        {
            int result;
            if (!Int32.TryParse(token, System.Globalization.NumberStyles.None, NumberFormatInfo.InvariantInfo, out result)
                || result > maxval || result < min)
                throw new MalformedFileException();
            return result;
        }

        // 0,0 is upper left corner, indices are postitive
        internal void SetPixel(int index, byte r, byte g, byte b)
        {
            if (index >= (Width * Height))
                throw new ArgumentException();
            unsafe
            {
                int realIndex = index * 3;
                fixed(byte* rasterp = raster)
                {
                    rasterp[realIndex] = r;
                    rasterp[++realIndex] = g;
                    rasterp[++realIndex] = b;
                }
            }
        }

        // 0,0 is upper left corner, indices are postitive
        internal void SetPixel(int x, int y, byte r, byte g, byte b)
        {
            SetPixel((x * Width) + y, r, g, b);
        }

        internal void GetPixel(int index, out byte r, out byte g, out byte b)
        {
            if (index >= (Width * Height))
                throw new ArgumentException();
            int realIndex = index * 3;
            unsafe
            {
                fixed (byte* rasterp = raster)
                {
                    r = raster[realIndex];
                    g = raster[++realIndex];
                    b = raster[++realIndex];
                }
            }
        }

        internal void WriteShortHeader(string magic, FileStream stream)
        {
            var encoding = Encoding.ASCII;
            byte[] header = encoding.GetBytes(magic + Environment.NewLine + Width + " " + Height + "\n");
            stream.Write(header, 0, header.Length);
        }


        internal void WriteLongHeader(string magic, FileStream stream)
        {
            var encoding = Encoding.ASCII;
            byte[] header = encoding.GetBytes(magic + Environment.NewLine + Width + " " + Height + Environment.NewLine + "255" + "\n");
            stream.Write(header, 0, header.Length);
        }

        internal static byte RGBToLuminosity(byte r, byte g, byte b)
        {
            return Convert.ToByte((r * 0.299) + (g * 0.587) + (b * 0.114));
        }

        internal static bool ColorToBlack(byte r, byte g, byte b)
        {
            return RGBToLuminosity(r, g, b) < 128;
        }

        // returned array is 256 elements long, every element has value between 0 and 1
        public double[] GetHistogramLuminosity()
        {
            int[] valueArray = new int[256];
            byte r, g, b;
            int size = Width * Height;
            for (int i = 0; i < size; i++)
            {
                GetPixel(i, out r, out g, out b);
                valueArray[(RGBToLuminosity(r, g, b))]++;
            }
            return valueArray.Select(amount => (double)amount / (double)size).ToArray();
        }

        // returned array is 256 elements long, every element has value between 0 and 1
        public double[] GetHistogramRed()
        {

            int[] valueArray = new int[256];
            byte r, g, b;
            int size = Width * Height;
            for (int i = 0; i < size; i++)
            {
                GetPixel(i, out r, out g, out b);
                valueArray[r]++;
            }
            return valueArray.Select(amount => (double)amount / (double)size).ToArray();
        }

        // returned array is 256 elements long, every element has value between 0 and 1
        public double[] GetHistogramGreen()
        {

            int[] valueArray = new int[256];
            byte r, g, b;
            int size = Width * Height;
            for (int i = 0; i < size; i++)
            {
                GetPixel(i, out r, out g, out b);
                valueArray[g]++;
            }
            return valueArray.Select(amount => (double)amount / (double)size).ToArray();
        }

        // returned array is 256 elements long, every element has value between 0 and 1
        public double[] GetHistogramBlue()
        {

            int[] valueArray = new int[256];
            byte r, g, b;
            int size = Width * Height;
            for (int i = 0; i < size; i++)
            {
                GetPixel(i, out r, out g, out b);
                valueArray[b]++;
            }
            return valueArray.Select(amount => (double)amount / (double)size).ToArray();
        }

        public static PNM Copy(PNM image)
        {
            PNM newImage = new PNM(image.Width, image.Height);
            Buffer.BlockCopy(image.raster, 0, newImage.raster, 0, image.raster.Length);
            return newImage;
        }
    }
}
