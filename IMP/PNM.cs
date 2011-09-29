using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UAM.PTO
{
    public abstract class PNM
    {
        private static char[] whitespace = new char[] { ' ', '\t', '\r', '\n' };

        protected int width;
        protected int height;

        public int Width { get { return width; } }
        public int Height { get { return height; } }
        public byte[] Bitmap { get; protected set; }
        public int Stride { get { return Width * 3; } }

        public static PNM LoadFile(string path)
        {
            using (TextReader reader = new StreamReader(path))
            {
                string line = ReadLineSkipComments(reader);
                if(line == null)
                    throw new MalformedFileException("Malformed header");
                string[] tokens = Split(line.Trim());
                if (tokens.Length != 1)
                    throw new MalformedFileException("Malformed header");
                switch(tokens[0])
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

        public static void SaveFile(string path)
        {

        }

        protected static string ReadLineSkipComments(TextReader reader)
        {
            string line = null;
            while ((line = reader.ReadLine()) != null && line.Length != 0 && line[0] == '#');
            return line;
        }

        protected static string[] Split(string str)
        {
            return str.Split(whitespace, StringSplitOptions.RemoveEmptyEntries);
        }

        // 0,0 is upper left corner, indices are postitive
        protected void ColorPixel(int x, int y, byte r, byte g, byte b)
        {
            if (x >= width)
                throw new ArgumentException();
            if (y >= height)
                throw new ArgumentException();
            int index = (y * width * 3) + (x * 3);
            Buffer.SetByte(Bitmap, index, r);
            Buffer.SetByte(Bitmap, ++index, r);
            Buffer.SetByte(Bitmap, ++index, r);
        }
    }
}
