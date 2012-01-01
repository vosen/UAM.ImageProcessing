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
    }
}
