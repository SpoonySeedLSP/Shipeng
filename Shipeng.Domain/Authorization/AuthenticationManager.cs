using Shipeng.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.Domain.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace Shipeng.Domain.Authorization
{
    internal class AuthenticationManager : DomainService, IAuthenticationManager
    {
        private const string CacheKey = "AuthenticationConfigure";
        //
        private readonly IRepository<AuthenticationConfigure> _repository;
        private readonly IMemoryCache _memoryCache;

        public AuthenticationManager(IMemoryCache memoryCache, IRepository<AuthenticationConfigure> repository)
        {
            _memoryCache = memoryCache;
            _repository = repository;
        }

        /// <summary>
        /// 获取鉴权配置
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<AuthenticationConfigure> GetConfigureAsync()
        {
            var data = _memoryCache.Get<AuthenticationConfigure>(CacheKey);
            if (data == null)
            {
                data = await _repository.FirstOrDefaultAsync();
                if (data == null)
                {
                    throw new ArgumentNullException("身份认证信息未配置");
                }
                _memoryCache.Set(CacheKey, data, DateTimeOffset.Now.AddDays(7));
            }
            return data;
        }
    }
}
