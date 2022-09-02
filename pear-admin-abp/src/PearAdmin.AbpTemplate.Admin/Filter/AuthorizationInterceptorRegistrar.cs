using Abp.Application.Features;
using Abp.Authorization;
using Abp.Dependency;
using Castle.Core;
using Castle.MicroKernel;
using System;
using System.Linq;
using System.Reflection;

namespace PearAdmin.AbpTemplate.Admin.Filter
{
    /// <summary>
    /// 权限拦截器初始化绑定----这个类用于在应用层注册拦截器
    /// 权限拦截器在Abp框架初始化完成的时候就开始监听了组件注册事件，
    /// 只要被注入的类型实现了AbpAuthorizeAttribute特性与RequiresFeatureAttribute特性
    /// 都会被注入AuthorizationInterceptor拦截器
    /// </summary>
    internal static class AuthorizationInterceptorRegistrar
    {
        public static void Initialize(IIocManager iocManager)
        {
            // 监听 DI 组件注册事件
            iocManager.IocContainer.Kernel.ComponentRegistered += Kernel_ComponentRegistered;
        }

        private static void Kernel_ComponentRegistered(string key, IHandler handler)
        {
            // 判断注入的类型是否符合要求
            if (ShouldIntercept(handler.ComponentModel.Implementation))
            {
                // 符合要求，针对该组件添加权限拦截器
                handler.ComponentModel.Interceptors.Add(new InterceptorReference(typeof(AuthorizationInterceptor)));
            }
        }

        private static bool ShouldIntercept(Type type)
        {
            if (SelfOrMethodsDefinesAttribute<AbpAuthorizeAttribute>(type))
            {
                return true;
            }

            if (SelfOrMethodsDefinesAttribute<RequiresFeatureAttribute>(type))
            {
                return true;
            }

            return false;
        }

        private static bool SelfOrMethodsDefinesAttribute<TAttr>(Type type)
        {
            // 判断传入的 Type 有定义 TAttr 类型的特性
            if (type.GetTypeInfo().IsDefined(typeof(TAttr), true))
            {
                return true;
            }

            // 或者说，该类型的所有公开的方法是否有方法标注了 TAttr 类型的特性
            return type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(m => m.IsDefined(typeof(TAttr), true));
        }
    }

}
