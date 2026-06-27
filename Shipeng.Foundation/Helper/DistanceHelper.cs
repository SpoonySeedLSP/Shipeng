using Shipeng.Util.Helper;

namespace Shipeng.Util
{
    public class DistanceHelper
    {

        /// <summary>
        /// 计算距离
        /// </summary>
        /// <param name="DbLatitude"> 数据库存储纬度 </param>
        /// <param name="DbLongitude"> 数据库存储经度 </param>
        /// <param name="Latitude"> 纬度 </param>
        /// <param name="Longitude"> 经度 </param>
        /// <returns> </returns>

        public static double GetDistance(double? DbLatitude, double? DbLongitude, double? Latitude, double? Longitude)
        {
            double mrz;
            try
            {
                if (DbLatitude > 0 && DbLongitude > 0 && Latitude > 0 && Longitude > 0)
                {
                    double radLat1 = (double)DbLatitude;
                    double radLng1 = (double)DbLongitude;
                    double radLat2 = (double)Latitude;
                    double radLng2 = (double)Longitude;
                    mrz = CoordinateDistanceHelper.GetDistance(radLat1, radLng1, radLat2, radLng2);
                }
                else
                {
                    mrz = 995000000000;
                }
            }
            catch
            {
                mrz = 995000000000;
            }
            return mrz;
        }


    }
}
