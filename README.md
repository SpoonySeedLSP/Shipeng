# .NET 6 基于Abp Vnext

###  介绍

基于ABP vNext开源框架的WebApi开发框架
 

### 软件架构
 
.NET6
 

### 安装教程 


1. 建议使用VS2022启动
2. 配置.NET6.0 运行时

###  具备的基础设施 


1. 多数据库支持 MySQL、SQLServer、Oracle、Sqlite、PostgreSql
2. swagger分组
3. 日志面板地址：导航到 /logdashboard

###  CodeFisrt数据迁移
 

1. 找到项目中的 Shipeng.EntityFrameworkCore 层
2. 在程序包管理控制台执行 Add-Migration Init 生成迁移
3. 执行 Update-Database Init 还原数据库

 **项目结构(以及层依赖说明)** 

- Shipeng.Hosting : 启动项目,定义了过滤器以及中间件
- Shipeng.Application : 应用服务层,组合业务逻辑层业务,提交数据库保存
- Shipeng.Application.Contracts : 应用服务公共合约层,定义应用服务层接口,DTO对象
- Shipeng.DbMigrator：
- Shipeng.Domain: 领域服务层,业务逻辑处理核心层
- Shipeng.Domain.Shared : 领域服务共享层,定义公共的枚举,通用工具类等
- Shipeng.EntityFrameworkCore : 仓储实现层,依赖于领域服务,基于EF Core实现

