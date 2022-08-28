using Shipeng.Util;

namespace Shipeng.HRMS.Service.AutoJob
{
    public interface IJobTask
    {
        //执行方法
        Task<AlwaysResult> Start();
    }
}
