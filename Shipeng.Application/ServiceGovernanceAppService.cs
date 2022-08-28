using Mapster;
using Shipeng.Application.Contracts;
using Shipeng.Application.Contracts.Dtos.ServiceGovernance;
using Shipeng.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Shipeng.Application
{
    public class ServiceGovernanceAppService : BaseApplicationService, IServiceGovernanceAppService
    {
        private readonly IRepository<ServiceGovernanceConfigure> _repository;

        public ServiceGovernanceAppService(IRepository<ServiceGovernanceConfigure> repository)
        {
            _repository = repository;
        }

        public async Task<ShipengResult<ServiceGovernanceConfigureDto>> GetServiceGovernanceConfigureAsync()
        {
            var result = (await _repository.GetQueryableAsync())
                .ProjectToType<ServiceGovernanceConfigureDto>()
                .FirstOrDefault();
            if (result == null)
            {
                result = new ServiceGovernanceConfigureDto();
            }
            return Ok(result);
        }

        public async Task<ShipengResult> SaveServiceGovernanceConfigureAsync(ServiceGovernanceConfigureDto configure)
        {
            var model = await _repository.FirstOrDefaultAsync();
            if (model == null)
            {
                model = new ServiceGovernanceConfigure(GuidGenerator.Create());
                TypeAdapter.Adapt(configure, model);
                await _repository.InsertAsync(model);
            }
            else
            {
                TypeAdapter.Adapt(configure, model);
                await _repository.UpdateAsync(model);
            }
            return Ok();
        }
    }
}
