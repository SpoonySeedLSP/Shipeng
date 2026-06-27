using Shipeng.Util;

namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// 拓展类
    /// </summary>
    public static partial class Extention
    {
        public static string MapPath(this HttpContext httpContext, string virtualPath)
        {
            return $"{Path.Combine(new List<string> { GlobalData.WebRootPath }.Concat(GlobalData.WebRootPathWwwroot.Split('/')).ToArray())}";
        }
    }
}