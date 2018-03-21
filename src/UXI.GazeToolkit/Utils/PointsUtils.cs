using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UXI.GazeToolkit.Utils
{
    public static class PointsUtils
    {
        public static Point2 Interpolate(int step, Point2 start, Point2 end, int totalSteps)
        {
            var x = MathUtils.Interpolate(start.X, end.X, step, totalSteps);
            var y = MathUtils.Interpolate(start.Y, end.Y, step, totalSteps);

            return new Point2(x, y);
        }


        public static Point3 Interpolate(int step, Point3 start, Point3 end, int totalSteps)
        {
            var x = MathUtils.Interpolate(start.X, end.X, step, totalSteps);
            var y = MathUtils.Interpolate(start.Y, end.Y, step, totalSteps);
            var z = MathUtils.Interpolate(start.Z, end.Z, step, totalSteps);

            return new Point3(x, y, z);
        }


        public static Point2 Average(Point2 pointA, Point2 pointB)
        {
            var x = (pointA.X + pointB.X) / 2;
            var y = (pointA.Y + pointB.Y) / 2;

            return new Point2(x, y);
        }


        public static Point3 Average(Point3 pointA, Point3 pointB)
        {
            var x = (pointA.X + pointB.X) / 2;
            var y = (pointA.Y + pointB.Y) / 2;
            var z = (pointA.Z + pointB.Z) / 2;

            return new Point3(x, y, z);
        }


        public static double EuclidianDistance(Point3 pointA, Point3 pointB)
        {
            return Math.Sqrt(Math.Pow(pointB.X - pointA.X, 2) + Math.Pow(pointB.Y - pointA.Y, 2) + Math.Pow(pointB.Z - pointA.Z, 2));
        }
    }
}