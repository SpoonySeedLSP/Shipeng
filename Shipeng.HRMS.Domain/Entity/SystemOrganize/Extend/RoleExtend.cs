using SqlSugar;

namespace Shipeng.HRMS.Domain.SystemOrganize
{
    /// <summary>
    /// 创 建：李仕鹏
    /// 日 期：2022-08-27 23:36
    /// 描 述：角色类别实体扩展类
    /// </summary>
    [SugarTable("sys_role")]
    public class RoleExtend : RoleEntity
    {
        //使用导入错误信息
        public string ErrorMsg { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        /// <returns></returns>
        public string F_Remark { get; set; }
        public string F_CompanyName { get; set; }
    }
}
