using AutoMapper;
using PearAdmin.AbpTemplate.Social.Chat;
using PearAdmin.AbpTemplate.Social.Chat.Dto;
using PearAdmin.AbpTemplate.Social.Friendships;
using PearAdmin.AbpTemplate.Social.Friendships.Cache;
using PearAdmin.AbpTemplate.Social.Friendships.Dto;

namespace PearAdmin.AbpTemplate.Social
{
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
