using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace UAM.PTO
{
    public abstract class PNM
    {
        private byte[] raster;

        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public byte[] Raster 
        { 
            get { return raster;}
            private set { raster = value;} 
        }
        public int Stride { get { return Width * 6; } }

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
            Raster = new byte[Width * Height * 6];
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
        internal void SetPixel(int index, ushort r, ushort g, ushort b)
        {
            if (index >= (Width * Height))
                throw new ArgumentException();
            unsafe
            {
                int realIndex = index * 6;
                fixed(byte* rasterp = raster)
                {
                    rasterp[realIndex] = (byte)r;
                    rasterp[++realIndex] = (byte)(r >> 8);
                    rasterp[++realIndex] = (byte)g;
                    rasterp[++realIndex] = (byte)(g >> 8);
                    rasterp[++realIndex] = (byte)b;
                    rasterp[++realIndex] = (byte)(b >> 8);
                }
            }
        }

        // 0,0 is upper left corner, indices are postitive
        internal void SetPixel(int x, int y, ushort r, ushort g, ushort b)
        {
            SetPixel((x * Width) + y, r, g, b);
        }

        internal void GetPixel(int index, out ushort r, out ushort g, out ushort b)
        {
            if (index >= (Width * Height))
                throw new ArgumentException();
            int realIndex = index * 6;
            r = BitConverter.ToUInt16(Raster, realIndex);
            g = BitConverter.ToUInt16(Raster, realIndex + 2);
            b = BitConverter.ToUInt16(Raster, realIndex + 4);
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
            byte[] header = encoding.GetBytes(magic + Environment.NewLine + Width + " " + Height + Environment.NewLine + "65535" + "\n");
            stream.Write(header, 0, header.Length);
        }

        internal static ushort RGBToLuminosity(ushort r, ushort g, ushort b)
        {
            return Convert.ToUInt16((r * 0.299) + (g * 0.587) + (b * 0.114));
        }

        internal static bool ColorToBlack(ushort r, ushort g, ushort b)
        {
            ushort gray = RGBToLuminosity(r, g, b);
            return gray < 32768;
        }

        // returned array is 256 elements long, every element has value between 0 and 1
        public double[] GetHistogramLuminosity()
        {
            int[] valueArray = new int[256];
            ushort r, g, b;
            int size = Width * Height;
            for (int i = 0; i < size; i++)
            {
                GetPixel(i, out r, out g, out b);
                valueArray[(RGBToLuminosity(r, g, b) / 256)]++;
            }
            return valueArray.Select(amount => (double)amount / (double)size).ToArray();
        }

        // returned array is 256 elements long, every element has value between 0 and 1
        public double[] GetHistogramRed()
        {

            int[] valueArray = new int[256];
            ushort r, g, b;
            int size = Width * Height;
            for (int i = 0; i < size; i++)
            {
                GetPixel(i, out r, out g, out b);
                valueArray[r / 256]++;
            }
            return valueArray.Select(amount => (double)amount / (double)size).ToArray();
        }

        // returned array is 256 elements long, every element has value between 0 and 1
        public double[] GetHistogramGreen()
        {

            int[] valueArray = new int[256];
            ushort r, g, b;
            int size = Width * Height;
            for (int i = 0; i < size; i++)
            {
                GetPixel(i, out r, out g, out b);
                valueArray[g / 256]++;
            }
            return valueArray.Select(amount => (double)amount / (double)size).ToArray();
        }

        // returned array is 256 elements long, every element has value between 0 and 1
        public double[] GetHistogramBlue()
        {

            int[] valueArray = new int[256];
            ushort r, g, b;
            int size = Width * Height;
            for (int i = 0; i < size; i++)
            {
                GetPixel(i, out r, out g, out b);
                valueArray[b / 256]++;
            }
            return valueArray.Select(amount => (double)amount / (double)size).ToArray();
        }

        public void ApplyFilter(Func<ushort,ushort,ushort,Tuple<ushort,ushort,ushort>> filter)
        {
            ushort r,g,b;
            int size = Width * Height;
            for (int i = 0; i < size; i++)
            {
                GetPixel(i, out r, out g, out b);
                Tuple<ushort, ushort, ushort> pixel = filter(r, g, b);
                SetPixel(i, pixel.Item1, pixel.Item2, pixel.Item3);
            }
        }


        // just pad with black for now
        protected void Pad(int length)
        {
            int newHeight = Height + (2 * length);
            int newWidth =  Width + (2 * length);
            byte[] newRaster = new byte[newHeight * newWidth * 6];
            // skip black rows at the top
            int start = length * newWidth * 6;
            int oldSize = Height * Width * 6;
            // copy rows
            for (int i_new = start, i_old = 0; i_old < oldSize; i_new += (newWidth * 6), i_old += (Width * 6))
            {
                Buffer.BlockCopy(raster, i_old, newRaster, i_new + (length*6), Width * 6);
            }
            raster = newRaster;
            Width = newWidth;
            Height = newHeight;
        }

        protected void Trim(int length)
        {

        }

        // for now assume that args are correct and sums nicely
        public void ApplyConvolutionMatrix(double[] matrix, int length)
        {
            //s
            Pad((length - 1) / 2);
            ApplyConvolutionMatrixCore(matrix, length);
            Trim((length - 1) / 2);
        }

        private void ApplyConvolutionMatrixCore(double[] matrix,int padding)
        {
            byte[] newRaster = new byte[raster.Length];
            for (int i = padding * 6; i < (Width - padding) * 6; i += 6)
            {
                for (int j = padding * 6; j < (Height - padding) * 6; j += 6)
                {

                }
            }
            Height -= (2 * padding);
            Width -= (2 * padding);
            raster = newRaster;
        }
    }
}
