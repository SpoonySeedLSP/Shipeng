using Shipeng.Util;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;

namespace Microsoft.AspNetCore.Mvc
{
    public static partial class Extention
    {
        /// <summary>
        /// 获取最新的s文件或css文件 注：解决缓存问题，只有文件修改后才会获取最新版
        /// </summary>
        /// <param name="helper"> </param>
        /// <param name="scriptVirtualPath"> </param>
        /// <returns> </returns>
        public static string Scrpit(this IUrlHelper helper, string scriptVirtualPath)
        {
            string filePath = helper.ActionContext.HttpContext.MapPath(scriptVirtualPath);
            FileInfo fileInfo = new FileInfo(filePath);
            int lastTime = fileInfo.LastWriteTime.GetHashCode();
            string url = $"{scriptVirtualPath}?_v={lastTime}";
            if (!GlobalData.IsDevelopment)
            {
                string u = GlobalData.WebRootUrl;
                url = $"{scriptVirtualPath.Replace("~", u)}?_v={lastTime}";
            }
            return helper.Content(url);
        }

        /// <summary>
        /// 获取最新的s文件或css文件 注：解决缓存问题，只有文件修改后才会获取最新版
        /// </summary>
        /// <param name="helper"> </param>
        /// <param name="scriptVirtualPath"> </param>
        /// <returns> </returns>
        public static string ScrpitComplete(this IUrlHelper helper, string scriptVirtualPath)
        {
            string filePath = helper.ActionContext.HttpContext.MapPath(scriptVirtualPath);
            FileInfo fileInfo = new FileInfo(filePath);
            int lastTime = fileInfo.LastWriteTime.GetHashCode();
            string url = helper.ActionContext.HttpContext.Request.GetAbsoluteUri();
            return helper.Content($"{scriptVirtualPath.Replace("~", url)}?_v={lastTime}");
        }

        public static string GetAbsoluteUri(this HttpRequest request)
        {
            return new StringBuilder()
                .Append(request.Scheme)
                .Append("://")
                .Append(request.Host)
                //.Append(request.PathBase)
                //.Append(request.Path)
                //.Append(request.QueryString)
                .ToString().Replace("p://", "ps://");
        }
    }
}