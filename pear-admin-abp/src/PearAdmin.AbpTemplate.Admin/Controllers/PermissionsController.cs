using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using PearAdmin.AbpTemplate.Authorization.Permissions;
using PearAdmin.AbpTemplate.Authorization.Permissions.Dto;
using PearAdmin.AbpTemplate.Admin.Models.Common;
using PearAdmin.AbpTemplate.Admin.Models.Permissions;
using PearAdmin.AbpTemplate.Authorization.Roles.Dto;
using System.Linq;
using System.Collections.Generic;

namespace PearAdmin.AbpTemplate.Admin.Controllers
{
    /// <summary>
    /// 权限管理控制器
    /// </summary>
    [AbpMvcAuthorize]
    public class PermissionsController : AbpTemplateControllerBase
    {
        #region 初始化
        private readonly IPermissionAppService _permissionAppService;

        public PermissionsController(IPermissionAppService permissionAppService)
        {
            _permissionAppService = permissionAppService;
        }
        #endregion

        #region 权限管理
        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        public JsonResult GetPermissions()
        {
            /* 得到所有权限
             * tenancyFilter:可以传递false来禁用租户过滤器
             */
            var permissions = PermissionManager.GetAllPermissions();
            List<RolePermissionDto> permissionData = permissions
                .Select(p => new RolePermissionDto()
                {
                    Id = p.Name,
                    ParentId = p.Parent == null ? string.Empty : p.Parent.Name,
                    Name = p.Name,
                    Description = p.Description?.ToString(),
                    DisplayName = p.Name,
                })
                .OrderBy(p => p.DisplayName)
                .ToList();
            return Json(new ResponseParamPagedViewModel<RolePermissionDto>(permissionData.Count, permissionData));
        }

        /// <summary>
        /// 权限列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetPermissionList(GetPagedPermissionViewModel viewModel)
        {
            var input = PagedViewModelMapToPagedInputDto<GetPagedPermissionViewModel, GetPagedPermissionInput>(viewModel);

            var pagedPermissionList = _permissionAppService.GetPagedPermission(input);

            return Json(new ResponseParamPagedViewModel<PermissionDto>(pagedPermissionList.TotalCount, pagedPermissionList.Items));
        }
        #endregion
    }
}