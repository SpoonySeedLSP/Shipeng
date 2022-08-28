namespace Shipeng.Util
{
    public class EnumEitityDTO
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Describe { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string EnumName { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Text => Describe;

        /// <summary>
        /// id
        /// </summary>
        public string Value => Id.ToString();
    }

    public class EnumListEitityDTO
    {
        /// <summary>
        /// 枚举名称
        /// </summary>
        public string EnumName { get; set; }

        /// <summary>
        /// 枚举内容
        /// </summary>
        public List<EnumEitityDTO> Value { get; set; }
    }
}
