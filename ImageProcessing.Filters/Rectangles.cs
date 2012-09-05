using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace UAM.PTO.Filters
{
    public static class Rectangles
    {
        private static double AngularThreshold = Math.PI * 3 / 180;
        private static double DistanceThreshold = 3;
        private static double NormalizedThreshold = 0.4;

        public static PNM ApplyHoughRectanglesDetector(this PNM image, int dmax = 60, int dmin = 13)
        {
            PNM workImage = PNM.Copy(image).ApplyCannyDetector();
            double maxR = Math.Sqrt(Math.Pow(image.Width / 2d, 2) + Math.Pow(image.Height / 2d, 2));
            int padding = Math.Max((dmax + (dmax % 2)) /2, (dmin + (dmin % 2))/2);
            workImage.raster = Filter.PadWithZeros(workImage.raster, image.Width * 3, image.Height, padding * 3, padding);
            workImage.Width += padding * 2;
            workImage.Height += padding * 2;
            int maxW = (int)Math.Ceiling(maxR);
            bool[] circleMask = GenerateCircleMask(dmax, dmin);
            int imageSize = image.Width * image.Height;
            IEnumerable<Tuple<float, float>[]>[] allRectangles = new IEnumerable<Tuple<float, float>[]>[image.Width];
            Parallel.For(0, image.Width, i =>
            //for (int i = 0; i < image.Width; i++)
            {
                allRectangles[i] = Enumerable.Range(0, image.Height).Select(j =>
                {
                    int center = workImage.Width * (padding + (int)j) + padding + i;
                    var masked = MaskedHoughVote(workImage, center, circleMask, dmax, dmin, padding);
                    return masked;
                }).SelectMany(x => x).ToArray();
            //}
            });
            return DrawRectangles(image, allRectangles.SelectMany(x => x).ToArray());
        }

        private static bool[] GenerateCircleMask(int dmax, int dmin)
        {
            int width = dmax + 1 + (dmax % 2);
            bool[] circleMask = new bool[width * width];
            int center = width / 2;
            double maxDistance = dmax / 2d;
            double minDistance = dmin / 2d;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double distance = Edges.Module(center - i, center - j);
                    circleMask[j * width + i] = distance < maxDistance && distance > minDistance;
                }
            }
            return circleMask;
        }

        private static IEnumerable<Tuple<float, float>[]> MaskedHoughVote(PNM image, int center, bool[] mask, int dmax, int dmin, int padding)
        {
            double angleStep = (Math.PI * 3) / (4d * dmax);
            int angles = (int)Math.Floor(Math.PI / angleStep);
            double distanceStep = 3/4d;
            int distances = (int)Math.Ceiling(dmax / distanceStep);
            int[][] votingBoard = new int[angles][];
            for (int i = 0; i < angles; i++)
                votingBoard[i] = new int[distances];
            int maskSize = mask.Length;
            int maskIndex = 0;
            for (int i = -dmax/2; i <= dmax/2; i++)
            {
                for (int j = -dmax / 2; j <= dmax/2; j++)
                {
                    if (!mask[maskIndex++])
                        continue;
                    int x = center % image.Width + i;
                    int y = center / image.Width - j;
                    int realIndex = (y * image.Width) + x;
                    byte l;
                    image.GetPixel(realIndex, out l, out l, out l);
                    if (l < 255)
                        continue;
                    for (int angle = 0; angle < angles; angle++)
                    {
                        double radianAngle = angle * angleStep;
                        double w = i * Math.Cos(radianAngle) + j * Math.Sin(radianAngle);
                        double normalizedW = w + (dmax / 2d);
                        int steppedW = (int)(normalizedW / distanceStep);
                        votingBoard[angle][steppedW]++;
                    }
                }
            }
            // votingboard is full - enhance it now
            List<double[]> peaks = EnhanceHoughVotingBoard(votingBoard, dmax, dmin, angleStep, distanceStep);
            List<Tuple<int, int, double>> extendedPeaks = new List<Tuple<int, int, double>>(peaks.Count / 2);
            for (int i = 0; i < peaks.Count; i++)
            {
                for (int j = i + 1; j < peaks.Count; j++)
                {
                    if (Math.Abs(peaks[i][0] - peaks[j][0]) < AngularThreshold
                       && Math.Abs(peaks[i][1] + peaks[j][1]) < DistanceThreshold
                       && Math.Abs(peaks[i][2] - peaks[j][2]) < NormalizedThreshold * (peaks[i][2] + peaks[j][2]) / 2)
                    extendedPeaks.Add(Tuple.Create(i, j, 0.5 * (peaks[i][0] + peaks[j][0])));
                }
            }
            // extendedPeaks now holds Tuples of (i, j, ak), where i and j are indices of paired peaks and their alpha_k
            List<Tuple<double[][], double[][]>> finalPairs = new List<Tuple<double[][], double[][]>>();
            for (int i = 0; i < extendedPeaks.Count; i++)
            {
                for (int j = i + 1; j < extendedPeaks.Count; j++)
                {
                    if (Math.Abs(Math.Abs(extendedPeaks[i].Item3 - extendedPeaks[j].Item3) - (Math.PI / 2)) < AngularThreshold)
                        // we got pairs of peak pairs
                        finalPairs.Add(Tuple.Create( new double[][] {peaks[extendedPeaks[i].Item1], peaks[extendedPeaks[i].Item2] },  new double[][] { peaks[extendedPeaks[j].Item1], peaks[extendedPeaks[j].Item2] }));
                }
            }
            return finalPairs.Select(pair => new Tuple<double, double>[] { 
                    PolarLineIntersection(pair.Item1[0][0], pair.Item1[0][1], pair.Item2[0][0], pair.Item2[0][1]),
                    PolarLineIntersection(pair.Item1[1][0], pair.Item1[1][1], pair.Item2[0][0], pair.Item2[0][1]),
                    PolarLineIntersection(pair.Item1[0][0], pair.Item1[0][1], pair.Item2[1][0], pair.Item2[1][1]),
                    PolarLineIntersection(pair.Item1[1][0], pair.Item1[1][1], pair.Item2[1][0], pair.Item2[1][1])})
                .Select(tups => new Tuple<float, float>[]{
                    CorrectCoordinates(tups[0].Item1, tups[0].Item2, center, image.Width, image.Height, padding),
                    CorrectCoordinates(tups[1].Item1, tups[1].Item2, center, image.Width, image.Height, padding),
                    CorrectCoordinates(tups[2].Item1, tups[2].Item2, center, image.Width, image.Height, padding),
                    CorrectCoordinates(tups[3].Item1, tups[3].Item2, center, image.Width, image.Height, padding)});
        }

        private static Tuple<float, float> CorrectCoordinates(double x, double y, int center, int width, int height, int padding)
        {
            int centerX = center % width;
            int centerY = center / width;
            return Tuple.Create(centerX + (float)x - padding, centerY - (float)y - padding);
        }

        // returns peaks as {angle, distance}
        private static List<double[]> EnhanceHoughVotingBoard(int[][] votingBoard, double dmax, double dmin, double angleStep, double distanceStep)
        {
            int width = votingBoard.GetLength(0);
            int height = votingBoard[0].GetLength(0);
            double[][] convoluted = QuickSum(votingBoard);
            List<double[]> peaks = new List<double[]>(16);
            double threshold = dmax / 2;
            for (int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    if (25 * Math.Pow(votingBoard[i][j], 2) / convoluted[i][j] > threshold)
                        peaks.Add(new double[] { i * angleStep, (j * distanceStep) - (dmax / 2), votingBoard[i][j] });
                }
            }
            return peaks;
        }

        private static Tuple<double, double> PolarLineIntersection(double a1, double r1, double a2, double r2)
        {
            // convert to cartesian
            double A1 = Math.Cos(a1);
            double B1 = Math.Sin(a1);
            double C1 = -r1;
            double A2 = Math.Cos(a2);
            double B2 = Math.Sin(a2);
            double C2 = -r2;
            double W = (A1 * B2) - (A2 * B1);
            double Wx = (-C1 * B2) - (- C2 * B1);
            double Wy = (A1 *-C2) - (A2 * -C1);
            return Tuple.Create(Wx / W, Wy / W);
        }

        // Summing over whole array in a 5x5 window
        private static double[][] QuickSum(int[][] votingBoard)
        {
            int width = votingBoard.GetLength(0);
            int height = votingBoard[0].GetLength(0);
            double[][] summed = new double[width][];
            for (int i = 0; i < width; i++)
                summed[i] = new double[height];
            // fill top-bottom borders
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int xStart = i < 2 ? 0 : i - 2;
                    int xEnd = i + 2 < width ? i + 2 : width - 1;
                    int yUpStart = j < 2 ? 0 : j - 2;
                    // sum the neighbourhood of (i,j)
                    for (int x0 = xStart; x0 <= xEnd; x0++)
                    {
                        for (int y0 = yUpStart; y0 <= j + 2; y0++)
                        {
                            summed[i][j] += votingBoard[x0][y0];
                        }
                    }
                    if (j == 2)
                        break;
                    // lets' not waste perfectly good loop
                    // and sum the neighbourhood of (i, height-j-1)
                    int yDownEnd = j < 2 ? height - 1 : j - 2;
                    for (int x0 = xStart; x0 <= xEnd; x0++)
                    {
                        for (int y0 = height - j - 3; y0 < height; y0++)
                        {
                            summed[i][height - j - 1] += votingBoard[x0][y0];
                        }
                    }
                }
            }
            // fill left-right borders
            int leftFillHeight = height - 2;
            for (int j = 3; j < leftFillHeight; j++)
            {
                for (int i = 0; i < 2; i++)
                {
                    int xLeftEnd = i + 2;
                    int yEnd = j + 2;
                    for (int x0 = 0; x0 <= xLeftEnd; x0++)
                    {
                        for (int y0 = j - 2; y0 <= yEnd; y0++)
                        {
                            summed[i][j] += votingBoard[x0][y0];
                        }
                    }
                    // copy from left side
                    for (int x0 = width - i - 3; x0 < width; x0++)
                    {
                        for (int y0 = j - 2; y0 <= yEnd; y0++)
                        {
                            summed[width - i - 1][j] += votingBoard[x0][y0];
                        }
                    }
                }
            }
            // fill insides
            int fillWidth = width -2;
            for (int i = 2; i < fillWidth; i++)
            {
                for (int j = 3; j < leftFillHeight; j++)
                {
                    summed[i][j] = summed[i][j - 1]
                                   - votingBoard[i - 2][j - 3] + votingBoard[i - 2][j + 2]
                                   - votingBoard[i - 1][j - 3] + votingBoard[i - 1][j + 2]
                                   - votingBoard[i][j - 3] + votingBoard[i][j + 2]
                                   - votingBoard[i + 1][j - 3] + votingBoard[i + 1][j + 2]
                                   - votingBoard[i + 2][j - 3] + votingBoard[i + 2][j + 2];
                }
            }
            return summed;
        }

        private static PNM DrawRectangles(PNM image, Tuple<float, float>[][] corners)
        {
            using (Bitmap bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb))
            {
                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                byte[] stridedBuffer = Lines.Stride(image.raster, image.Width, image.Height);
                Marshal.Copy(stridedBuffer, 0, data.Scan0, stridedBuffer.Length);
                bitmap.UnlockBits(data);
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    // draw the lines
                    foreach (var corner in corners)
                    {
                        for (int i = 0; i < 4; i++)
                            for (int j = i + 1; j < 4; j++ )
                                graphics.DrawLine(Pens.Blue, corner[i].Item1, corner[i].Item2, corner[j].Item1, corner[j].Item2);
                    }
                }
                // get raw data
                PNM rectImage = new PNM(image.Width, image.Height);
                data = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                rectImage.raster = Lines.UnStride(data.Scan0, image.Width, image.Height);
                bitmap.UnlockBits(data);
                return rectImage;
            }
        }
    }
}
