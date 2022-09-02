using Abp;
using Abp.Application.Features;
using Abp.Authorization;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Localization;
using Abp.Runtime.Session;
using PearAdmin.AbpTemplate.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PearAdmin.AbpTemplate.Admin.Filter
{
    /// <summary>
    /// 权限验证---IAuthorizationHelper的具体实现
    /// 当如果用户请求的方法或者控制器是标注了授权特性的话，都会通过 IAuthorizationHelper 进行验证，它一共有两个公开方法
    /// 在其默认的实现当中，注入了两个相对重要的组件，
    /// 第一个是 IAbpSession，它是 Abp 框架定义的用户会话状态，如果当前用户处于登录状态的时候，
    /// 其内部必定有值，在这里主要用于判断用户是否登录。
    /// 第二个则是 IPermissionChecker ，它则是用于具体的检测逻辑，
    /// 如果说 IAuthorizationHelper 是用来提供权限验证的工具，
    /// 那么 IPermissionChecker 就是权限验证的核心，
    /// 在 IPermissionChecker 内部则是真正的对传入的权限进行了验证逻辑
    /// </summary>
    public class AuthorizationHelper : IAuthorizationHelper, ITransientDependency
    {
        public IAbpSession AbpSession { get; set; }
        public IPermissionChecker PermissionChecker { get; set; }
        public IFeatureChecker FeatureChecker { get; set; }
        public ILocalizationManager LocalizationManager { get; set; }

        private readonly IFeatureChecker _featureChecker;
        private readonly IAuthorizationConfiguration _authConfiguration;

        public AuthorizationHelper(IFeatureChecker featureChecker, IAuthorizationConfiguration authConfiguration)
        {
            _featureChecker = featureChecker;
            _authConfiguration = authConfiguration;
            AbpSession = NullAbpSession.Instance;
            PermissionChecker = NullPermissionChecker.Instance;
            LocalizationManager = NullLocalizationManager.Instance;
        }

        public virtual async Task AuthorizeAsync(IEnumerable<IAbpAuthorizeAttribute> authorizeAttributes)
        {
            // 判断是否启用了授权系统，没有启用则直接跳过不做验证
            if (!_authConfiguration.IsEnabled)
            {
                return;
            }

            // 如果当前的用户会话状态其 SessionId 没有值，则说明用户没有登录，抛出授权验证失败异常
            if (!AbpSession.UserId.HasValue)
            {
                throw new AbpAuthorizationException(
                    LocalizationManager.GetString(AbpConsts.LocalizationSourceName, "CurrentUserDidNotLoginToTheApplication")
                    );
            }

            // 遍历所有授权特性，通过 IPermissionChecker 来验证用户是否拥有这些特性所标注的权限
            foreach (var authorizeAttribute in authorizeAttributes)
            {
                await PermissionChecker.AuthorizeAsync(authorizeAttribute.RequireAllPermissions, authorizeAttribute.Permissions);
            }
        }

        public virtual void Authorize(IEnumerable<IAbpAuthorizeAttribute> authorizeAttributes)
        {
            if (!_authConfiguration.IsEnabled)
            {
                return;
            }

            if (!AbpSession.UserId.HasValue)
            {
                throw new AbpAuthorizationException(
                    LocalizationManager.GetString(AbpConsts.LocalizationSourceName, "CurrentUserDidNotLoginToTheApplication")
                    );
            }

            foreach (var authorizeAttribute in authorizeAttributes)
            {
                PermissionChecker.Authorize(authorizeAttribute.RequireAllPermissions, authorizeAttribute.Permissions);
            }
        }


        // 授权过滤器与授权拦截器调用的方法，传入一个方法定义与方法所在的类的类型
        public virtual async Task AuthorizeAsync(MethodInfo methodInfo, Type type)
        {
            // 检测产品功能
            await CheckFeaturesAsync(methodInfo, type);
            // 检测权限
            await CheckPermissionsAsync(methodInfo, type);
        }

        public virtual void Authorize(MethodInfo methodInfo, Type type)
        {
            CheckFeatures(methodInfo, type);
            CheckPermissions(methodInfo, type);
        }

        protected virtual async Task CheckFeaturesAsync(MethodInfo methodInfo, Type type)
        {
            var featureAttributes =  ReflectionHelper.GetAttributesOfMemberAndType<RequiresFeatureAttribute>(methodInfo, type);

            if (featureAttributes.Count <= 0)
            {
                return;
            }

            foreach (var featureAttribute in featureAttributes)
            {
                // 检查当前用户是否启用了被调用方法标注上面的功能
                await _featureChecker.CheckEnabledAsync(featureAttribute.RequiresAll, featureAttribute.Features);
            }
        }

        protected virtual void CheckFeatures(MethodInfo methodInfo, Type type)
        {
            var featureAttributes = ReflectionHelper.GetAttributesOfMemberAndType<RequiresFeatureAttribute>(methodInfo, type);

            if (featureAttributes.Count <= 0)
            {
                return;
            }

            foreach (var featureAttribute in featureAttributes)
            {
                _featureChecker.CheckEnabled(featureAttribute.RequiresAll, featureAttribute.Features);
            }
        }

        protected virtual async Task CheckPermissionsAsync(MethodInfo methodInfo, Type type)
        {
            // 判断是否启用了授权系统，没有启用则直接跳过不做验证
            if (!_authConfiguration.IsEnabled)
            {
                return;
            }

            // 判断方法或者控制器类上是否标注了匿名访问特性，如果标注了，不做权限验证
            if (AllowAnonymous(methodInfo, type))
            {
                return;
            }

            if (ReflectionHelper.IsPropertyGetterSetterMethod(methodInfo, type))
            {
                return;
            }

            if (!methodInfo.IsPublic && !methodInfo.GetCustomAttributes().OfType<IAbpAuthorizeAttribute>().Any())
            {
                return;
            }

            // 获得方法和类上面定义的所有权限特性数组
            var authorizeAttributes =
                ReflectionHelper
                    .GetAttributesOfMemberAndType(methodInfo, type)
                    .OfType<IAbpAuthorizeAttribute>()
                    .ToArray();

            // 如果一个都不存在，跳过验证
            if (!authorizeAttributes.Any())
            {
                return;
            }

            // 传入所有权限特性，调用另外一个重载方法，使用 IPermissionChecker 针对这些特性进行具体验证
            await AuthorizeAsync(authorizeAttributes);
        }

        protected virtual void CheckPermissions(MethodInfo methodInfo, Type type)
        {
            if (!_authConfiguration.IsEnabled)
            {
                return;
            }

            if (AllowAnonymous(methodInfo, type))
            {
                return;
            }

            if (ReflectionHelper.IsPropertyGetterSetterMethod(methodInfo, type))
            {
                return;
            }

            if (!methodInfo.IsPublic && !methodInfo.GetCustomAttributes().OfType<IAbpAuthorizeAttribute>().Any())
            {
                return;
            }

            var authorizeAttributes =
                ReflectionHelper
                    .GetAttributesOfMemberAndType(methodInfo, type)
                    .OfType<IAbpAuthorizeAttribute>()
                    .ToArray();

            if (!authorizeAttributes.Any())
            {
                return;
            }

            Authorize(authorizeAttributes);
        }

        private static bool AllowAnonymous(MemberInfo memberInfo, Type type)
        {
            return ReflectionHelper
                .GetAttributesOfMemberAndType(memberInfo, type)
                .OfType<IAbpAllowAnonymousAttribute>()
                .Any();
        }
    
    }
}
