using Castle.DynamicProxy;

namespace Shipeng.Util
{
    internal class CastleInterceptor : AsyncInterceptorBase
    {
        private readonly IServiceProvider _serviceProvider;

        public CastleInterceptor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private IAOPContext _aopContext;
        private List<BaseAOPAttribute> _aops;

        private async Task Befor()
        {
            foreach (BaseAOPAttribute aAop in _aops)
            {
                await aAop.Befor(_aopContext);
            }
        }

        private async Task After()
        {
            foreach (BaseAOPAttribute aAop in _aops)
            {
                await aAop.After(_aopContext);
            }
        }

        private void Init(IInvocation invocation)
        {
            _aopContext = new CastleAOPContext(invocation, _serviceProvider);

            _aops = invocation.MethodInvocationTarget.GetCustomAttributes(typeof(BaseAOPAttribute), true)
                .Concat(invocation.InvocationTarget.GetType().GetCustomAttributes(typeof(BaseAOPAttribute), true))
                .Select(x => (BaseAOPAttribute)x)
                .ToList();
        }

        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            Init(invocation);
            await Befor();
            await proceed(invocation, proceedInfo);
            await After();
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            Init(invocation);
            await Befor();
            TResult result = await proceed(invocation, proceedInfo);
            invocation.ReturnValue = result;//设置返回值
            await After();
            return result;
        }
    }
}
