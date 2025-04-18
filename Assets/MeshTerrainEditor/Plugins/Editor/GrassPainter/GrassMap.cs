using System.Collections.Generic;
using UnityEngine;

namespace MTE
{
    public static class GrassMap
    {
        public static IEnumerable<GrassItem> GetAllGrassItems()
        {
            var allPoints = PointCloud.GetAllPoints();
            foreach (var point in allPoints)
            {
                yield return point as GrassItem;
            }
        }

        public static void GetGrassItemsInCircle(Vector3 center, float radius, List<GrassItem> result)
        {
            result.Clear();
            var pointList = new List<IQuadObject>();
            PointCloud.GetPointsInCircle(center, radius, pointList);
            foreach (var point in pointList)
            {
                var item = point as GrassItem;
                System.Diagnostics.Debug.Assert(item != null, nameof(item) + " != null");
                if (item.Destroyed)
                {
                    continue;
                }
                result.Add(item);
            }
        }
        
        public static int GetGrassNumberInCircle(Vector3 center, float radius)
        {
            int result = 0;
            var pointList = new List<IQuadObject>();
            PointCloud.GetPointsInCircle(center, radius, pointList);
            foreach (var point in pointList)
            {
                var item = point as GrassItem;
                System.Diagnostics.Debug.Assert(item != null, nameof(item) + " != null");
                if (item.Destroyed)
                {
                    continue;
                }

                result++;
            }
            return result;
        }
        public static void Clear()
        {
            PointCloud.Clear();
        }

        public static void Insert(GrassItem grassItem)
        {
            PointCloud.Insert(grassItem);
        }

        public static void Remove(GrassItem point)
        {
            PointCloud.Remove(point);
        }

        private static readonly PointCloudMap PointCloud = new PointCloudMap();

        /// <summary>
        /// Filter grass positions, remove any point that intersects with existing grass items
        /// </summary>
        public static bool Intersect(Vector2 grassPosition, float grassSize)
        {
            return PointCloud.AnyPointInCircle(new Vector3(grassPosition.x, 0, grassPosition.y), grassSize);
        }
    }
}