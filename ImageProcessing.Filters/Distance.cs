using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UAM.PTO.Filters
{
    public static class Distance
    {
        public static PNM ApplyDistanceTransform(this PNM image)
        {
            PNM binaryImage = image.ApplyCannyDetector();
            int pixelCount = 0;
            double[][] distanceMap = ToInitialDistanceMap(binaryImage, ref pixelCount);
            CalculateDistances(distanceMap, pixelCount);
            PNM distancedImage = new PNM(image.Width, image.Height);
            for(int i=0; i < image.Width * image.Height; i++)
            {
                byte distance = Filter.Coerce(distanceMap[i % image.Width][i / image.Width]);
                distancedImage.SetPixel(i, distance, distance, distance);
            }
            return distancedImage;
        }

        private static void CalculateDistances(double[][] distanceMap, int pixelCount)
        {
            int width = distanceMap.GetLength(0);
            int height = distanceMap[0].GetLength(0);
            int size = width * height;
            int lastPixelCount;
            while (pixelCount < size)
            {
                lastPixelCount = pixelCount;
                double[][] newLayer = new double[width][];
                for (int i = 0; i < width; i++)
                    newLayer[i] = distanceMap[i].ToArray();

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (distanceMap[i][j] < double.PositiveInfinity)
                            UpdateNeighborhood(distanceMap, newLayer, i, j, width, height);
                    }
                }

                MergeNewLayer(distanceMap, newLayer, ref pixelCount, width, height);
                if (pixelCount == lastPixelCount)
                    break;
            }
        }

        private static void MergeNewLayer(double[][] distanceMap, double[][] newLayer, ref int pixelCount, int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (newLayer[i][j] != double.PositiveInfinity)
                    {
                        if (distanceMap[i][j] == double.PositiveInfinity)
                            pixelCount++;
                        distanceMap[i][j] = newLayer[i][j];
                    }
                }
            }
        }

        private static void UpdateNeighborhood(double[][] distanceMap, double[][] newLayer, int i, int j, int width, int height)
        {
            double neighborDistance = distanceMap[i][j] + 1;
            if (i > 0 && newLayer[i-1][j] > neighborDistance)
                newLayer[i-1][j] = neighborDistance;
            if (i < height -1 && newLayer[i+1][j] > neighborDistance)
                newLayer[i+1][j] = neighborDistance;
            if (j > 0 && newLayer[i][j-1] > neighborDistance)
                newLayer[i][j-1] = neighborDistance;
            if (j < width - 1 && newLayer[i][j+1] > neighborDistance)
                newLayer[i][j+1] = neighborDistance;
        }

        private static double[][] ToInitialDistanceMap(PNM image, ref int pixelCount)
        {
            double[][] distanceMap = new double[image.Width][];
            for(int i =0; i< image.Width; i++)
            {
                distanceMap[i] = new double[image.Height];
            }
            byte l;
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    image.GetPixel(i, j, out l, out l, out l);
                    if (l == 255)
                    {
                        distanceMap[i][j] = 0;
                        pixelCount++;
                    }
                    else
                    {
                        distanceMap[i][j] = Double.PositiveInfinity;
                    }
                }
            }
            return distanceMap;
        }
    }
}
