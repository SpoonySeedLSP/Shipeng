using AutoMapper;
using PearAdmin.AbpTemplate.Social.Chat;
using PearAdmin.AbpTemplate.Social.Chat.Dto;
using PearAdmin.AbpTemplate.Social.Friendships;
using PearAdmin.AbpTemplate.Social.Friendships.Cache;
using PearAdmin.AbpTemplate.Social.Friendships.Dto;

namespace PearAdmin.AbpTemplate.Social
{
    /// <summary>
    /// Social映射配置文件
    /// </summary>
    public class SocialMapperProfile : Profile
    {
        public SocialMapperProfile()
        {
            CreateMap<Friendship, FriendDto>();
            CreateMap<FriendCacheItem, FriendDto>();
            CreateMap<ChatMessage, ChatMessageDto>();
        }
    }
}
