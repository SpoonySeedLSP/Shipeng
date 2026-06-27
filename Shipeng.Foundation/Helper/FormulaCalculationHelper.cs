using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Shipeng.Util.Helper
{
    /// <summary>
    /// 计算公式(不能包含中文 字母) 格式:(3+(2+3)/5)*2
    /// </summary>
    public class FormulaCalculationHelper
    {
        /// <summary>
        ///计算公式计算结果"3+(2+3)/5";
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static AjaxResult<decimal> FormulaCalculation(string exp)
        {
            AjaxResult<decimal> result = new();
            if (exp.IsNullOrEmpty())
            {
                result.Success = false;
                result.ErrorCode = 404;
                result.Msg = "请输入计算公式";
                result.Data = 0;
            }
            else
            {
                exp = exp.Trim().Replace(" ", "");
                if (StringHelper.IsEnCh(exp) || StringHelper.IsIncludeChinese(exp))
                {
                    result.Success = false;
                    result.Msg = "计算公式包含非法字符";
                    result.ErrorCode = 500;
                    result.Data = 0;
                }
                else
                {
                    try
                    {
                        result.Success = true;
                        result.ErrorCode = 0;
                        result.Msg = "";
                        result.Data = FormCompute(exp);
                    }
                    catch (Exception ex)
                    {
                        result.Success = false;
                        result.ErrorCode = 500;
                        result.Msg = "计算错误" + ex.Message;
                        result.Data = 0;
                    }
                }
            }
            return result;
        }

        private static decimal FormCompute(string exp)
        {
            DataTable dt = new();
            object result = dt.Compute(exp, "false");
            return decimal.Parse(result.ToString());
        }
    }


    /// <summary>
    ///C 代表房屋对应建筑面积
    ///F 代表房屋对应套内面积
    ///L 代表房屋层数
    ///D 代表电表、水表该月用量
    /// </summary>
    public class FormulaCalculationInputDTO
    {
        /// <summary>
        /// 建筑面积
        /// </summary>
        public decimal? BuildAreas { get; set; } = -1;

        /// <summary>
        /// 套内面积
        /// </summary>
        public decimal? InsideAreas { get; set; } = -1;

        /// <summary>
        /// 楼层
        /// </summary>
        public int? Floors { get; set; } = -1;

        public decimal? Dosage { get; set; } = -1;

        [Required]
        public string Formula { get; set; }

    }
}