using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UAM.PTO
{
    public static class Filters
    {
        public static Func<ushort,ushort,ushort,Tuple<ushort,ushort,ushort>> HistogramEqualize(PNM pnm)
        {
            double[] rawData = pnm.GetHistogramLuminosity();
            ushort[] lut = new ushort[256];
            double cumulated = 0;
            for (int i = 0; i < 256; i++)
            {
                cumulated += rawData[i];
                lut[i] = Convert.ToUInt16(cumulated * 65535);
            }
            return LuminosityMultitpliersToFunction(lut);
        }

        public static Func<ushort, ushort, ushort, Tuple<ushort, ushort, ushort>> HistogramMatch(PNM pnm)
        {
            return null;
        }


        private static Func<ushort, ushort, ushort, Tuple<ushort, ushort, ushort>> LuminosityMultitpliersToFunction(ushort[] mult)
        {
            return (r, g, b) =>
                {
                    ushort oldlum = PNM.RGBToLuminosity(r, g, b);
                    int index = oldlum / 256;
                    double newIncrLum = ((double)mult[index])+1;
                    double oldIncrLum = ((double)oldlum) + 1;
                    double scale = newIncrLum / oldIncrLum;
                    return Tuple.Create(Normalize(((r + 1) * scale) - 1), Normalize(((g + 1) * scale) - 1), Normalize(((b + 1) * scale) - 1));
                };
        }

        private static ushort Normalize(double value)
        {
            return Convert.ToUInt16(Math.Max(0, Math.Min(65535,value)));
        }
    }
}