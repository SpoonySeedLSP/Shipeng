using PearAdmin.AbpTemplate.Shared;

namespace PearAdmin.AbpTemplate.Resource.DataDictionaries
{
    /// <summary>
    /// 数据字典类型
    /// </summary>
    public class DataDictionaryType : Enumeration
    {
        public static DataDictionaryType FileManage_Dictionary = new DataDictionaryType(1, "文件/档案");
        public static DataDictionaryType Certificate_Dictionary = new DataDictionaryType(2, "证书");
        public static DataDictionaryType Education_Dictionary = new DataDictionaryType(3, "学历");

        public DataDictionaryType(int id, string name)
            : base(id, name)
        {

        }
    }
}
