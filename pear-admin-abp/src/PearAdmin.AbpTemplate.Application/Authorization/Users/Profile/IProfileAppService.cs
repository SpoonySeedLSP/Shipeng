using Abp.Application.Services.Dto;
using PearAdmin.AbpTemplate.Authorization.Users.Profile.Dto;
using System.Threading.Tasks;

namespace PearAdmin.AbpTemplate.Authorization.Users.Profile
{
    /// <summary>
    /// 文件应用服务
    /// </summary>
    public interface IProfileAppService
    {
        Task ChangePhoneNumber(ChangePhoneNumberDto input);

        Task ChangePassword(ChangePasswordDto input);

        Task<GetProfilePictureOutput> GetProfilePicture();

        Task<GetProfilePictureOutput> GetProfilePictureByUser(EntityDto<long> input);

        Task UpdateProfilePicture(UpdateProfilePictureInput input);
    }
}
