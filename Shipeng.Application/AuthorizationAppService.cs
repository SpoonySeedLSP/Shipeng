using Shipeng.Application.Contracts;
using Shipeng.Application.Contracts.Dtos.Authorization;
using Shipeng.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Mapster;
namespace Shipeng.Application
{
    public class AuthorizationAppService : BaseApplicationService, IAuthorizationAppService
    {
        private readonly IRepository<AuthenticationConfigure> _authenticationRepository;

        public AuthorizationAppService(IRepository<AuthenticationConfigure> authenticationRepository)
        {
            _authenticationRepository = authenticationRepository;

        }

        public async Task<ShipengResult<SaveAuthenticationDto>> GetAuthenticationAsync()
        {
            var result= (await _authenticationRepository.GetQueryableAsync())
                .ProjectToType<SaveAuthenticationDto>()
                .FirstOrDefault();
            if (result == null)
            {
                result = new SaveAuthenticationDto() 
                {
                    UseState = true
                };
            }
            return Ok(result);
        }

        public async Task<ShipengResult> SaveAuthenticationAsync(SaveAuthenticationDto authenticationDto)
        {
            var model =await _authenticationRepository.FirstOrDefaultAsync();
            if (model == null)
            {
                model = new AuthenticationConfigure(GuidGenerator.Create());
                TypeAdapter.Adapt(authenticationDto, model);
                await _authenticationRepository.InsertAsync(model);
            }
            else
            {
                TypeAdapter.Adapt(authenticationDto, model);
                
                await _authenticationRepository.UpdateAsync(model);
            }
            return Ok();
        }
    }
}
