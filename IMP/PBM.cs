using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace IMP
{
    // Eveerything is in rgb24
    public class PBM
    {
        protected int width;
        protected int height;

        public int Width { get { return width; } }
        public int Height { get { return height; } }
        public byte[] Bitmap { get; private set; }
        public int Stride { get { return Width * 3; } }

        public PBM(StreamReader file)
        {
            // Check header
            if (file.ReadLine() != "P1")
                throw new Exception();
            string line = null;
            // Skip comments
            while ((line = file.ReadLine())[0] == '#') ;
            // Get width and height
            var dims = line.Split(' ');
            if(dims.Length != 2)
                throw new Exception();

            if (!Int32.TryParse(dims[0], System.Globalization.NumberStyles.None, NumberFormatInfo.InvariantInfo, out width)
               || !Int32.TryParse(dims[1], System.Globalization.NumberStyles.None, NumberFormatInfo.InvariantInfo, out height))
                throw new Exception();

            // Every pixel is 3 bytes
            Bitmap = new byte[height * width * 3];

            string[] splitLine;
            int linenr = -1;
            while((line = file.ReadLine()) != null)
            {
                linenr++;
                splitLine = line.Split(' ');
                if(splitLine.Length != width)
                    throw new Exception();
                for(int i = 0; i < width; i++)
                {
                    if (splitLine[i] == "0")
                        ColorPixel(i, linenr, 255, 255, 255);
                    else if (splitLine[i] != "1")
                        throw new Exception();
                }
            }
        }

        // 0,0 is upper left corner, indices are postitive
        protected void ColorPixel(int x, int y, byte r, byte g, byte b)
        {
            if(x >= width)
                throw new ArgumentException();
            if(y >= height)
                throw new ArgumentException();
            int index = (y * width * 3) + (x * 3);
            Buffer.SetByte(Bitmap, index, r);
            Buffer.SetByte(Bitmap, ++index, r);
            Buffer.SetByte(Bitmap, ++index, r);
        }

        public void DrawOn(object obj)
        {

        }
    }
}
