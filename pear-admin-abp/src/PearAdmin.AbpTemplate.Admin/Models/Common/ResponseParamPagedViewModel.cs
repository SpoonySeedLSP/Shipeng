using System.Collections.Generic;

namespace PearAdmin.AbpTemplate.Admin.Models.Common
{
    /// <summary>
    /// 分页查询结果及总数
    /// </summary>
    public class ResponseParamPagedViewModel<T> : ResponseParamViewModel
    {
        public ResponseParamPagedViewModel(int count, IReadOnlyList<T> data, string msg = "", int code = 200, bool success = true)
            : base(msg, code, success)
        {
            Count = count;
            Data = data;
        }

        /// <summary>
        /// 总条数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 数据集
        /// </summary>
        public IReadOnlyList<T> Data { get; set; }
    }

    /// <summary>
    /// 查询结果视图模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResponseParamViewModel<T> : ResponseParamViewModel
    {
        public ResponseParamViewModel(T data, string msg = "", int code = 200, bool success = true)
           : base(msg, code, success)
        {
            Data = data;
        }

        /// <summary>
        /// 数据集
        /// </summary>
        public T Data { get; set; }
    }



}
