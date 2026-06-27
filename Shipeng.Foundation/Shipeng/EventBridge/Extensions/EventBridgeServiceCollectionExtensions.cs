using Shipeng;
using Shipeng.Dependency;
using Shipeng.EventBridge;
using Shipeng.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// �¼����߷�����չ
    /// </summary>
    [SuppressSniffer]
    public static class EventBridgeServiceCollectionExtensions
    {
        /// <summary>
        /// ����¼����߷���
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddEventBridge(this IServiceCollection services)
        {
            return services.AddEventBridge<MemoryEventStoreProvider>();
        }

        /// <summary>
        /// ����¼����߷���
        /// </summary>
        /// <typeparam name="TEventStoreProvider"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddEventBridge<TEventStoreProvider>(this IServiceCollection services)
            where TEventStoreProvider : class, IEventStoreProvider
        {
            // ���������¼��������
            var eventHandlerTypes = App.EffectiveTypes.Where(u => u.IsClass && !u.IsInterface && !u.IsAbstract && typeof(IEventHandler).IsAssignableFrom(u));
            if (!eventHandlerTypes.Any()) return services;

            // ע���¼��洢�ṩ��
            services.AddTransient<IEventStoreProvider, TEventStoreProvider>();

            using var serviceProvider = InternalApp.InternalServices.BuildServiceProvider();

            // ����ע��
            foreach (var type in eventHandlerTypes)
            {
                // ע���¼��������
                services.AddTransient(type);
                services.AddTransient(typeof(IEventHandler), type);

                // �����¼��洢�ṩ�� [ע���¼�] �ӿڷ���
                var eventStoreProvider = serviceProvider.GetService<IEventStoreProvider>();
                eventStoreProvider.RegisterEventHandlerAsync(new EventHandlerMetadata
                {
                    AssemblyName = Reflect.GetAssemblyName(type),
                    Category = Event.GetEventHandlerCategory(type),
                    TypeFullName = type.FullName,
                    CreatedTime = DateTimeOffset.UtcNow
                }).GetAwaiter().GetResult();
            }

            // ע���¼��������ί��
            services.TryAddTransient(provider =>
            {
                IEventHandler eventHandlerResolve(EventMessageMetadata eventIdMetadata)
                {
                    // �������ͳ���
                    var eventHandlerType = Reflect.GetType(eventIdMetadata.AssemblyName, eventIdMetadata.TypeFullName);
                    return provider.GetService(eventHandlerType) as IEventHandler;
                }
                return (Func<EventMessageMetadata, IEventHandler>)eventHandlerResolve;
            });

            return services;
        }
    }
}