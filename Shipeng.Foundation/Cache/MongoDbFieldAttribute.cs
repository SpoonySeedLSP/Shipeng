namespace Shipeng.Util.Cache
{
    /// <summary>  
    /// Mongodb数据库的字段特性  主要是设置索引之用  
    /// Author:李仕鹏
    /// </summary>  
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MongoDbFieldAttribute : Attribute
    {
        /// <summary>  
        /// 是否是索引  
        /// </summary>  
        public bool IsIndex { get; set; }

        /// <summary>  
        /// 是否是唯一的  默认flase  
        /// </summary>  
        public bool Unique { get; set; }

        /// <summary>  
        /// 是否是升序 默认true  
        /// </summary>  
        public bool Ascending { get; set; }

        public MongoDbFieldAttribute(bool _isIndex)
        {
            IsIndex = _isIndex;
            Unique = false;
            Ascending = true;
        }
    }
}
