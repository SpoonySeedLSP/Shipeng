using System;
using System.IO;
using System.Linq;
using Abp.Reflection.Extensions;

namespace PearAdmin.AbpTemplate.Web
{
    /// <summary>
    /// 这个类用于查找web项目的根路径
    /// 单元测试(查找视图)和实体框架核心命令行命令(查找conn string)
    /// </summary>
    public static class WebContentDirectoryFinder
    {
        public static string CalculateContentRootFolder()
        {
            var coreAssemblyDirectoryPath = Path.GetDirectoryName(typeof(AbpTemplateCoreModule).GetAssembly().Location);
            if (coreAssemblyDirectoryPath == null)
            {
                throw new Exception("找不到PearAdmin.AbpTemplate.Core程序集的位置!");
            }

            var directoryInfo = new DirectoryInfo(coreAssemblyDirectoryPath);
            while (!DirectoryContains(directoryInfo.FullName, "PearAdmin.AbpTemplate.sln"))
            {
                if (directoryInfo.Parent == null)
                {
                    throw new Exception("找不到内容根文件夹!");
                }

                directoryInfo = directoryInfo.Parent;
            }

            var webMvcFolder = Path.Combine(directoryInfo.FullName, "src", "PearAdmin.AbpTemplate.Admin");
            if (Directory.Exists(webMvcFolder))
            {
                return webMvcFolder;
            }

            throw new Exception("找不到web项目的根文件夹!");
        }

        private static bool DirectoryContains(string directory, string fileName)
        {
            return Directory.GetFiles(directory).Any(filePath => string.Equals(Path.GetFileName(filePath), fileName));
        }
    }
}
