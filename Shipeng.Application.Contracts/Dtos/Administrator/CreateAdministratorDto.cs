using System.ComponentModel.DataAnnotations;

namespace Shipeng.Application.Contracts.Dtos.Administrator
{
    public class CreateAdministratorDto
    {
        /// <summary>
        /// 管理员名
        /// </summary>
        [Required]
        public string AdminName { get; set; }
        /// <summary>
        /// 登录密码
        /// </summary>
        [Required]
        public string Password { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        [Required]
        public string NickName { get; set; }
    }
}
