using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace UAM.PTO.Filters
{
    public static class LineDetection
    {
        public static PNM ApplyHoughTransform(this PNM image)
        {
            // greyscale, edge detection, thresholding
            PNM workImage = PNM.Copy(image).ApplyPointProcessing(Color.ToGrayscale)
                                           .ApplyConvolutionMatrix(new float[]{  0,  0, -1, -1, -1,  0,  0,
                                                                                 0, -2, -3, -3, -3, -2,  0,
                                                                                -1, -3,  5,  5,  5, -3, -1,
                                                                                -1, -3,  5, 16,  5, -3, -1,
                                                                                -1, -3,  5,  5,  5, -3, -1,
                                                                                 0, -2, -3, -3, -3, -2,  0,
                                                                                 0,  0, -1, -1, -1,  0,  0}, 1, 0)
                                           .ApplyPointProcessing(Thresholding.Entropy(image));
            IEnumerable<Tuple<Point, Point>> lines = GenerateHoughLines(workImage);
            return DrawLines(image, lines);
        }

        private static PNM DrawLines(PNM image, IEnumerable<Tuple<Point, Point>> lines)
        {
            // prepare the bitmap
            using (Bitmap bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb))
            {
                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                byte[] stridedBuffer = Stride(image.raster, image.Width, image.Height);
                Marshal.Copy(stridedBuffer, 0, data.Scan0, stridedBuffer.Length);
                bitmap.UnlockBits(data);
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    // draw the lines
                    foreach(var tuple in lines)
                    {
                        graphics.DrawLine(Pens.Blue, tuple.Item1, tuple.Item2);
                    }
                }
                // get raw data
                PNM lineImage = new PNM(image.Width, image.Height);
                data = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                lineImage.raster = UnStride(data.Scan0, image.Width, image.Height);
                bitmap.UnlockBits(data);
                return lineImage;
            }
        }

        private static byte[] Stride(byte[] array, int width, int height)
        {
            int stridedWidth = width * 3;
            int overflow = (stridedWidth % 4);
            if (overflow == 0)
                return array;
            stridedWidth += (4 - overflow);
            byte[] stridedBuffer = new byte[stridedWidth * height];
            for (int i = 0; i < height; i++)
            {
                Buffer.BlockCopy(array, i * width * 3, stridedBuffer, stridedWidth * i, width * 3);
            }
            return stridedBuffer;
        }

        private static byte[] UnStride(IntPtr array, int width, int height)
        {
            int stridedWidth = width * 3;
            int overflow = (stridedWidth % 4);
            if (overflow == 0)
                overflow = 4;
            stridedWidth += (4 - overflow);
            byte[] unstridedBuffer = new byte[width * height * 3];
            for (int i = 0; i < height; i++)
            {
                Marshal.Copy(array + (stridedWidth * i), unstridedBuffer, width * i * 3, width * 3);
            }
            return unstridedBuffer;
        }


        // returns list tuples which encode interesection of lines with image edges
        private static IEnumerable<Tuple<Point, Point>> GenerateHoughLines(PNM image)
        {
            // initialize voting board 
            double maxR = Math.Sqrt(Math.Pow(image.Width / 2d, 2) + Math.Pow(image.Height / 2d, 2));
            int maxW = (int)Math.Ceiling(maxR);
            // angle values in board are stored relative to (-1,0) axis, counterclockwise
            int[][] votingBoard = new int[180][];
            for (int i = 0; i < 180; i++)
                votingBoard[i] = new int[maxW * 2];
            int size = image.Width * image.Height;
            for (int i = 0; i < size; i++)
            {
                byte l;
                image.GetPixel(i, out l, out l, out l);
                if (l < 255)
                    continue;
                double centeredX = i % image.Width - (image.Width / 2d);
                double centeredY = (image.Height / 2d) - i / image.Width;
                // pixel is white - vote in all directions
                for (int angle = 0; angle < 180; angle++)
                {
                    double radianAngle = Math.PI * angle / 180d;
                    double w = centeredX * Math.Cos(radianAngle) + centeredY * Math.Sin(radianAngle);
                    votingBoard[angle][(int)w + maxW]++;
                }
            }
            // prepare function
            Func<double, double, Tuple<Point, Point>> polarLinesToEdges = (dist, ang) => PolarLineToImageEdges(dist, ang, image.Width, image.Height);
            // pick top 10
            // MADNESS
            return votingBoard
                //.AsParallel()
                   .SelectMany((array, angle) => array.Select((count, distance) => new { Angle = angle, Distance = distance, Count = count }))
                   .OrderByDescending((a) => a.Count)
                   .Take(20)
                   .Select((a) => new { Distance = a.Distance - maxW, Angle = Math.PI * a.Angle / 180d })
                   .Select((a) => polarLinesToEdges(a.Distance, a.Angle))
                   .Where(tupl => tupl != null);
        }

        private static Tuple<Point, Point> PolarLineToImageEdges(double w, double angle, double width, double height)
        {
            if (angle >= Math.PI / 4 && angle < Math.PI * 3 / 4)
                return PolarLineToImageEdgesX(w, angle, width, height);
            return PolarLineToImageEdgesY(w, angle, width, height);
        }

        private static Tuple<Point, Point> PolarLineToImageEdgesX(double w, double angle, double width, double height)
        {
            double A = Math.Cos(angle);
            double B = Math.Sin(angle);
            double yLeft = (w - A * (-width / 2)) / B;
            double yRight = (w - A * (width / 2)) / B;
            // now convert those points to image coordinates
            double y1 = (height / 2) - yLeft;
            double y2 = (height / 2) - yRight;
            return Tuple.Create(new Point(0, (int)y1), new Point((int)width, (int)y2));
        }

        private static Tuple<Point, Point> PolarLineToImageEdgesY(double w, double angle, double width, double height)
        {
            double A = Math.Cos(angle);
            double B = Math.Sin(angle);
            double xUp = ((height / 2) * B - w) / -A;
            double xDown = ((-height / 2) * B - w) / -A;
            // now convert those points to image coordinates
            double x1 = xUp + (width / 2);
            double x2 = xDown + (width / 2);
            return Tuple.Create(new Point((int)x1, 0), new Point((int)x2, (int)height));
        }
    }
}
