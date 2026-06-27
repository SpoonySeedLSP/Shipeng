namespace Shipeng.Util.Helper
{
    /// <summary>
    /// 腾讯地图地址转换（根据详细地址转换经纬度）
    /// </summary>
    public class WebServiceAPIHelper
    {
        private static readonly string Key = "VDCBZ-IMNKP-A3KDC-LIN3U-RTR42-ORBDT";
        //https://apis.map.qq.com/ws/geocoder/v1/?address=

        /// <summary>
        /// 腾讯地图地址转换
        /// </summary>
        /// <param name="province"> </param>
        /// <param name="city"> </param>
        /// <param name="district"> </param>
        /// <param name="address"> </param>
        /// <returns> </returns>
        public static async Task<ServiceResultLocation> GetServiceGeoCoderDataAsync(string province, string city, string district, string address)
        {
            return await Task.Run(() =>
            {
                return GetServiceGeoCoderData(province, city, district, address);
            });
        }

        /// <summary>
        /// 腾讯地图地址转换
        /// </summary>
        /// <param name="province"> </param>
        /// <param name="city"> </param>
        /// <param name="district"> </param>
        /// <param name="address"> </param>
        /// <returns> </returns>
        public static ServiceResultLocation GetServiceGeoCoderData(string province, string city, string district, string address)
        {
            address = address.Replace(province, "");
            address = address.Replace(city, "");
            address = address.Replace(district, "");
            string getaddress = province + city + district + address;
            Dictionary<string, object> paramters = new Dictionary<string, object> { { "address", getaddress }, { "key", Key } };
            string url = "https://apis.map.qq.com/ws/geocoder/v1";
            string result = HttpHelper.GetData(url, paramters);
            ServiceData data = result.ToObject<ServiceData>();
            if (data.status == 0)
            {
                return data.result.location;
            }
            return null;
        }
    }

    public class ServiceData
    {
        /// <summary>
        /// 状态
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// 状态说明
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 返回值
        /// </summary>
        public ServiceResultData result { get; set; }
    }

    public class ServiceResultData
    {
        /// <summary>
        /// 解析到的坐标
        /// </summary>
        public ServiceResultLocation location { get; set; }

        /// <summary>
        /// 解析后的地址部件
        /// </summary>
        public ServiceResultAddress_components address_components { get; set; }

        /// <summary>
        /// 行政区划信息
        /// </summary>
        public ServiceResultAd_info ad_info { get; set; }

        /// <summary>
        /// 可信度参考
        /// </summary>
        public double reliability { get; set; }

        /// <summary>
        /// 解析精度级别
        /// </summary>
        public int level { get; set; }
    }

    public class ServiceResultLocation
    {
        /// <summary>
        /// 纬度
        /// </summary>
        public double lat { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        public double lng { get; set; }
    }

    public class ServiceResultAddress_components
    {
        /// <summary>
        /// 省
        /// </summary>
        public string province { get; set; }

        /// <summary>
        /// 市
        /// </summary>
        public string city { get; set; }

        /// <summary>
        /// 区
        /// </summary>
        public string district { get; set; }

        /// <summary>
        /// 街道
        /// </summary>
        public string street { get; set; }

        /// <summary>
        /// 门牌
        /// </summary>
        public string street_number { get; set; }
    }

    public class ServiceResultAd_info
    {
        /// <summary>
        /// 行政区划代码
        /// </summary>
        public string adcode { get; set; }
    }
}
