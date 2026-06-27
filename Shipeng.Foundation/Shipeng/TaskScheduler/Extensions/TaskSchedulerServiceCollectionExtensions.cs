using Shipeng;
using Shipeng.Dependency;
using Shipeng.Extensions;
using Shipeng.TaskScheduler;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// ��������������չ
    /// </summary>
    [SuppressSniffer]
    public static class TaskSchedulerServiceCollectionExtensions
    {
        /// <summary>
        /// ���������ȷ���
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddTaskScheduler(this IServiceCollection services)
        {
            // ������������ [SpareTime] ���Եķ��������Һ���һ������������Ϊ SpareTimer ����
            var taskMethods = App.EffectiveTypes
                    // ��ѯ������������������
                    .Where(u => u.IsClass && !u.IsInterface && !u.IsAbstract && typeof(ISpareTimeWorker).IsAssignableFrom(u))
                    // ��ѯ�������������񷽷�
                    .SelectMany(u =>
                        u.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                         .Where(m => m.IsDefined(typeof(SpareTimeAttribute), false)
                                                    && m.GetParameters().Length == 2
                                                    && m.GetParameters()[0].ParameterType == typeof(SpareTimer)
                                                    && m.GetParameters()[1].ParameterType == typeof(long)
                                                    )
                         .GroupBy(m => m.DeclaringType));

            if (!taskMethods.Any()) return services;

            // �������е� Worker
            foreach (var item in taskMethods)
            {
                if (!item.Any()) continue;

                // �����������
                var typeInstance = Activator.CreateInstance(item.Key);

                foreach (var method in item)
                {
                    // �ж��Ƿ����첽����
                    var isAsyncMethod = method.IsAsync();

                    // ����ί������
                    var action = Delegate.CreateDelegate(isAsyncMethod ? typeof(Func<SpareTimer, long, Task>) : typeof(Action<SpareTimer, long>), typeInstance, method.Name);

                    // ��ȡ������������
                    var spareTimeAttributes = method.GetCustomAttributes<SpareTimeAttribute>();

                    // ע������
                    foreach (var spareTimeAttribute in spareTimeAttributes)
                    {
                        switch (spareTimeAttribute.Type)
                        {
                            // ִ�м������
                            case SpareTimeTypes.Interval:
                                // ִ��һ��
                                if (spareTimeAttribute.DoOnce)
                                {
                                    if (isAsyncMethod)
                                    {
                                        SpareTime.DoOnce(spareTimeAttribute.Interval, (Func<SpareTimer, long, Task>)action, spareTimeAttribute.WorkerName, spareTimeAttribute.Description, spareTimeAttribute.StartNow, executeType: spareTimeAttribute.ExecuteType);
                                    }
                                    else
                                    {
                                        SpareTime.DoOnce(spareTimeAttribute.Interval, (Action<SpareTimer, long>)action, spareTimeAttribute.WorkerName, spareTimeAttribute.Description, spareTimeAttribute.StartNow, executeType: spareTimeAttribute.ExecuteType);
                                    }
                                }
                                // �����ִ��
                                else
                                {
                                    if (isAsyncMethod)
                                    {
                                        SpareTime.Do(spareTimeAttribute.Interval, (Func<SpareTimer, long, Task>)action, spareTimeAttribute.WorkerName, spareTimeAttribute.Description, spareTimeAttribute.StartNow, executeType: spareTimeAttribute.ExecuteType);
                                    }
                                    else
                                    {
                                        SpareTime.Do(spareTimeAttribute.Interval, (Action<SpareTimer, long>)action, spareTimeAttribute.WorkerName, spareTimeAttribute.Description, spareTimeAttribute.StartNow, executeType: spareTimeAttribute.ExecuteType);
                                    }
                                }
                                break;
                            // ִ�� Cron ���ʽ����
                            case SpareTimeTypes.Cron:
                                if (isAsyncMethod)
                                {
                                    SpareTime.Do(spareTimeAttribute.CronExpression, (Func<SpareTimer, long, Task>)action, spareTimeAttribute.WorkerName, spareTimeAttribute.Description, spareTimeAttribute.StartNow, cronFormat: spareTimeAttribute.CronFormat == default ? default : (CronFormat)spareTimeAttribute.CronFormat, executeType: spareTimeAttribute.ExecuteType);
                                }
                                else
                                {
                                    SpareTime.Do(spareTimeAttribute.CronExpression, (Action<SpareTimer, long>)action, spareTimeAttribute.WorkerName, spareTimeAttribute.Description, spareTimeAttribute.StartNow, cronFormat: spareTimeAttribute.CronFormat == default ? default : (CronFormat)spareTimeAttribute.CronFormat, executeType: spareTimeAttribute.ExecuteType);
                                }
                                break;

                            default:
                                break;
                        }
                    }
                }
            }

            return services;
        }
    }
}