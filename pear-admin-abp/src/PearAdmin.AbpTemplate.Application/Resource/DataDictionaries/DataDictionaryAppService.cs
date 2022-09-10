using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using PearAdmin.AbpTemplate.Authorization;
using PearAdmin.AbpTemplate.Resource.DataDictionaries.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PearAdmin.AbpTemplate.Resource.DataDictionaries
{
    public class DataDictionaryAppService : AbpTemplateApplicationServiceBase, IDataDictionaryAppService
    {
        #region 初始化
        private readonly IRepository<DataDictionaryItem, int> _dataDictionaryItemRepository;

        public DataDictionaryAppService(IRepository<DataDictionaryItem, int> dataDictionaryItemRepository)
        {
            _dataDictionaryItemRepository = dataDictionaryItemRepository;
        }
        #endregion

        #region 数据字典
        /// <summary>
        /// 获取数据字典集合
        /// </summary>
        /// <returns></returns>
        public ListResultDto<DataDictionaryDto> GetAllDataDictionary()
        {
            var dataDictionaryTypes = Shared.Enumeration.GetAll<DataDictionaryType>();
            var dataDictionaryDtos = new List<DataDictionaryDto>();
            foreach (var dataDictionaryType in dataDictionaryTypes)
            {
                dataDictionaryDtos.Add(new DataDictionaryDto()
                {
                    Id = dataDictionaryType.Id,
                    TypeName = dataDictionaryType.Name
                });
            }

            return new ListResultDto<DataDictionaryDto>(dataDictionaryDtos);
        }

        /// <summary>
        /// 根据字典类型名称获取数据字典集合
        /// </summary>
        /// <param name="input">根据字典类型名称获取数据字典详细信息</param>
        /// <returns></returns>
        public async Task<ListResultDto<DataDictionaryDto>> GetDataDictionaryListByTypeNames(GetDataDictionaryListByTypeNamesInput input)
        {
            var dataDictionaryDtos = new List<DataDictionaryDto>();
            var dataDictionaryTypes = Shared.Enumeration.GetAll<DataDictionaryType>();

            if (input.TypeNames != null && input.TypeNames.Any())
            {
                dataDictionaryTypes = dataDictionaryTypes.Where(i => input.TypeNames.Contains(i.Name));
            }

            foreach (var dataDictionaryType in dataDictionaryTypes)
            {
                var dataDictionaryItems = await _dataDictionaryItemRepository.GetAllListAsync(d => d.DataDictionaryId == dataDictionaryType.Id);

                var dataDictionaryDto = new DataDictionaryDto()
                {
                    Id = dataDictionaryType.Id,
                    TypeName = dataDictionaryType.Name,
                    DataDictionaryItems = ObjectMapper.Map<List<DataDictionaryItemDto>>(dataDictionaryItems)
                };
                dataDictionaryDtos.Add(dataDictionaryDto);
            }

            return new ListResultDto<DataDictionaryDto>(ObjectMapper.Map<List<DataDictionaryDto>>(dataDictionaryDtos));
        }
        #endregion

        #region 数据字典项
        /// <summary>
        /// 获取数据字典列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ListResultDto<DataDictionaryItemDto>> GetAllDataDictionaryItem(GetAllDataDictionaryItemInput input)
        {
            var items = await _dataDictionaryItemRepository.GetAll()
                .WhereIf(input.DataDictionaryId.HasValue, di => di.DataDictionaryId == input.DataDictionaryId)
                .WhereIf(!input.FilterText.IsNullOrWhiteSpace(), di => di.Name.Contains(input.FilterText) || di.Code.Contains(input.FilterText))
                .ToListAsync();

            return new ListResultDto<DataDictionaryItemDto>(items.Select(item =>
            {
                return ObjectMapper.Map<DataDictionaryItemDto>(item);
            }).ToList());
        }

        /// <summary>
        /// 获取数据字典项
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<DataDictionaryItemDto> GetDataDictionaryItemForEdit(NullableIdDto<int> input)
        {
            var output = new DataDictionaryItemDto();

            if (input.Id.HasValue)
            {
                var dataDictionaryItem = await _dataDictionaryItemRepository.GetAsync(input.Id.Value);
                output = ObjectMapper.Map<DataDictionaryItemDto>(dataDictionaryItem);
            }

            return output;
        }

        /// <summary>
        /// 根据字典类型和字典项名称获取字典项值
        /// </summary>
        /// <param name="input">根据字典类型或业务代码获取字典项展示值</param>
        /// <returns></returns>
        public async Task<GetDataDictionaryItemNameOutput> GetDataDictionaryItemName(GetDataDictionaryItemNameInput input)
        {
            var dataDictionaryType = Shared.Enumeration.FromName<DataDictionaryType>(input.TypeName);

            var dataDictionaryItem = await _dataDictionaryItemRepository.GetAll()
                .Where(di => di.DataDictionaryId == dataDictionaryType.Id && di.Code == input.ItemCode)
                .FirstOrDefaultAsync();

            return new GetDataDictionaryItemNameOutput()
            {
                ItemName = dataDictionaryItem == null ? string.Empty : dataDictionaryItem.Name
            };
        }

        /// <summary>
        /// 创建数据字典项
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        [AbpAuthorize(AppPermissionNames.Pages_ResourceManagement_DataDictionary_DataDictionaryItem_Create)]
        public async Task CreateDataDictionaryItem(CreateDataDictionaryItemDto input)
        {
            var existedDataDictionaryItem = await _dataDictionaryItemRepository.GetAll()
                .Where(d => d.DataDictionaryId == input.DataDictionaryId)
                .Where(d => d.Name == input.Name)
                .AnyAsync();

            if (existedDataDictionaryItem)
            {
                throw new UserFriendlyException(L("该字典名称已存在，无法添加"));
            }

            var dataDictionaryItem = DataDictionaryItem.Builder(1, AbpSession.TenantId.Value, input.DataDictionaryId)
                .SetNameAndCode(input.Name, input.Code,input.Describe);

            await _dataDictionaryItemRepository.InsertAsync(dataDictionaryItem);
        }

        /// <summary>
        /// 更新数据字典项
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        [AbpAuthorize(AppPermissionNames.Pages_ResourceManagement_DataDictionary_DataDictionaryItem_Update)]
        public async Task UpdateDataDictionaryItem(UpdateDataDictionaryItemDto input)
        {
            var dataDictionaryItem = await _dataDictionaryItemRepository.GetAsync(input.Id);

            var isExistedDataDictionaryItem = await _dataDictionaryItemRepository.GetAll().Where(d => d.DataDictionaryId == dataDictionaryItem.DataDictionaryId && d.Name == input.Name && d.Id != dataDictionaryItem.Id).AnyAsync();
            if (isExistedDataDictionaryItem)
            {
                throw new UserFriendlyException(L("该字典名称已存在，无法更新"));
            }

            dataDictionaryItem.SetNameAndCode(input.Name, input.Code,input.Describe);

            await _dataDictionaryItemRepository.UpdateAsync(dataDictionaryItem);
        }

        /// <summary>
        /// 删除数据字典项
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        [AbpAuthorize(AppPermissionNames.Pages_ResourceManagement_DataDictionary_DataDictionaryItem_Delete)]
        public async Task DeleteDataDictionaryItem(List<EntityDto<int>> inputs)
        {
            foreach (var input in inputs)
            {
                await _dataDictionaryItemRepository.DeleteAsync(input.Id);
            }
        }
        #endregion   
    }
}
