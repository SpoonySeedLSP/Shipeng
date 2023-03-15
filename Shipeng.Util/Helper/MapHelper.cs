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
        /// 地球的平均半径为6371千米,赤道半径6378千米,极半径6357千米,赤道周长约为 40091千米
        /// </summary>
        private static double EARTH_RADIUS = 6378137.0;//地球半径,单位 m
        private static double ChiDaoR = 6378.2;//赤道半径,单位 km

        /// <summary>
        /// 判断是否在误差范围内
        /// </summary>
        /// <param name="point">坐标点</param>
        /// <param name="points">边界点集合</param>
        /// <param name="limitDistance">极限距离 单位：m</param>
        /// <returns></returns>
        public static bool InLimitDistance(MapPoint point, List<MapPoint> points, double limitDistance)
        {
            List<double> distance = new List<double>();
            var len = points.Count;
            var maxIndex = len - 1;
            for (int i = 0; i < len; i++)
            {
                //多边形中当前点
                var currentPoint = points[i];
                var nearPoint = maxIndex == i ? points[0] : points[i + 1];
                double a, b, c;
                a = GetDistance(point, currentPoint);//经纬坐标系中求两点的距离公式
                b = GetDistance(point, nearPoint);//经纬坐标系中求两点的距离公式
                c = GetDistance(currentPoint, nearPoint);//经纬坐标系中求两点的距离公式
                if (b * b >= c * c + a * a)
                {
                    distance.Add(c);
                    continue;

                }
                if (c * c >= b * b + a * a)
                {
                    distance.Add(b);
                    continue;
                }

                double l = (a + b + c) / 2;//周长的一半
                double s = Math.Sqrt(l * (l - a) * (l - b) * (l - c));//海伦公式求面积
                distance.Add(2 * s / a);
            }

            if (!distance.Any())
            {
                return false;
            }

            var count = distance.Where(s => s < limitDistance).Count();
            if (count > 0) return true;
            return false;
        }

        /// <summary>
        /// 连点之间距离公式判断坐标是否在圆内,√[(x1-x2)²+(y1-y2)²]
        /// </summary>
        /// <param name="p"></param>
        /// <param name="f"></param>
        /// <param name="r">半径（单位：公里或千米）</param>
        /// <returns></returns>
        public static bool InoutCircle(MapPoint p, MapPoint f, double r)
        {
            double distanceBetPoints;//两点之间距离
            distanceBetPoints = Math.Pow(Math.Pow(p.x - f.x, 2) + Math.Pow(p.y - f.y, 2), 0.5);
            return distanceBetPoints <= r;
        }

        /// <summary>
        /// 判断点是否在多边形内.
        /// ----------原理----------
        /// 注意到如果从P作水平向左的射线的话，如果P在多边形内部，那么这条射线与多边形的交点必为奇数，
        /// 如果P在多边形外部，则交点个数必为偶数(0也在内)。
        /// 所以，我们可以顺序考虑多边形的每条边，求出交点的总个数。还有一些特殊情况要考虑。假如考虑边(P1,P2)，
        /// 1)如果射线正好穿过P1或者P2,那么这个交点会被算作2次，处理办法是如果P的从坐标与P1,P2中较小的纵坐标相同，则直接忽略这种情况
        /// 2)如果射线水平，则射线要么与其无交点，要么有无数个，这种情况也直接忽略。
        /// 3)如果射线竖直，而P0的横坐标小于P1,P2的横坐标，则必然相交。
        /// 4)再判断相交之前，先判断P是否在边(P1,P2)的上面，如果在，则直接得出结论：P再多边形内部。
        /// </summary>
        /// <param name="checkPoint">要判断的点</param>
        /// <param name="polygonPoints">多边形的顶点</param>
        /// <returns></returns>
        public static bool IsInPolygon2(MapPoint checkPoint, List<MapPoint> polygonPoints)
        {
            int counter =0;
            int i;
            double xinters;
            MapPoint p1, p2;
            int pointCount = polygonPoints.Count;
            p1 = polygonPoints[0];
            for (i =0 ; i <= pointCount; i++)
            {
                p2 = polygonPoints[i % pointCount];
                if (checkPoint.y > Math.Min(p1.y, p2.y)//校验点的Y大于线段端点的最小Y
                    && checkPoint.y <= Math.Max(p1.y, p2.y))//校验点的Y小于线段端点的最大Y
                {
                    if (checkPoint.x <= Math.Max(p1.x, p2.x))//校验点的X小于等线段端点的最大X(使用校验点的左射线判断).
                    {
                        if (p1.y != p2.y)//线段不平行于X轴
                        {
                            xinters = (checkPoint.y - p1.y) * (p2.x - p1.x) / (p2.y - p1.y) + p1.x;
                            if (p1.x == p2.x || checkPoint.x <= xinters)
                            {
                                counter++;
                            }
                        }
                    }

                }
                p1 = p2;
            }

            if (counter % 2  != 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 判断当前位置是否在不规则形状里面
        /// </summary>
        /// <param name="nvert">不规则形状的定点数</param>
        /// <param name="vertx">当前x坐标</param>
        /// <param name="verty">当前y坐标</param>
        /// <param name="testx">不规则形状x坐标集合</param>
        /// <param name="testy">不规则形状y坐标集合</param>
        /// <returns></returns>
        public static bool PositionPnpoly(int nvert, List<double> vertx, List<double> verty, double testx, double testy)
        {
            int i, j, c = 0;
            for (i =0 , j = nvert -- ; i < nvert; j = i++)
            {
                if (((verty[i] > testy) != (verty[j] > testy)) && (testx < (vertx[j] - vertx[i]) * (testy - verty[i]) / (verty[j] - verty[i]) + vertx[i]))
                {
                    c = +c; ;
                }
            }
            if (c % 2  != 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 判断点是否在多边形内.
        /// ----------原理----------
        /// 注意到如果从P作水平向左的射线的话，如果P在多边形内部，那么这条射线与多边形的交点必为奇数，
        /// 如果P在多边形外部，则交点个数必为偶数(0也在内)。
        /// </summary>
        /// <param name="checkPoint">要判断的点</param>
        /// <param name="polygonPoints">多边形的顶点</param>
        /// <returns></returns>
        public static bool IsInPolygon(MapPoint checkPoint, List<MapPoint> polygonPoints)
        {
            bool inside = false;
            int pointCount = polygonPoints.Count;
            MapPoint p1, p2;
            //第一个点和最后一个点作为第一条线，之后是第一个点和第二个点作为第二条线，之后是第二个点与第三个点，第三个点与第四个点...
            for (int i = 0, j = pointCount -- ; i < pointCount; j = i, i++)
            {
                p1 = polygonPoints[i];
                p2 = polygonPoints[j];
                if (checkPoint.y < p2.y)
                {
                    //p2在射线之上
                    if (p1.y <= checkPoint.y)
                    {//p1正好在射线中或者射线下方
                        if ((checkPoint.y - p1.y) * (p2.x - p1.x) > (checkPoint.x - p1.x) * (p2.y - p1.y))//斜率判断,在P1和P2之间且在P1P2右侧
                        {
                            //射线与多边形交点为奇数时则在多边形之内，若为偶数个交点时则在多边形之外。
                            //由于inside初始值为false，即交点数为零。所以当有第一个交点时，则必为奇数，则在内部，此时为inside=(!inside)
                            //所以当有第二个交点时，则必为偶数，则在外部，此时为inside=(!inside)
                            inside = (!inside);
                        }
                    }
                }
                else if (checkPoint.y < p1.y)
                {
                    //p2正好在射线中或者在射线下方，p1在射线上
                    if ((checkPoint.y - p1.y) * (p2.x - p1.x) < (checkPoint.x - p1.x) * (p2.y - p1.y))//斜率判断,在P1和P2之间且在P1P2右侧
                    {
                        inside = (!inside);
                    }
                }
            }
            return inside;
        }

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
        /// 以一个经纬度为中心计算出四个顶点
        /// </summary>
        /// <param name="distance">半径(米)</param>
        /// <returns></returns>
        public static MapPoint[] GetDegreeCoordinates(MapPoint point, double distance)
        {
            double dlng = 2 * Math.Asin(Math.Sin(distance / (2 * EARTH_RADIUS)) / Math.Cos(point.x));
            dlng = ToAngle(dlng);//一定转换成角度数

            double dlat = distance / EARTH_RADIUS;
            dlat = ToAngle(dlat);//一定转换成角度数

            return new MapPoint[] { new MapPoint(Math.Round(point.x + dlat,6), Math.Round(point.y - dlng,6)),//left-top
                                  new MapPoint(Math.Round(point.x - dlat,6), Math.Round(point.y - dlng,6)),//left-bottom
                                  new MapPoint(Math.Round(point.x + dlat,6), Math.Round(point.y + dlng,6)),//right-top
                                  new MapPoint(Math.Round(point.x - dlat,6), Math.Round(point.y + dlng,6)) //right-bottom
            };

        }

        #region 根据圆点经纬度和半径，给出圆周上各点经纬度
        /// <summary>
        /// 根据圆点经纬度和半径，给出圆周上各点经纬度
        /// </summary>
        /// <param name="pos">圆点经纬度</param>
        /// <param name="r">半径（单位：公里或千米）</param>
        /// <returns></returns>
        public static List<MapPoint> GetCirclePoint(MapPoint pos, double r)
        {
            List<MapPoint> CirclePt = new List<MapPoint>();
            double longitude = 0, latitude = 0;
            //从0度开始，每隔5度计算一个点，一共360/5 = 72个点
            for (double rangle = 0; rangle <= 360; rangle += 5)
            {
                ComputePosition(pos.x, pos.y, rangle, r, ref longitude, ref latitude);
                MapPoint pt = new MapPoint(longitude, latitude);
                CirclePt.Add(pt);
            }
            return CirclePt;
        }

        /// <summary>
        /// 计算位置
        /// </summary>
        /// <param name="longitude1">经度1</param>
        /// <param name="latitude1">纬度1</param>
        /// <param name="rangle"></param>
        /// <param name="r">半径（单位：公里或千米）</param>
        /// <param name="longitude2">经度2</param>
        /// <param name="latitude2">纬度2</param>
        public static void ComputePosition(double longitude1, double latitude1, double rangle, double r, ref double longitude2, ref double latitude2)
        {
            double ddu, w, tempPa, Dd;
            double g_HuDu = Math.PI / 180.0;
            //将传入的公里值转换成海里值
            r = r * 1.843;//在赤道区域,1海里大约是1.843公里,而在两极区域,1海里大约是1.862公里

            rangle = rangle * g_HuDu;
            ddu = r * Math.Cos(rangle);
            w = r * Math.Sin(rangle);

            //计算第二点纬度
            tempPa = latitude1 + ddu / 60.0;
            if (tempPa > 90)
            {
                tempPa = 90.0;
            }
            else if (tempPa < -90)
            {
                tempPa = -90.0;
            }
            latitude2 = tempPa;

            //计算第二点经度
            Dd = G_JCWD(60 * tempPa) - G_JCWD(60 * latitude1);//计算渐长纬度差
            if (Math.Abs(ddu) <= 0.00001)
            {
                tempPa = longitude1 + (w / Math.Cos(latitude1 * g_HuDu)) / 60.0;
            }
            else
            {
                tempPa = (longitude1 + (w * Dd / ddu) / 60.0);
            }
            if (tempPa > 180)
            {
                tempPa = tempPa - 360.0;
            }
            else if (tempPa < -180.0)
            {
                tempPa = tempPa + 360.0;
            }
            longitude2 = tempPa;
        }

        private static double G_JCWD(double fi)
        {
            double JCWD;
            JCWD = ChiDaoR * Math.Log10(Math.Tan(Math.PI / 4 + fi / (2 * 60) * Math.PI / 180));
            return JCWD;
        }

        #endregion

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
        /// 计算两个经纬度之间的直接距离
        /// 该公式为GOOGLE提供，误差小于0.2米
        /// </summary>
        /// <param name="point1">第一个经纬度</param>
        /// <param name="point2">第二个经纬度</param>
        /// <returns></returns>
        public static double GetDistance(MapPoint point1, MapPoint point2)
        {
            double radLat1 = ToRadians(point1.x);
            double radLat2 = ToRadians(point2.x);
            double a = radLat1 - radLat2;
            double b = ToRadians(point1.y) - ToRadians(point2.y);

            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }

        /// <summary>
        /// 计算两个经纬度之间的直接距离(google 算法)
        /// </summary>
        /// <param name="point1">第一个经纬度</param>
        /// <param name="point2">第二个经纬度</param>
        /// <returns></returns>
        public static double GetDistanceGoogle(MapPoint point1, MapPoint point2)
        {
            double radLat1 = ToRadians(point1.x);
            double radLng1 = ToRadians(point1.y);
            double radLat2 = ToRadians(point2.x);
            double radLng2 = ToRadians(point2.y);

            double s = Math.Acos(Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Cos(radLng1 - radLng2) + Math.Sin(radLat1) * Math.Sin(radLat2));
            s = s * EARTH_RADIUS;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }


        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="degrees">角度</param>
        /// <returns></returns>
        public static double ToRadians(double degrees)
        {
            double radians = (Math.PI / 180) * degrees;
            return (radians);
        }

        /// <summary>
        /// 弧度转换为角度
        /// </summary>
        /// <param name="degrees">弧度</param>
        public static double ToAngle(double degrees)
        {
            return degrees * (180 / Math.PI);
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
