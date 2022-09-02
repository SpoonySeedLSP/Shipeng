using Abp.Authorization;
using AutoMapper;
using PearAdmin.AbpTemplate.Authorization.Permissions.Dto;
using PearAdmin.AbpTemplate.Authorization.Roles;
using PearAdmin.AbpTemplate.Authorization.Roles.Dto;
using PearAdmin.AbpTemplate.Authorization.Users;
using PearAdmin.AbpTemplate.Authorization.Users.Dto;

namespace PearAdmin.AbpTemplate.Authorization
{
    public class AuthorizationMapperProfile : Profile
    {
        public AuthorizationMapperProfile()
        {
            CreateMap<Role, RoleDto>();
            CreateMap<Permission, RolePermissionDto>();
            CreateMap<User, UserDto>();
            CreateMap<User, UserEditDto>();
            CreateMap<Permission, PermissionDto>();
        }
    }
}
