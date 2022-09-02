using AutoMapper;
using PearAdmin.AbpTemplate.Shared;
using PearAdmin.AbpTemplate.TaskCenter.DailyTasks;
using PearAdmin.AbpTemplate.TaskCenter.DailyTasks.Dto;

namespace PearAdmin.AbpTemplate.TaskCenter
{
    public class TaskCenterMapperProfile : Profile
    {
        public TaskCenterMapperProfile()
        {
            CreateMap<DailyTask, DailyTaskDto>()
                .ForMember(d => d.StartTime, options => options.MapFrom(t => t.DateRange.StartTime))
                .ForMember(d => d.EndTime, options => options.MapFrom(t => t.DateRange.EndTime))
                .ForMember(d => d.TaskStateTypeName, options => options.MapFrom(t => Enumeration.FromValue<TaskStateType>(t.TaskState.Id).Name));
        }
    }
}
