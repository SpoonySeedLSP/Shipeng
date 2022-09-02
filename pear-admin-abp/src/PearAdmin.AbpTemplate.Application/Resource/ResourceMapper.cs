using AutoMapper;
using PearAdmin.AbpTemplate.Resource.DataDictionaries;
using PearAdmin.AbpTemplate.Resource.DataDictionaries.Dto;

namespace PearAdmin.AbpTemplate.Resource
{
    public class ResourceMapperProfile : Profile
    {
        public ResourceMapperProfile()
        {
            CreateMap<DataDictionaryItem, DataDictionaryItemDto>();
        }
    }
}
