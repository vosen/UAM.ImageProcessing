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
            double[] rawDataR = pnm.GetHistogramRed();
            double[] rawDataG = pnm.GetHistogramGreen();
            double[] rawDataB = pnm.GetHistogramBlue();
            ushort[] rlum = new ushort[256];
            ushort[] glum = new ushort[256];
            ushort[] blum = new ushort[256];
            double cumulatedR = 0;
            double cumulatedG = 0;
            double cumulatedB = 0;
            for (int i = 0; i < 256; i++)
            {
                cumulatedR += rawDataR[i];
                cumulatedG += rawDataG[i];
                cumulatedB += rawDataB[i];
                rlum[i] = Convert.ToUInt16(cumulatedR * 65535);
                glum[i] = Convert.ToUInt16(cumulatedG * 65535);
                blum[i] = Convert.ToUInt16(cumulatedB * 65535);
            }
            return LuminosityMultitpliersToFunction(rlum, glum, blum);
        }

        public static Func<ushort, ushort, ushort, Tuple<ushort, ushort, ushort>> HistogramMatch(PNM pnm)
        {
            return null;
        }


        private static Func<ushort, ushort, ushort, Tuple<ushort, ushort, ushort>> LuminosityMultitpliersToFunction(ushort[] rlum, ushort[] glum, ushort[] blum)
        {
            return (r, g, b) =>
                {
                    return Tuple.Create(rlum[r / 256], glum[g / 256], blum[b / 256]);
                };
        }
    }
}