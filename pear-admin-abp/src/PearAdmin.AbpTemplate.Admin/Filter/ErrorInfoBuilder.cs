using Abp.Dependency;
using Abp.Localization;
using Abp.Web.Configuration;
using Abp.Web.Models;
using System;

namespace PearAdmin.AbpTemplate.Admin.Filter
{
    /// <summary>
    /// 异常包装器
    /// 当 Abp 捕获到异常之后，会通过 IErrorInfoBuilder 的 BuildForException() 方法来将异常转换为 ErrorInfo 对象
    /// 它的默认实现只有一个，就是 ErrorInfoBuilder ，内部结构也很简单，
    /// 其 BuildForException() 方法直接通过内部的一个转换器进行转换，
    /// 也就是 IExceptionToErrorInfoConverter，直接调用的 IExceptionToErrorInfoConverter.Convert() 方法
    /// 时它拥有另外一个方法，叫做 AddExceptionConverter()，可以传入你自己实现的异常转换器
    /// </summary>
    public class ErrorInfoBuilder : IErrorInfoBuilder, ISingletonDependency
    {
        private static IExceptionToErrorInfoConverter Converter { get; set; }

        public ErrorInfoBuilder(IAbpWebCommonModuleConfiguration configuration, ILocalizationManager localizationManager)
        {
            // 异常包装器默认使用的 DefaultErrorInfoConverter 来进行转换
            //Converter = new DefaultErrorInfoConverter(configuration, localizationManager);
        }

        // 根据异常来构建异常信息
        public ErrorInfo BuildForException(Exception exception)
        {
            return Converter.Convert(exception);
        }

        // 添加用户自定义的异常转换器
        public void AddExceptionConverter(IExceptionToErrorInfoConverter converter)
        {
            converter.Next = Converter;
            Converter = converter;
        }      

    }
}
