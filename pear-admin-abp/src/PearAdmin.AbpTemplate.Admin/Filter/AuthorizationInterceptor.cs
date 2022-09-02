using Abp.Authorization;
using Abp.Dependency;
using Castle.DynamicProxy;
using System.Threading.Tasks;

namespace PearAdmin.AbpTemplate.Admin.Filter
{
    /// <summary>
    /// 权限拦截器实现-----该类用于拦截方法，以便在方法定义时进行授权
    /// Abp框架针对权限拦截器的实现则是简单了许多，
    /// 只是在被拦截的方法在执行的时候，会直接使用IAuthorizationHelper进行权限验证
    /// </summary>
    public class AuthorizationInterceptor : AbpInterceptorBase, ITransientDependency
    {
        private readonly IAuthorizationHelper _authorizationHelper;

        public AuthorizationInterceptor(IAuthorizationHelper authorizationHelper)
        {
            _authorizationHelper = authorizationHelper;
        }

        public void Intercept(IInvocation invocation)
        {
            // 使用 IAuthorizationHelper 进行权限验证
            _authorizationHelper.Authorize(invocation.MethodInvocationTarget, invocation.TargetType);
            //继续调用下一个拦截器，并最终调用目标方法
            invocation.Proceed();
        }

        public override void InterceptSynchronous(IInvocation invocation)
        {
            _authorizationHelper.Authorize(invocation.MethodInvocationTarget, invocation.TargetType);
            invocation.Proceed();
        }

        protected override async Task InternalInterceptAsynchronous(IInvocation invocation)
        {
            var proceedInfo = invocation.CaptureProceedInfo();

            await _authorizationHelper.AuthorizeAsync(invocation.MethodInvocationTarget, invocation.TargetType);

            proceedInfo.Invoke();
            var task = (Task)invocation.ReturnValue;
            await task;
        }

        protected override async Task<TResult> InternalInterceptAsynchronous<TResult>(IInvocation invocation)
        {
            var proceedInfo = invocation.CaptureProceedInfo();

            await _authorizationHelper.AuthorizeAsync(invocation.MethodInvocationTarget, invocation.TargetType);

            proceedInfo.Invoke();
            var taskResult = (Task<TResult>)invocation.ReturnValue;
            return await taskResult;
        }

    }
}
