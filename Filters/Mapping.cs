using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Threading.Tasks;

namespace UAM.PTO.Filters
{
    public static class Mapping
    {
        // 16 is a magic number to make visualization nicer
        private static int MagicHorizonNumber = 16;

        private static byte Pack(double v)
        {
            return Filter.Coerce((v + 1) * 127.5d);
        }

        public static Pixel Normal(int width, int height, float[] heightMap, int index)
        {
            byte val = Convert.ToByte(heightMap[index] * 255);
            Vector3D v10 = new Vector3D(0, -1, heightMap[Filter.RelativeIndex(width, index, 0, -1)] - heightMap[index]);
            Vector3D v21 = new Vector3D(1, 0, heightMap[Filter.RelativeIndex(width, index, 1, 0)] - heightMap[index]);
            Vector3D v12 = new Vector3D(0, 1, heightMap[Filter.RelativeIndex(width, index, 0, 1)] - heightMap[index]);
            Vector3D v01 = new Vector3D(-1, 0, heightMap[Filter.RelativeIndex(width, index, -1, 0)] - heightMap[index]);
            Vector3D normal = Vector3D.CrossProduct(v10, v21)
                              + Vector3D.CrossProduct(v21, v12)
                              + Vector3D.CrossProduct(v12, v01)
                              + Vector3D.CrossProduct(v01, v10);
            normal.Normalize();
            return new Pixel(Pack(normal.X), Pack(normal.Y), Pack(normal.Z));
        }

        // returns SENW directions as BGRA32 raw byte array
        public static byte[] GenerateHorizonMapping(this PNM image)
        {
            int size = image.Width * image.Height;
            float[] heightMap = new float[size];
            byte r, g, b;
            for (int i = 0; i < size; i++)
            {
                image.GetPixel(i, out r, out g, out b);
                heightMap[i] = PNM.RGBToLuminosity(r, g, b) / 255f;
            }
            byte[] northMap = GenerateHorizonNorth(heightMap, image.Width, image.Height);
            byte[] southMap = GenerateHorizonSouth(heightMap, image.Width, image.Height);
            byte[] eastMap = GenerateHorizonEast(heightMap, image.Width, image.Height);
            byte[] westMap = GenerateHorizonWest(heightMap, image.Width, image.Height);
            byte[] rawBitmap = new byte[size * 4];
            for (int i = 0; i < size; i++)
            {
                rawBitmap[i * 4] = southMap[i];
                rawBitmap[1 + (i * 4)] = eastMap[i];
                rawBitmap[2 + (i * 4)] = northMap[i];
                rawBitmap[3 + (i * 4)] = westMap[i];
            }
            return rawBitmap;
        }

        private static byte[] GenerateHorizonNorth(float[] heightMap, int width, int height)
        {
            return GenerateHorizon(heightMap, width, height, StripeIndexerNorth);
        }

        private static byte[] GenerateHorizonSouth(float[] heightMap, int width, int height)
        {
            return GenerateHorizon(heightMap, width, height, StripeIndexerSouth);
        }

        private static byte[] GenerateHorizonEast(float[] heightMap, int width, int height)
        {
            return GenerateHorizon(heightMap, height, width, StripeIndexerEast);
        }

        private static byte[] GenerateHorizonWest(float[] heightMap, int width, int height)
        {
            return GenerateHorizon(heightMap, height, width, StripeIndexerWest);
        }

        // So ugly, yet so beautiful
        private static byte[] GenerateHorizon(float[] heightMap,
                                              int width,
                                              int height,
                                              Func<int,int,int,int,int> stripeIndexer)
        {
            byte[] horizonMap = new byte[width * height];
            for (int i = 0; i < width; i++)
            {
                GenerateStripe(heightMap, width, height, horizonMap, i, stripeIndexer);
            }
            return horizonMap;
        }

        private static void GenerateStripe(float[] heightMap,
                                                  int imageWidth,
                                                  int imageHeight,
                                                  byte[] horizonMap,
                                                  int index,
                                                  Func<int, int, int, int, int> stripeIndexer)
        {
            List<int> horizon = new List<int>(imageHeight);
            horizon.Add(0);
            // func calculating element index in bitmap from element index in stripe
            Func<int, int> stripeIndex = (i) => { return stripeIndexer(imageWidth, imageHeight, index, i); };
            for (int i = 0; i < imageHeight; i++)
            {
                int arrayIndex = stripeIndex(i);
                float height = heightMap[arrayIndex];
                // check if we are equal or larger than the highest horizon point
                if (height >= heightMap[stripeIndex(horizon[0])])
                {
                    horizonMap[arrayIndex] = 0;
                    horizon.Clear();
                    horizon.Add(i);
                }
                else
                {
                    double highestAngle = 0;
                    int highestIndex = -1;
                    // check all the past horizons
                    foreach (int pos in horizon)
                    {
                        double horizonHeight = heightMap[stripeIndex(pos)] - height;
                        double horizonDistance = MagicHorizonNumber * (i - pos) / (double)imageHeight;
                        double pseudoAngle = Math.Atan(horizonHeight / horizonDistance) / (Math.PI/2);
                        if (pseudoAngle > highestAngle)
                        {
                            highestAngle = pseudoAngle;
                            highestIndex = i;
                        }
                    }
                    double pseudoAngleAbove = Math.Atan((heightMap[stripeIndex(i - 1)] - height) * imageHeight / MagicHorizonNumber) / (Math.PI / 2);
                    // check special case for pixel above in stripe
                    if (pseudoAngleAbove > highestAngle)
                    {
                        // replace highest angle
                        highestAngle = pseudoAngleAbove;
                        // add index to horizon list
                        horizon.Add(i - 1);
                    }
                    horizonMap[arrayIndex] = Filter.Coerce(highestAngle * 255);
                }
            }
        }

        private static int StripeIndexerNorth(int width, int height, int stripe, int index)
        {
            return (width * index) + stripe;
        }

        private static int StripeIndexerSouth(int width, int height, int stripe, int index)
        {
            return StripeIndexerNorth(width, height, stripe, (height - index - 1));
        }

        private static int StripeIndexerWest(int height, int width, int stripe, int index)
        {
            return StripeIndexerNorth(width, height, index, stripe);
        }

        private static int StripeIndexerEast(int height, int width, int stripe, int index)
        {
            return StripeIndexerSouth(width, height, (width - index - 1), stripe);
        }
    }
}
