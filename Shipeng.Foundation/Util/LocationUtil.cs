namespace Shipeng.Util
{
    /// <summary>
    /// 位置坐标转换类
    /// 地球上同一个地理位置的经纬度，在不同的坐标系中，会有少于偏移，国内目前常见的坐标系主要分为三种：
    ///     地球坐标系——WGS84：常见于GPS设备，Google地图等国际标准的坐标体系，是国际标准，GPS坐标（Google Earth使用、或者GPS模块）
    ///     火星坐标系——GCJ-02：中国国内使用的被强制加密后的坐标体系，高德坐标就属于该种坐标体系，中国坐标偏移标准，Google Map、高德、腾讯使用
    ///     百度坐标系——BD-09：百度地图所使用的坐标体系，是在火星坐标系的基础上又进行了一次加密处理，百度坐标偏移标准，Baidu Map使用
    /// </summary>
    public class LocationUtil
    {
        public const double Pi = 3.14159265358979324 * 3000.0 / 180.0;
        private static readonly double pi = 3.14159265358979324;
        private static readonly double a = 6378245.0;
        private static readonly double ee = 0.00669342162296594323;

        /// <summary>
        /// 国际标准->百度标准（gps坐标转换成百度坐标，小数点前4位为准确坐标）
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lon">经度</param>
        /// <returns></returns>
        public static double[] Wgs2Bd(double lat, double lon)
        {
            var wgs2Gcj = Wgs2Gcj(lat, lon);
            var gcj2Bd = Gcj2Bd(wgs2Gcj[0], wgs2Gcj[1]);
            return gcj2Bd;
        }

        /// <summary>
        /// 中国坐标偏移标准->百度标准
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lon">经度</param>
        /// <returns></returns>
        public static double[] Gcj2Bd(double lat, double lon)
        {
            double x = lon, y = lat;
            var z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * Pi);
            var theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * Pi);
            var cos = z * Math.Cos(theta) + 0.0065;
            var sin = z * Math.Sin(theta) + 0.006;
            return new[] { sin, cos };
        }

        /// <summary>
        /// 百度标准->中国坐标偏移标准
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lon">经度</param>
        /// <returns></returns>
        public static double[] Bd2Gcj(double lat, double lon)
        {
            double x = lon - 0.0065, y = lat - 0.006;
            var z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * Pi);
            var theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * Pi);
            var cos = z * Math.Cos(theta);
            var sin = z * Math.Sin(theta);
            return new[] { sin, cos };
        }

        /// <summary>
        /// 国际标准->中国坐标偏移标准
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lon">经度</param>
        /// <returns></returns>
        public static double[] Wgs2Gcj(double lat, double lon)
        {
            var dLat = TransformLat(lon - 105.0, lat - 35.0);
            var dLon = TransformLon(lon - 105.0, lat - 35.0);
            var radLat = lat / 180.0 * pi;
            var magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            var sqrtMagic = Math.Sqrt(magic);
            dLat = dLat * 180.0 / (a * (1 - ee) / (magic * sqrtMagic) * pi);
            dLon = dLon * 180.0 / (a / sqrtMagic * Math.Cos(radLat) * pi);
            var mgLat = lat + dLat;
            var mgLon = lon + dLon;
            double[] loc = { mgLat, mgLon };
            return loc;
        }

        /// <summary>
        /// 转换纬度
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lon">经度</param>
        /// <returns></returns>
        private static double TransformLat(double lat, double lon)
        {
            var ret = -100.0 + 2.0 * lat + 3.0 * lon + 0.2 * lon * lon + 0.1 * lat * lon + 0.2 * Math.Sqrt(Math.Abs(lat));
            ret += (20.0 * Math.Sin(6.0 * lat * pi) + 20.0 * Math.Sin(2.0 * lat * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(lon * pi) + 40.0 * Math.Sin(lon / 3.0 * pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(lon / 12.0 * pi) + 320 * Math.Sin(lon * pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        /// <summary>
        /// 转换经度
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lon">经度</param>
        /// <returns></returns>
        private static double TransformLon(double lat, double lon)
        {
            var ret = 300.0 + lat + 2.0 * lon + 0.1 * lat * lat + 0.1 * lat * lon + 0.1 * Math.Sqrt(Math.Abs(lat));
            ret += (20.0 * Math.Sin(6.0 * lat * pi) + 20.0 * Math.Sin(2.0 * lat * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(lat * pi) + 40.0 * Math.Sin(lat / 3.0 * pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(lat / 12.0 * pi) + 300.0 * Math.Sin(lat / 30.0 * pi)) * 2.0 / 3.0;
            return ret;
        }

        /// <summary>
        /// 百度地图坐标转腾讯、高德地图坐标
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lon">经度</param>
        /// <returns></returns>
        public static double[] TransPosition(double lat, double lon)
        {
            double x = lon - 0.0065;
            double y = lat - 0.006;
            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * Pi);
            double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * Pi);
            lon = z * Math.Cos(theta);
            lat = z * Math.Sin(theta);
            return new[] { lat, lon };
        }

        /// <summary>
        /// 将腾讯、高德地图经纬度转换为百度地图经纬度
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lon">经度</param>
        /// <returns></returns>
        public static double[] MapPointTxTurnBaiDu(double lat, double lon)
        {
            double x = lon;
            double y = lat;
            double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * pi);
            double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * pi);
            lon = z * Math.Cos(theta) + 0.0065;
            lat = z * Math.Sin(theta) + 0.006;
            return new[] { lat, lon };
        }

    }
}
