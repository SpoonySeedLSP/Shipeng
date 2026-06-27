using System;

namespace SqlSugar
{
    /// <summary>
    /// 然后需要根据具体业务和查询创建一个返回的实体，用来接收查询结果，和返回给前端
    /// 这样，利用反射就可以获取到查询类的属性和类型，进而用来判断sql语句该怎么拼
    /// 但是这样做太过简单，很多业务无法涵盖到，不符合实际需求，
    /// 比如，string类型需要比大小，比如大于、小于，比如区间等查询条件都无法识别，所以还需要进行改进
    /// 我的做法是使用特性进行区分，在查询类中将作为查询条件的字段给上特性
    /// 例如：
    ///  [Conditions]
    ///  public string BillNO { get; set; }
    ///  [Conditions(ConditionsType.DATETIME, SymbolAttribute.INTERVAL, true, ",")]
    ///  public string CreationTime { get; set; }
    ///  这里因为我的时间查询字段前端传递的格式是"yyyy-MM-dd,yyyy-MM-dd"，
    ///  所以我给CreationTime的特性附上DATETIME类型，区间查询，有分隔符，分隔符为","
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public class ConditionsAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public ConditionsAttribute()
        {
            Enable = true;
            NotSelect = false;
            ConditionsTypes = ConditionsType.STRING;
            IsSplit = false;
            SymbolAttributes = SymbolAttribute.EQUAL;
        }

        /// <summary>
        /// 条件属性
        /// </summary>
        /// <param name="notSelect">是否永不查询，优先级最高</param>
        public ConditionsAttribute(bool notSelect) : this(true, notSelect, ConditionsType.STRING, SymbolAttribute.EQUAL, false, "")
        { }

        /// <summary>
        /// 条件属性
        /// </summary>
        /// <param name="notSelect">是否永不查询，优先级最高</param>
        /// <param name="enable">是否启用，如果不启用特性，则默认使用字段的类型,优先级低于NotSelect</param>
        public ConditionsAttribute(bool notSelect, bool enable) : this(enable, notSelect, ConditionsType.STRING, SymbolAttribute.EQUAL, false, "")
        { }

        /// <summary>
        /// 条件属性
        /// </summary>
        /// <param name="conditionsTypes">字段类型</param>
        public ConditionsAttribute(ConditionsType conditionsTypes) : this(true, false, conditionsTypes, SymbolAttribute.EQUAL, false, "")
        { }

        /// <summary>
        /// 条件属性
        /// </summary>
        /// <param name="conditionsTypes">字段类型</param>
        public ConditionsAttribute(ConditionsType conditionsTypes, SymbolAttribute symbolAttribute, bool isSplit, string splitString) : this(true, false, conditionsTypes, symbolAttribute, isSplit, splitString)
        { }

        /// <summary>
        /// 条件属性
        /// </summary>
        /// <param name="symbolAttribute">字符串类型,只在为string时生效</param>
        /// <param name="isSplit">是否字符分割，只在string或datetime类型下生效</param>
        /// <param name="splitString">指定分割字符串，只在IsSplit为true时生效</param>
        public ConditionsAttribute(SymbolAttribute symbolAttribute, bool isSplit, string splitString) : this(true, false, ConditionsType.STRING, symbolAttribute, isSplit, splitString)
        {
        }

        /// <summary>
        /// 条件属性
        /// </summary>
        /// <param name="enable">是否启用，如果不启用特性，则默认使用字段的类型,优先级低于NotSelect</param>
        /// <param name="notSelect">是否永不查询，优先级最高</param>
        /// <param name="conditionsTypes">字段类型</param>
        /// <param name="symbolAttributes">字符串类型,只在为string时生效</param>
        /// <param name="isSplit">是否字符分割，只在string或datetime类型下生效</param>
        /// <param name="splitString">指定分割字符串，只在IsSplit为true时生效</param>
        public ConditionsAttribute(bool enable, bool notSelect, ConditionsType conditionsTypes, SymbolAttribute symbolAttributes, bool isSplit, string splitString = "")
        {
            Enable = enable;
            NotSelect = notSelect;
            ConditionsTypes = conditionsTypes;
            SymbolAttributes = symbolAttributes;
            IsSplit = isSplit;
            SplitString = splitString;
        }

        /// <summary>
        /// 是否启用，如果不启用特性，则默认使用字段的类型,优先级低于NotSelect
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// 是否永不查询，优先级最高
        /// </summary>
        public bool NotSelect { get; set; }
        /// <summary>
        /// 字段类型
        /// </summary>
        public ConditionsType ConditionsTypes { get; set; }
        /// <summary>
        /// 字符串运算符
        /// </summary>
        public SymbolAttribute SymbolAttributes { get; set; }
        /// <summary>
        /// 是否字符分割，只在string或datetime类型下生效
        /// </summary>
        public bool IsSplit { get; set; }
        /// <summary>
        /// 指定分割字符串，只在IsSplit为true时生效
        /// </summary>
        public string SplitString { get; set; }
    }

    /// <summary>
    /// 条件类型
    /// </summary>
    public enum ConditionsType
    {
        /// <summary>
        /// 字符
        /// </summary>
        INT,
        /// <summary>
        /// 字符串
        /// </summary>
        STRING,
        /// <summary>
        /// 时间
        /// </summary>
        DATETIME,
    }

    /// <summary>
    /// 符号属性
    /// </summary>
    public enum SymbolAttribute
    {
        /// <summary>
        /// 等于
        /// </summary>
        EQUAL,
        /// <summary>
        /// 包含
        /// </summary>
        CONTAILS,
        /// <summary>
        /// 从左包含
        /// </summary>
        STARTSWITH,
        /// <summary>
        /// 从右包含
        /// </summary>
        ENDSWITH,
        /// <summary>
        /// 大于
        /// </summary>
        GREATER,
        /// <summary>
        /// 小于
        /// </summary>
        LESS,
        /// <summary>
        /// 大于等于
        /// </summary>
        GREATEREQUAL,
        /// <summary>
        /// 小于等于
        /// </summary>
        LESSEQUAL,
        /// <summary>
        /// 区间
        /// </summary>
        INTERVAL

    }
}
