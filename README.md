.NET 6 基于Abp Vnext


项目结构(以及层依赖说明)
Shipeng.Hosting : 启动项目,定义了过滤器以及中间件
Shipeng.Application : 应用服务层,组合业务逻辑层业务,提交数据库保存
Shipeng.Application.Contracts : 应用服务公共合约层,定义应用服务层接口,DTO对象
Shipeng.Domain: 领域服务层,业务逻辑处理核心层
Shipeng.Domain.Shared : 领域服务共享层,定义公共的枚举,通用工具类等
Shipeng.EntityFrameworkCore : 仓储实现层,依赖于领域服务,基于EF Core实现
