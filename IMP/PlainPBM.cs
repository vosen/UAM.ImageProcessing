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
        internal PlainPBM(TextReader file)
        {
            string line = null;
            // Skip comments
            line = ReadLineSkipComments(file);
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
    }
}
