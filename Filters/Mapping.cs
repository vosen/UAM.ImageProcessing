using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace UAM.PTO.Filters
{
    public static class Mapping
    {
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

        public static PNM ApplyHorizonMapping(this PNM image)
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
            PNM newImage = new PNM(image.Width, image.Height);
            for (int i = 0; i < size; i++)
            {
                newImage.SetPixel(i, northMap[i], northMap[i], northMap[i]);
            }
            return newImage;
        }

        private static byte[] GenerateHorizonNorth(float[] heightMap, int width, int height)
        {
            byte[] horizonMap = new byte[width * height];
            for (int i = 0; i < width; i++)
            {
                GenerateHorizonNorthStripe(heightMap, width, height, horizonMap, i);
            }
            return horizonMap;
        }


        // UGLINESS WARNING: 16 is a magic number to make visualization nicer
        private static void GenerateHorizonNorthStripe(float[] heightMap, int imageWidth, int imageHeight, byte[] horizonMap, int index)
        {
            List<int> horizon = new List<int>(imageHeight);
            horizon.Add(0);
            // func calculating element index in bitmap from element index in stripe
            Func<int, int> stripeIndex = (i) => { return StripeIndexNorth(imageWidth, index, i); };
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
                        double horizonDistance = 16 * (i - pos) / (double)imageHeight;
                        double pseudoAngle = Math.Atan(horizonHeight / horizonDistance) / (Math.PI/2);
                        if (pseudoAngle > highestAngle)
                        {
                            highestAngle = pseudoAngle;
                            highestIndex = i;
                        }
                    }
                    double pseudoAngleAbove = Math.Atan((heightMap[stripeIndex(i-1)] - height) * imageHeight / 16) / (Math.PI/2);
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

        private static int StripeIndexNorth(int width, int stripe, int index)
        {
            return (width * index) + stripe;
        }
    }
}
