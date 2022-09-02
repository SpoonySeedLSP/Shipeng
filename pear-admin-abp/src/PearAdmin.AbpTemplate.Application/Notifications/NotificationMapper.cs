using Abp.Notifications;
using AutoMapper;
using PearAdmin.AbpTemplate.Notifications.Dto;

namespace PearAdmin.AbpTemplate.Notifications
{
    public class NotificationMapperProfile : Profile
    {
        public NotificationMapperProfile()
        {
            CreateMap<NotificationDefinition, NotificationSubscriptionWithDisplayNameDto>();
        }
    }
}
