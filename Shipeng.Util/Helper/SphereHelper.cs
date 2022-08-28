using NetTopologySuite.Mathematics;

namespace Shipeng.Util.Helper
{
    /// <summary>
    /// 地球操作类
    /// </summary>
    public class SphereHelper
    {
        /// <summary>
        /// 地球
        /// </summary>
        public static SphereHelper Earth => new(6371.393);

        public SphereHelper(double radius)
        {
            Radius = radius;
        }

        /// <summary>
        /// 半径
        /// </summary>
        public double Radius { get; }

        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="d">角度</param>
        /// <returns></returns>
        private static double Angle2Radian(double d)
        {
            return d / 180.0 * Math.PI;
        }

        /// <summary>
        /// 计算球体上两点的弧长
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <returns></returns>
        public double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double rad1 = Angle2Radian(lat1);
            double rad2 = Angle2Radian(lat2);
            return 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin((rad1 - rad2) / 2), 2) + Math.Cos(rad1) * Math.Cos(rad2) * Math.Pow(Math.Sin((Angle2Radian(lng1) - Angle2Radian(lng2)) / 2), 2))) * Radius;
        }

        /// <summary>
        /// 是否相交
        /// </summary>
        /// <param name="circle1"></param>
        /// <param name="circle2"></param>
        /// <returns></returns>
        public bool IsCrossWith(Circle circle1, Circle circle2)
        {
            double dis = GetDistance(circle1.Center.X, circle1.Center.Y, circle2.Center.X, circle2.Center.Y);
            return circle1.Radius - circle2.Radius < dis && dis < circle1.Radius + circle2.Radius;
        }

        /// <summary>
        /// 是否相切
        /// </summary>
        /// <param name="circle1"></param>
        /// <param name="circle2"></param>
        /// <returns></returns>
        public bool IsIntersectWith(Circle circle1, Circle circle2)
        {
            double dis = GetDistance(circle1.Center.X, circle1.Center.Y, circle2.Center.X, circle2.Center.Y);
            return Math.Abs(circle1.Radius - circle2.Radius - dis) < 1e-7 || Math.Abs(dis - (circle1.Radius + circle2.Radius)) < 1e-7;
        }

        /// <summary>
        /// 是否相离
        /// </summary>
        /// <param name="circle1"></param>
        /// <param name="circle2"></param>
        /// <returns></returns>
        public bool IsSeparateWith(Circle circle1, Circle circle2)
        {
            return !IsCrossWith(circle1, circle2) && !IsIntersectWith(circle1, circle2);
        }
    }

    /// <summary>
    /// 圆形
    /// </summary>
    public class Circle
    {
        /// <summary>
        /// 圆形
        /// </summary>
        /// <param name="center">圆心坐标</param>
        /// <param name="radius">半径</param>
        public Circle(Point2D center, double radius)
        {
            Center = center;
            Radius = radius;
            if (radius < 0)
            {
                throw new ArgumentException("半径不能为负数", nameof(radius));
            }
        }

        /// <summary>
        /// 圆心坐标
        /// </summary>
        public Point2D Center { get; }

        /// <summary>
        /// 半径
        /// </summary>
        public double Radius { get; }

        /// <summary>
        /// 是否相交
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public bool IsCrossWith(Circle that)
        {
            double dis = Math.Sqrt(Math.Pow(that.Center.X - Center.X, 2) + Math.Pow(that.Center.Y - Center.Y, 2));
            return that.Radius - Radius < dis && dis < that.Radius + Radius;
        }

        /// <summary>
        /// 是否相切
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public bool IsIntersectWith(Circle that)
        {
            double dis = Math.Sqrt(Math.Pow(that.Center.X - Center.X, 2) + Math.Pow(that.Center.Y - Center.Y, 2));
            return Math.Abs(that.Radius - Radius - dis) < 1e-7 || Math.Abs(dis - (that.Radius + Radius)) < 1e-7;
        }

        /// <summary>
        /// 是否相离
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public bool IsSeparateWith(Circle that)
        {
            return !IsCrossWith(that) && !IsIntersectWith(that);
        }
    }

    public class Point2D
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Vector2D operator -(Point2D first, Point2D second)
        {
            return new Vector2D(first.X - second.X, first.Y - second.Y);
        }

        public static Point2D operator +(Point2D pt, Vector2D vec)
        {
            return new Point2D(pt.X + vec.X, pt.Y + vec.Y);
        }
    }

    public class RadarChart
    {
        /// <summary>
        /// 向量长度集合
        /// </summary>
        public List<double> Data { get; }

        /// <summary>
        /// 起始弧度
        /// </summary>
        public double StartAngle { get; } // 弧度

        /// <summary>
        /// 多边形
        /// </summary>
        /// <param name="data">向量长度集合</param>
        /// <param name="startAngle">起始弧度</param>
        public RadarChart(List<double> data, double startAngle = 0)
        {
            Data = new List<double>(data);
            StartAngle = startAngle;
        }

        /// <summary>
        /// 获取每个点的坐标
        /// </summary>
        /// <returns></returns>
        public List<Point2D> GetPoints()
        {
            int count = Data.Count;
            List<Point2D> result = new List<Point2D>();
            for (int i = 0; i < count; i++)
            {
                double length = Data[i];
                double angle = StartAngle + Math.PI * 2 / count * i;
                double x = length * Math.Cos(angle);
                double y = length * Math.Sin(angle);
                result.Add(new Point2D(x, y));
            }

            return result;
        }
    }

    /// <summary>
    /// 雷达图引擎
    /// </summary>
    public static class RadarChartEngine
    {
        /// <summary>
        /// 计算多边形面积的函数
        /// (以原点为基准点,分割为多个三角形)
        /// 定理：任意多边形的面积可由任意一点与多边形上依次两点连线构成的三角形矢量面积求和得出。矢量面积=三角形两边矢量的叉乘。
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static double ComputeArea(this List<Point2D> points)
        {
            double area = 0;
            int count = points.Count;
            for (int i = 0; i < count; i++)
            {
                area = area + (points[i].X * points[(i + 1) % count].Y - points[(i + 1) % count].X * points[i].Y);
            }

            return Math.Abs(0.5 * area);
        }
    }
}