using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAM.PTO.Filters
{
    public static class Rectangles
    {
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
            Parallel.For(0, image.Width, i =>
            {
                for (int j = 0; j < image.Height; j++)
                {
                    int center = workImage.Height * (padding + j) + padding + i;
                    MaskedHoughVote(workImage, center, circleMask, dmax, dmin);
                }
            });
            return workImage;
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

        private static void MaskedHoughVote(PNM image, int center, bool[] mask, int dmax, int dmin)
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
                    int y = center / image.Width + j;
                    int realIndex = (y * image.Width) + x;
                    byte l;
                    image.GetPixel(realIndex, out l, out l, out l);
                    if (l < 255)
                        continue;
                    for (int angle = 0; angle < angles; angle++)
                    {
                        double radianAngle = angle * angleStep;
                        double w = i * Math.Cos(radianAngle) + j * Math.Sin(radianAngle);
                        int normalizedW = (int)(w + (dmax / 2d));
                        votingBoard[angle][normalizedW]++;
                    }
                }
            }
            // votingboard is full - enhance it now
            var peaks = EnhanceHoughVotingBoard(votingBoard, dmin);
        }

        private static List<int> EnhanceHoughVotingBoard(int[][] votingBoard, double dmin)
        {
            int width = votingBoard.GetLength(0);
            int height = votingBoard[0].GetLength(0);
            double[][] convoluted = QuickSum(votingBoard);
            List<int> peaks = new List<int>(16);
            double threshold = dmin / 2;
            for (int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    if (25 * Math.Pow(votingBoard[i][j], 2) / convoluted[i][j] > threshold)
                        peaks.Add(i);
                }
            }
            return peaks;
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
                                   - summed[i - 2][j - 3] + summed[i - 2][j + 2]
                                   - summed[i - 1][j - 3] + summed[i - 1][j + 2]
                                   - summed[i][j - 3] + summed[i][j + 2]
                                   - summed[i + 1][j - 3] + summed[i + 1][j + 2]
                                   - summed[i + 2][j - 3] + summed[i + 2][j + 2];
                }
            }
            return summed;
        }
    }
}
