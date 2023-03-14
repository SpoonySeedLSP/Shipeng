namespace Shipeng.Util
{
    /// <summary>
    /// 地图计算帮助类
    /// Author:李仕鹏
    /// 判断经纬度是否在区域内，是否偏离线路
    /// 主要原理：
    ///        判断点是否在区域内(光线投射算法)：
    ///                                       求解从该点向右发出的水平线射线与多边形各边的交点，当交点数为奇数，则在内部
    ///        不过要注意几种特殊情况：
    ///                              1、点在边或者顶点上;
    ///                              2、点在边的延长线上;
    ///                              3、点出发的水平射线与多边形相交在顶点上
    /// 判是否偏离线路：
    ///            求点到直线的垂足，获取最小值判断是否超过预设的距离
    ///            1.公式
    ///                 设直线方程为ax+by+c=0,点坐标为(m, n)
    ///                 则垂足为((b* b* m-a* b*n-a* c)/(a* a+b* b),(a* a* n-a* b* m-b* c)/(a* a+b* b))
    ///            2.计算点到线段的最近点
    ///                 如果该线段平行于X轴（Y轴），则过点point作该线段所在直线的垂线，垂足很容易求得，然后计算出垂足，如果垂足在线段上则返回垂足，否则返回离垂足近的端点；
    ///                 如果该线段不平行于X轴也不平行于Y轴，则斜率存在且不为0。设线段的两端点为pt1和pt2，斜率为：
    ///                 k = ( pt2.y - pt1. y ) / (pt2.x - pt1.x );
    ///                 该直线方程为：
    ///                 y = k* ( x - pt1.x) + pt1.y
    ///                 其垂线的斜率为 - 1 / k，
    ///                 垂线方程为：
    ///                 y = (-1/k) * (x - point.x) + point.y
    ///                 联立两直线方程解得：
    ///                 x = ( k^2 * pt1.x + k * (point.y - pt1.y ) + point.x ) / ( k^2 + 1)
    ///                 y = k * ( x - pt1.x) + pt1.y;
    /// </summary>
    public class MapHelper
    {
        /// <summary>
        /// 地球半径,单位 m
        /// </summary>
        private static double EARTH_RADIUS = 6378137.0;

        /// <summary>
        /// 判断点是否在多边形内或多边形上
        /// </summary>
        /// <param name="point">坐标点</param>
        /// <param name="points">多边形边界点集合</param>
        /// <returns></returns>
        public static bool IsPtInPoly(MapPoint point, MapPoint[] points)
        {
            double ALon = point.x, ALat = point.y;
            int iSum, iCount, iIndex;
            double dLon1 = 0, dLon2 = 0, dLat1 = 0, dLat2 = 0, dLon;
            if (points.Length < 3)
            {
                return false;
            }
            iSum = 0;
            iCount = points.Length;
            for (iIndex = 0; iIndex < iCount; iIndex++)
            {
                if (ALon == points[iIndex].x && ALat == points[iIndex].y)  //A点在多边形上    
                    return true;

                if (iIndex == iCount - 1)
                {
                    dLon1 = points[iIndex].x;
                    dLat1 = points[iIndex].y;
                    dLon2 = points[0].x;
                    dLat2 = points[0].y;
                }
                else
                {
                    dLon1 = points[iIndex].x;
                    dLat1 = points[iIndex].y;
                    dLon2 = points[iIndex + 1].x;
                    dLat2 = points[iIndex + 1].y;
                }

                //以下语句判断A点是否在边的两端点的纬度之间，在则可能有交点
                if (((ALat > dLat1) && (ALat < dLat2)) || ((ALat > dLat2) && (ALat < dLat1)))
                {
                    if (Math.Abs(dLat1 - dLat2) > 0)
                    {
                        //获取A点向左射线与边的交点的x坐标：
                        dLon = dLon1 - ((dLon1 - dLon2) * (dLat1 - ALat)) / (dLat1 - dLat2);
                        //如果交点在A点左侧，则射线与边的全部交点数加一：
                        if (dLon < ALon)
                        {
                            iSum++;
                        }
                        //如果相等，则说明A点在边上
                        if (dLon == ALon)
                            return true;
                    }
                }
            }
            if ((iSum % 2) != 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断是否偏离航线在允许范围内
        /// </summary>
        /// <param name="point">实时点用于判断此点是否偏离航线</param>
        /// <param name="points">航线组成的点坐标</param>
        /// <param name="allowRange">允许偏离航线的距离 单位：m</param>
        /// <returns>true -- 未偏离航线 ; false -- 偏离航线</returns>
        public static bool PointToPintLine(MapPoint point, List<MapPoint> points, double allowRange)
        {
            double minDistance = -1;

            for (int i = 0; i < points.Count - 1; i++)
            {
                if (points[i].x == points[i + 1].x && points[i].y == points[i + 1].y)
                {
                    continue;
                }
                /**
                 * 获取线段的x取值范围和Y的取值范围
                 */
                double[] rangeX = new double[2];
                double[] rangeY = new double[2];
                if (points[i].x > points[i + 1].x)
                {
                    rangeX[0] = points[i + 1].x;
                    rangeX[1] = points[i].x;
                }
                else
                {
                    rangeX[0] = points[i].x;
                    rangeX[1] = points[i + 1].x;
                }

                if (points[i].y > points[i + 1].y)
                {
                    rangeY[0] = points[i + 1].y;
                    rangeY[1] = points[i].y;
                }
                else
                {
                    rangeY[0] = points[i].y;
                    rangeY[1] = points[i + 1].y;
                }

                /**
                 * 根据两点求出直线方程AX+BY+C=0中，A B C 的值
                 */
                double a = points[i + 1].y - points[i].y;
                double b = points[i].x - points[i + 1].x;
                double c = points[i + 1].x * points[i].y - points[i].x * points[i + 1].y;

                /**
                 * 求点到直线的垂足以及距离
                 */
                //得到垂足点
                MapPoint foot = GetFootOfPerpendicular(point.x, point.y, a, b, c);
                //得到距离
                double distance = GetDistance(point.x, point.y, foot.x, foot.y);

                /**
                 * 判断垂足是否在线段上
                 */
                if (foot.x >= rangeX[0] && foot.x <= rangeX[1] &&
                        foot.y >= rangeY[0] && foot.y <= rangeY[1])
                {
                    /**
                     * 1.如果在线段上则记录值
                     * 2.跟minDistance比较，如果小于目前值则进行替换(若是初始值(-1)也进行替换)
                     */
                    if ((minDistance == -1) || (minDistance != -1 && distance < minDistance))
                    {
                        minDistance = distance;
                    }
                }
                else
                {
                    //计算点距离
                    double startPointDistance = GetDistance(point.x, point.y, points[i].x, points[i].y);
                    double endPointDistance = GetDistance(point.x, point.y, points[i + 1].x, points[i + 1].y);
                    distance = (startPointDistance <= endPointDistance ? startPointDistance : endPointDistance);
                    if (minDistance == -1 || minDistance > distance)
                    {
                        minDistance = distance;
                    }
                }

            }
            /**
             * 1.看是否minDistance是否是初始值，
             * 2.如果是初始值则再次计算点到首末两点的距离，若均大于allowRange则认为偏离航线
             * 3.如果不是初始值则判断最小值是否小于allowRange
             */
            if (minDistance == -1)
            {
                /**
                 * 计算点到首末两点的距离
                 */
                MapPoint startPoint = points[0];
                MapPoint endPoint = points[points.Count - 1];

                double startPointDistance = GetDistance(point.x, point.y, startPoint.x, startPoint.y);
                double endPointDistance = GetDistance(point.x, point.y, endPoint.x, endPoint.y);
                double distance = (startPointDistance <= endPointDistance ? startPointDistance : endPointDistance);
                minDistance = distance;
            }
            if (minDistance <= allowRange)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// 求点到直线的垂足
        /// </summary>
        /// <param name="x1">点横坐标</param>
        /// <param name="y1">点纵坐标</param>
        /// <param name="A">直线方程一般式系数A</param>
        /// <param name="B">直线方程一般式系数B</param>
        /// <param name="C">直线方程一般式系数C</param>
        /// <returns>垂足点</returns>
        private static MapPoint GetFootOfPerpendicular(double x1, double y1, double A, double B, double C)
        {
            if (A * A + B * B < 1e-13)
                return null;

            if (Math.Abs(A * x1 + B * y1 + C) < 1e-13)
            {
                return new MapPoint(x1, y1);
            }
            else
            {
                double newX = (B * B * x1 - A * B * y1 - A * C) / (A * A + B * B);
                double newY = (-A * B * x1 + A * A * y1 - B * C) / (A * A + B * B);
                return new MapPoint(newX, newY);
            }
        }

        /// <summary>
        /// 根据一个给定经纬度的点和距离，进行附近地点查询
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <param name="distance">距离（单位：公里或千米）</param>
        /// <returns>返回一个范围的4个点，最小纬度和纬度，最大经度和纬度</returns>
        public static List<MapPoint> FindNeighPosition(double longitude, double latitude, double distance)
        {
            //先计算查询点的经纬度范围  
            double r = 6378.137;//地球半径千米  
            double dis = distance;//千米距离    
            double dlng = 2 * Math.Asin(Math.Sin(dis / (2 * r)) / Math.Cos(latitude * Math.PI / 180));
            dlng = dlng * 180 / Math.PI;//角度转为弧度  
            double dlat = dis / r;
            dlat = dlat * 180 / Math.PI;
            double minlat = latitude - dlat;
            double maxlat = latitude + dlat;
            double minlng = longitude - dlng;
            double maxlng = longitude + dlng;
            return new List<MapPoint>() { new MapPoint(minlng, minlat), new MapPoint(maxlng, maxlat) };
        }

        /// <summary>
        /// 根据经纬度，计算两点间的距离
        /// </summary>
        /// <param name="longitude1">第一个点的经度</param>
        /// <param name="latitude1">第一个点的纬度</param>
        /// <param name="longitude2">第二个点的经度</param>
        /// <param name="latitude2">第二个点的纬度</param>
        /// <returns>返回距离 单位 米</returns>
        public static double GetDistance(double longitude1, double latitude1, double longitude2, double latitude2)
        {
            // 纬度
            double lat1 = ToRadians(latitude1);
            double lat2 = ToRadians(latitude2);
            // 经度
            double lng1 = ToRadians(longitude1);
            double lng2 = ToRadians(longitude2);
            // 纬度之差
            double a = lat1 - lat2;
            // 经度之差
            double b = lng1 - lng2;
            // 计算两点距离的公式
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(b / 2), 2)));
            // 弧长乘地球半径, 返回单位: 千米
            s = s * EARTH_RADIUS;
            return s;
        }

        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static double ToRadians(double degrees)
        {
            double radians = (Math.PI / 180) * degrees;
            return (radians);
        }

    }

    /// <summary>
    /// 地图点
    /// </summary>
    public class MapPoint
    {
        /// <summary>
        /// 经度
        /// </summary>
        public double x { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public double y { get; set; }
        /// <summary>
        /// 地图点
        /// </summary>
        /// <param name="alon">经度</param>
        /// <param name="alat">纬度</param>
        public MapPoint(double alon, double alat)
        {
            x = alon;
            y = alat;
        }

        public MapPoint(object alon, object alat)
        {
            x = Convert.ToDouble(alon);
            y = Convert.ToDouble(alat);
        }
    }

}
