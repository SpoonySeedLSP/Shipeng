using Shipeng;
using Shipeng.Dependency;
using Shipeng.DynamicApiController;
using Shipeng.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// ����ע����չ��
    /// </summary>
    [SuppressSniffer]
    public static class DependencyInjectionServiceCollectionExtensions
    {
        /// <summary>
        /// �������ע��ӿ�
        /// </summary>
        /// <param name="services">���񼯺�</param>
        /// <returns>���񼯺�</returns>
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            // ����ⲿ��������
            services.AddConfigurableOptions<DependencyInjectionSettingsOptions>();

            services.AddInnerDependencyInjection();
            return services;
        }

        /// <summary>
        /// ��ӽӿڴ���
        /// </summary>
        /// <typeparam name="TDispatchProxy">������</typeparam>
        /// <typeparam name="TIDispatchProxy">������ӿ�����</typeparam>
        /// <param name="services">���񼯺�</param>
        /// <returns>���񼯺�</returns>
        public static IServiceCollection AddScopedDispatchProxyForInterface<TDispatchProxy, TIDispatchProxy>(this IServiceCollection services)
            where TDispatchProxy : AspectDispatchProxy, IDispatchProxy
            where TIDispatchProxy : class
        {
            // ע�������
            services.AddScoped<AspectDispatchProxy, TDispatchProxy>();

            // ���������ӿ�����
            var proxyType = typeof(TDispatchProxy);
            var typeDependency = typeof(TIDispatchProxy);

            // ��ȡ���еĴ���ӿ�����
            var dispatchProxyInterfaceTypes = App.EffectiveTypes
                .Where(u => typeDependency.IsAssignableFrom(u) && u.IsInterface && u != typeDependency);

            // ע���������
            foreach (var interfaceType in dispatchProxyInterfaceTypes)
            {
                AddDispatchProxy(services, typeof(IScoped), default, proxyType, interfaceType, false);
            }

            return services;
        }

        /// <summary>
        /// ���ɨ��ע��
        /// </summary>
        /// <param name="services">���񼯺�</param>
        /// <returns>���񼯺�</returns>
        private static IServiceCollection AddInnerDependencyInjection(this IServiceCollection services)
        {
            // ����������Ҫ����ע�������
            var injectTypes = App.EffectiveTypes
                .Where(u => typeof(IPrivateDependency).IsAssignableFrom(u) && u.IsClass && !u.IsInterface && !u.IsAbstract)
                .OrderBy(u => GetOrder(u));

            var projectAssemblies = App.Assemblies;
            var lifetimeInterfaces = new[] { typeof(ITransient), typeof(IScoped), typeof(ISingleton) };

            // ִ������ע��
            foreach (var type in injectTypes)
            {
                // ��ȡע�᷽ʽ
                var injectionAttribute = !type.IsDefined(typeof(InjectionAttribute)) ? new InjectionAttribute() : type.GetCustomAttribute<InjectionAttribute>();

                var interfaces = type.GetInterfaces();

                // ��ȡ������ע��Ľӿ�
                var canInjectInterfaces = interfaces.Where(u => !injectionAttribute.ExpectInterfaces.Contains(u)
                                && u != typeof(IPrivateDependency)
                                && u != typeof(IDynamicApiController)
                                && !lifetimeInterfaces.Contains(u)
                                && projectAssemblies.Contains(u.Assembly)
                                && (
                                    (!type.IsGenericType && !u.IsGenericType)
                                    || (type.IsGenericType && u.IsGenericType && type.GetGenericArguments().Length == u.GetGenericArguments().Length))
                                );

                // ��ȡ������������
                var dependencyType = interfaces.Last(u => lifetimeInterfaces.Contains(u));

                // ע�����
                RegisterService(services, dependencyType, type, injectionAttribute, canInjectInterfaces);

                // ��������ע��
                var typeNamed = injectionAttribute.Named ?? type.Name;
                TypeNamedCollection.TryAdd(typeNamed, type);
            }

            // ע���ⲿ���÷���
            RegisterExternalServices(services);

            // ע���������񣨽ӿڶ�ʵ�֣�
            RegisterNamedService<ITransient>(services);
            RegisterNamedService<IScoped>(services);
            RegisterNamedService<ISingleton>(services);

            return services;
        }

        /// <summary>
        /// ע�����
        /// </summary>
        /// <param name="services">���񼯺�</param>
        /// <param name="dependencyType"></param>
        /// <param name="type">����</param>
        /// <param name="injectionAttribute">ע������</param>
        /// <param name="canInjectInterfaces">�ܱ�ע��Ľӿ�</param>
        private static void RegisterService(IServiceCollection services, Type dependencyType, Type type, InjectionAttribute injectionAttribute, IEnumerable<Type> canInjectInterfaces)
        {
            // ע���Լ�
            if (injectionAttribute.Pattern is InjectionPatterns.Self or InjectionPatterns.All or InjectionPatterns.SelfWithFirstInterface)
            {
                Register(services, dependencyType, type, injectionAttribute);
            }

            if (!canInjectInterfaces.Any()) return;

            // ֻע���һ���ӿ�
            if (injectionAttribute.Pattern is InjectionPatterns.FirstInterface or InjectionPatterns.SelfWithFirstInterface)
            {
                Register(services, dependencyType, type, injectionAttribute, canInjectInterfaces.Last());
            }
            // ע�����ӿ�
            else if (injectionAttribute.Pattern is InjectionPatterns.ImplementedInterfaces or InjectionPatterns.All)
            {
                foreach (var inter in canInjectInterfaces)
                {
                    Register(services, dependencyType, type, injectionAttribute, inter);
                }
            }
        }

        /// <summary>
        /// ע������
        /// </summary>
        /// <param name="services">����</param>
        /// <param name="dependencyType"></param>
        /// <param name="type">����</param>
        /// <param name="injectionAttribute">ע������</param>
        /// <param name="inter">�ӿ�</param>
        private static void Register(IServiceCollection services, Type dependencyType, Type type, InjectionAttribute injectionAttribute, Type inter = null)
        {
            // �޸�����ע������
            var fixedType = FixedGenericType(type);
            var fixedInter = inter == null ? null : FixedGenericType(inter);

            switch (injectionAttribute.Action)
            {
                case InjectionActions.Add:
                    if (fixedInter == null) services.InnerAdd(dependencyType, fixedType);
                    else
                    {
                        services.InnerAdd(dependencyType, fixedInter, fixedType);
                        AddDispatchProxy(services, dependencyType, fixedType, injectionAttribute.Proxy, fixedInter, true);
                    }
                    break;

                case InjectionActions.TryAdd:
                    if (fixedInter == null) services.InnerTryAdd(dependencyType, fixedType);
                    else services.InnerTryAdd(dependencyType, fixedInter, fixedType);
                    break;

                default: break;
            }
        }

        /// <summary>
        /// �����������
        /// </summary>
        /// <param name="services">���񼯺�</param>
        /// <param name="dependencyType"></param>
        /// <param name="type">���ص�����</param>
        /// <param name="proxyType">��������</param>
        /// <param name="inter">����ӿ�</param>
        /// <param name="hasTarget">�Ƿ���ʵ����</param>
        private static void AddDispatchProxy(IServiceCollection services, Type dependencyType, Type type, Type proxyType, Type inter, bool hasTarget = true)
        {
            proxyType ??= GlobalServiceProxyType;
            if (proxyType == null || (type != null && type.IsDefined(typeof(SuppressProxyAttribute), true))) return;

            // ע���������
            services.InnerAdd(dependencyType, typeof(AspectDispatchProxy), proxyType);

            // ע�����
            services.InnerAdd(dependencyType, inter, provider =>
            {
                dynamic proxy = DispatchCreateMethod.MakeGenericMethod(inter, proxyType).Invoke(null, null);
                proxy.Services = provider;
                if (hasTarget)
                {
                    proxy.Target = provider.GetService(type);
                }

                return proxy;
            });
        }

        /// <summary>
        /// ע���������񣨽ӿڶ�ʵ�֣�
        /// </summary>
        /// <typeparam name="TDependency"></typeparam>
        /// <param name="services"></param>
        private static void RegisterNamedService<TDependency>(IServiceCollection services)
            where TDependency : IPrivateDependency
        {
            // ע����������
            services.InnerAdd(typeof(TDependency), provider =>
            {
                object ResolveService(string named, TDependency _)
                {
                    var isRegister = TypeNamedCollection.TryGetValue(named, out var serviceType);
                    return isRegister ? provider.GetService(serviceType) : null;
                }
                return (Func<string, TDependency, object>)ResolveService;
            });
        }

        /// <summary>
        /// ע���ⲿ����
        /// </summary>
        /// <param name="services"></param>
        private static void RegisterExternalServices(IServiceCollection services)
        {
            // ��ȡѡ��
            var externalServices = App.GetConfig<DependencyInjectionSettingsOptions>("DependencyInjectionSettings", true);

            if (externalServices is { Definitions: not null })
            {
                // ����
                var extServices = externalServices.Definitions.OrderBy(u => u.Order);
                foreach (var externalService in extServices)
                {
                    var injectionAttribute = new InjectionAttribute
                    {
                        Action = externalService.Action,
                        Named = externalService.Named,
                        Order = externalService.Order,
                        Pattern = externalService.Pattern
                    };

                    // ���ش�������
                    if (!string.IsNullOrWhiteSpace(externalService.Proxy)) injectionAttribute.Proxy = GetProxyType(externalService.Proxy);

                    // ����ע������
                    var dependencyType = externalService.RegisterType switch
                    {
                        RegisterType.Transient => typeof(ITransient),
                        RegisterType.Scoped => typeof(IScoped),
                        RegisterType.Singleton => typeof(ISingleton),
                        _ => throw new InvalidOperationException("Unknown lifetime type.")
                    };

                    RegisterService(services, dependencyType,
                      GetProxyType(externalService.Service),
                      injectionAttribute,
                      new[] { GetProxyType(externalService.Interface) });
                }
            }
        }

        /// <summary>
        /// �޸���������ע����������
        /// </summary>
        /// <param name="type">����</param>
        /// <returns></returns>
        private static Type FixedGenericType(Type type)
        {
            if (!type.IsGenericType) return type;

            return Reflect.GetType(type.Assembly, $"{type.Namespace}.{type.Name}");
        }

        /// <summary>
        /// ��ȡ ע�� ����
        /// </summary>
        /// <param name="type">��������</param>
        /// <returns>int</returns>
        private static int GetOrder(Type type)
        {
            return !type.IsDefined(typeof(InjectionAttribute), true) ? 0 : type.GetCustomAttribute<InjectionAttribute>(true).Order;
        }

        /// <summary>
        /// ���ش������
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static Type GetProxyType(string str)
        {
            var typeDefinitions = str.Split(";");
            return Reflect.GetType(typeDefinitions[0], typeDefinitions[1]);
        }

        /// <summary>
        /// �������Ƽ���
        /// </summary>
        private static readonly ConcurrentDictionary<string, Type> TypeNamedCollection;

        /// <summary>
        /// �����������
        /// </summary>
        private static readonly MethodInfo DispatchCreateMethod;

        /// <summary>
        /// ȫ�ַ����������
        /// </summary>
        private static readonly Type GlobalServiceProxyType;

        /// <summary>
        /// ��̬���캯��
        /// </summary>
        static DependencyInjectionServiceCollectionExtensions()
        {
            // ��ȡȫ�ִ�������
            GlobalServiceProxyType = App.EffectiveTypes
                .FirstOrDefault(u => typeof(AspectDispatchProxy).IsAssignableFrom(u) && typeof(IGlobalDispatchProxy).IsAssignableFrom(u) && u.IsClass && !u.IsInterface && !u.IsAbstract);

            TypeNamedCollection = new ConcurrentDictionary<string, Type>();
            DispatchCreateMethod = typeof(AspectDispatchProxy).GetMethod(nameof(AspectDispatchProxy.Create));
        }
    }
}