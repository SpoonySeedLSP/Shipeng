using Abp.Dependency;
using Abp.Events.Bus.Exceptions;
using Abp.Events.Bus.Handlers;
using System;

namespace PearAdmin.AbpTemplate.Admin.Filter
{
    /// <summary>
    /// 监听异常事件
    /// 使用 Abp 框架的时候，你可以随时通过监听 AbpHandledExceptionData 事件来使用自己的逻辑处理产生的异常
    /// 比如说产生异常时向监控服务报警，或者说将异常信息持久化到其他数据库等等
    /// </summary>
    public class ExceptionEventHandler : IEventHandler<AbpHandledExceptionData>, ITransientDependency
    {
        /// <summary>
        /// 处理程序通过实现此方法来处理事件
        /// </summary>
        /// <param name="eventData">事件数据</param>
        public void HandleEvent(AbpHandledExceptionData eventData)
        {
            Console.WriteLine($"当前异常信息为：{eventData.Exception.Message}");
        }
    }
}
