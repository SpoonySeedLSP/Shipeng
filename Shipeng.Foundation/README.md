# Shipeng.Foundation

**中文**：`Shipeng.Foundation` 是一个面向 .NET 10 的基础能力库，目标是把业务系统开发中高频、重复、容易出错的能力沉淀为可复用的工具、扩展、基础设施封装和集成组件。它覆盖 HTTP、缓存、Redis、MongoDB、SQL、SqlSugar、EF Core、RabbitMQ、MinIO、Excel、Word、二维码、加解密、序列化、分页、树结构、异常处理、任务队列、JWT、Swagger/OpenAPI、支付、短信、日志、对象映射、动态 API、数据库索引等常见场景。

**English**: `Shipeng.Foundation` is a .NET 10 foundation library for reusable application utilities, infrastructure helpers, integration components, and development accelerators. It covers HTTP, caching, Redis, MongoDB, SQL, SqlSugar, EF Core, RabbitMQ, MinIO, Excel, Word, QR codes, cryptography, serialization, pagination, tree structures, exception handling, task queues, JWT, Swagger/OpenAPI, payments, SMS, logging, object mapping, dynamic APIs, database indexes, and other common application scenarios.

> The public namespaces of many legacy helpers are intentionally kept as `Shipeng.Util` for compatibility with existing projects. The package, assembly, product, and project name are `Shipeng.Foundation`.
>
> 为了兼容已有项目，很多历史工具类的公开命名空间仍保留为 `Shipeng.Util`。当前包名、程序集名、产品名和项目名统一为 `Shipeng.Foundation`。

## Package Information / 包信息

| Item | Value |
| --- | --- |
| Package ID / 包 ID | `Shipeng.Foundation` |
| Assembly / 程序集 | `Shipeng.Foundation` |
| Target Framework / 目标框架 | `net10.0` |
| Current Version / 当前版本 | `4.0.0` |
| Nullable | `disable` for legacy compatibility / 为兼容旧代码暂时关闭 |
| Implicit Usings | `enable` |
| Package Readme | `README.md` |

## Installation / 安装

Install from a local or remote NuGet source:

```powershell
dotnet add package Shipeng.Foundation --version 4.0.0
```

If you are referencing this project from the same solution:

```xml
<ProjectReference Include="..\Shipeng.Foundation\Shipeng.Foundation.csproj" />
```

如果你在同一个解决方案中引用源码项目，请使用上面的 `ProjectReference`。如果你发布到 NuGet 或私有源，请使用 `dotnet add package`。

## Quick Start / 快速开始

Many existing helpers use the compatibility namespace `Shipeng.Util`:

```csharp
using Shipeng.Util;
using Shipeng.Util.Helper;
using Shipeng.Util.Cache;
```

Infrastructure modules use `Shipeng.*` namespaces:

```csharp
using Shipeng.Data.MongoDB.Repositorie;
using Shipeng.Data.SqlSugar.Repositories;
using Shipeng.Authentication.JwtBearer;
using Shipeng.DatabaseIndex;
```

Example:

```csharp
// String helper, encryption helper, file helper, HTTP helper, and many other APIs
// are available through the compatibility namespaces used by existing projects.
var isEmpty = string.IsNullOrWhiteSpace(input);
```

## What This Library Provides / 功能范围

### 1. Common Helpers / 通用工具

`Helper`, `Util`, `Extention`, `Primitives`, `Model`, `DTO`, `Enum`, `Security`, and related folders contain common utilities used by business applications:

- String, date, enum, object, collection, stream, JSON, URL, and expression extensions.
- File, path, ZIP, MIME, Excel, Word, QR code, image, verification code, and export helpers.
- AES, DES, MD5, JWT, certificate, random, ID card, pinyin, sensitive data masking, and encoding helpers.
- Pagination, tree data, AJAX result, service result, table metadata, dynamic model, and request/response primitives.
- Async-to-sync helper, task queue, lock helper, queue helper, and reflection helper.

中文说明：

- 提供字符串、日期、枚举、对象、集合、流、JSON、URL、表达式等扩展方法。
- 提供文件、路径、压缩包、MIME、Excel、Word、二维码、图片、验证码、导入导出等工具。
- 提供 AES、DES、MD5、JWT、证书、随机数、身份证、拼音、敏感信息脱敏、编码等辅助能力。
- 提供分页、树结构、统一返回结果、表格元数据、动态模型、请求响应基础模型等基础对象。
- 提供异步转同步、任务队列、锁、队列、反射等基础开发工具。

### 2. Data Access / 数据访问

The package includes multiple data access styles so different projects can choose the most suitable abstraction:

- Low-level ADO.NET helpers for SQL Server, MySQL, PostgreSQL, and Oracle.
- SqlSugar repository and unit-of-work helpers.
- MongoDB repository helpers.
- EF Core integration modules.
- Database table metadata and database index helper modules.
- Hybrid MongoDB + SQL Server pagination helper for mixed storage scenarios.

中文说明：

- 支持 SQL Server、MySQL、PostgreSQL、Oracle 等常见数据库的基础访问封装。
- 支持 SqlSugar 仓储、工作单元和 IOC 扩展。
- 支持 MongoDB 仓储封装。
- 支持 EF Core 基础设施集成。
- 支持数据库表结构信息、数据库索引初始化和索引配置。
- 支持 MongoDB 与 SQL Server 混合分页查询场景。

### 3. Cache and Messaging / 缓存与消息

Cache and messaging related modules include:

- Memory cache helpers.
- Redis helpers based on `CSRedisCore`, `Caching.CSRedis`, and `ServiceStack.Redis`.
- RabbitMQ helper and compatibility wrapper for modern `RabbitMQ.Client` APIs.
- Distributed ID helpers and sequential GUID generators.

中文说明：

- 提供内存缓存封装。
- 提供 Redis 字符串、Hash、List、Set、ZSet 等常见操作封装。
- 提供 RabbitMQ 消息发布、消费和新版客户端兼容处理。
- 提供分布式 ID、短 ID、顺序 GUID 等 ID 生成能力。

### 4. Web, API, and ASP.NET Core Infrastructure / Web 与 ASP.NET Core 基础设施

The `Shipeng` infrastructure modules provide reusable ASP.NET Core capabilities:

- Application startup, dependency injection, configurable options, CORS, localization, dynamic API controller, friendly exception, event bridge, task scheduler, and view engine helpers.
- Swagger/OpenAPI and MiniProfiler integration.
- JWT bearer authentication helpers.
- Object mapping modules based on AutoMapper and Mapster.
- Logging modules based on Serilog and log4net-related integration.

中文说明：

- 提供应用启动、依赖注入、配置选项、跨域、本地化、动态 API 控制器、友好异常、事件桥、任务调度、视图引擎等基础设施能力。
- 提供 Swagger/OpenAPI、MiniProfiler 集成。
- 提供 JWT Bearer 认证辅助能力。
- 提供 AutoMapper、Mapster 映射相关封装。
- 提供 Serilog、log4net 等日志相关集成。

### 5. Third-Party Integrations / 第三方集成

The library contains helpers for common external systems:

- MinIO object storage.
- WeChat payment and WeChat message helpers.
- SMS providers such as Huawei, Aliyun, Tencent Cloud, and other integrated APIs.
- JPush push notification helper.
- Logistics query helper.
- Office document helpers based on Aspose, NPOI, EPPlus, FreeSpire, and Office interop.
- QR code helpers based on QRCoder, ZXing, ThoughtWorks QRCode, and related drawing libraries.

中文说明：

- 支持 MinIO 对象存储。
- 支持微信支付、微信消息等常见微信生态接口封装。
- 支持华为云、阿里云、腾讯云等短信能力封装。
- 支持极光推送。
- 支持物流查询。
- 支持 Aspose、NPOI、EPPlus、FreeSpire、Office Interop 等 Office 文档处理方式。
- 支持 QRCoder、ZXing、ThoughtWorks QRCode 等二维码生成与解析能力。

## Design Principles / 设计原则

### Compatibility First / 兼容优先

The package keeps many legacy namespaces, method names, and DTO shapes because existing projects may already reference them. Renaming every public API would make the library cleaner internally but would break callers. Public compatibility is therefore treated as a first-class requirement.

本库保留了很多历史命名空间、方法名和 DTO 结构，因为已有项目可能已经引用这些 API。一次性重命名所有公开 API 会让内部更整齐，但会破坏调用方。因此兼容性是首要设计目标之一。

### Practical Performance / 实用性能优化

The library avoids obvious performance problems where possible:

- Reuses `HttpClient`-based patterns where available to reduce connection setup cost and socket exhaustion risk.
- Uses blocking/async primitives instead of busy waiting for queue-like workflows.
- Uses modern MongoDB, RabbitMQ, and cryptography APIs instead of removed or obsolete APIs.
- Keeps heavy integrations optional at call-site level so applications only execute the features they actually use.
- Uses repository and helper abstractions to reduce repeated reflection, repeated parsing, and duplicated configuration code in business projects.

本库尽量避免明显性能问题：

- 尽可能复用 `HttpClient` 相关模式，减少连接创建成本和 Socket 耗尽风险。
- 队列类逻辑使用阻塞/异步原语，避免空转等待。
- 使用新版 MongoDB、RabbitMQ、加密 API，替代已删除或过时 API。
- 重型集成按调用方实际使用触发，避免业务系统启动时无意义执行。
- 通过仓储和工具封装减少业务项目中重复反射、重复解析和重复配置代码。

### Clear Operational Boundaries / 清晰的运行边界

Some APIs are platform-specific or dependency-heavy. For example, `System.Drawing.Common`, Office interop, and some image/font features are best suited for Windows environments. Server-side Linux deployments should prefer cross-platform image libraries and avoid Office interop at runtime.

部分 API 具有平台限制或依赖较重。例如 `System.Drawing.Common`、Office Interop、部分图片和字体能力更适合 Windows 环境。Linux 服务端部署时建议优先选择跨平台图片库，并避免运行时依赖 Office Interop。

## Platform Notes / 平台说明

- Target framework is `net10.0`; callers should use a compatible .NET SDK/runtime.
- ASP.NET Core infrastructure features require `Microsoft.AspNetCore.App`.
- Some Office and drawing APIs are Windows-oriented.
- Some provider helpers require external service credentials and network availability.
- Database helpers require the corresponding database driver and correct connection string configuration.

中文说明：

- 目标框架为 `net10.0`，调用方需要使用兼容的 .NET SDK/runtime。
- ASP.NET Core 基础设施功能依赖 `Microsoft.AspNetCore.App`。
- 部分 Office 和绘图 API 更偏 Windows 环境。
- 第三方服务类工具需要对应服务密钥和网络环境。
- 数据库工具需要正确的驱动、连接字符串和数据库权限。

## NuGet Publishing Checklist / NuGet 发布检查清单

Before publishing a package, run:

```powershell
dotnet restore Shipeng.Foundation\Shipeng.Foundation.csproj
dotnet build Shipeng.Foundation\Shipeng.Foundation.csproj -t:Rebuild -v:q -clp:ErrorsOnly
dotnet build Shipeng.Foundation\Shipeng.Foundation.csproj -c Release -v:q -clp:ErrorsOnly
dotnet pack Shipeng.Foundation\Shipeng.Foundation.csproj -c Release --no-build -v:q -clp:ErrorsOnly
dotnet list Shipeng.Foundation\Shipeng.Foundation.csproj package --outdated
dotnet list Shipeng.Foundation\Shipeng.Foundation.csproj package --vulnerable --include-transitive
```

发布前建议确认：

- 编译结果必须为 `0 errors`。
- 尽量保持 `0 warnings`，除非某些兼容性警告已经明确评估并在项目文件中集中说明。
- NuGet 包必须没有已知漏洞。
- 包版本、发布说明、README、依赖版本需要同步。
- 如果修改了公开 API，需要在发布说明中写清楚兼容性影响。
- 如果新增平台相关功能，需要在文档中说明 Windows/Linux/macOS 差异。

## Version 4.0.0 Release Notes / 4.0.0 发布说明

English:

- Upgraded the library to `net10.0`.
- Renamed the project and NuGet package identity to `Shipeng.Foundation`.
- Preserved `Shipeng.Util` namespaces for source and binary compatibility.
- Added reusable infrastructure modules under `Shipeng.*`.
- Added MongoDB + SQL Server hybrid pagination helper.
- Added ID string helper for comma-separated database ID fields.
- Updated NuGet dependencies to current versions available from the configured sources.
- Removed or replaced vulnerable package versions where possible.
- Updated MongoDB, RabbitMQ, JWT, cryptography, and other APIs for modern .NET compatibility.
- Verified project build, Release build, package generation, outdated package check, and vulnerable package check.

中文：

- 升级目标框架到 `net10.0`。
- 项目名和 NuGet 包身份统一为 `Shipeng.Foundation`。
- 为了兼容旧项目，保留 `Shipeng.Util` 公开命名空间。
- 新增 `Shipeng.*` 基础设施模块。
- 新增 MongoDB + SQL Server 混合分页查询辅助类。
- 新增逗号分隔 ID 字段处理辅助类。
- 按当前 NuGet 源升级依赖到可用的较新版本。
- 尽量移除或替换存在漏洞的旧依赖版本。
- 更新 MongoDB、RabbitMQ、JWT、加密等 API，使其适配现代 .NET。
- 已验证项目编译、Release 编译、NuGet 打包、过期包检查和漏洞包检查。

## Naming and Compatibility / 命名与兼容

`Shipeng.Foundation` is the new package identity. `Shipeng.Util` remains a compatibility namespace. This means new users should install and reference the `Shipeng.Foundation` package, while existing source code may continue to use:

```csharp
using Shipeng.Util;
using Shipeng.Util.Helper;
using Shipeng.Util.Cache;
```

中文说明：

`Shipeng.Foundation` 是新的包身份；`Shipeng.Util` 是兼容命名空间。新用户应安装 `Shipeng.Foundation` 包；旧项目中已有的 `using Shipeng.Util` 可以继续保留。

## Suggested Usage Guidelines / 使用建议

- Prefer small, focused helpers for application code instead of placing all business logic inside utility classes.
- Keep credentials, connection strings, and provider secrets outside source code.
- Validate external inputs before passing them to file, database, payment, or network helpers.
- For high-throughput services, benchmark hot paths before adding heavy Office, image, reflection, or dynamic compilation features.
- For public APIs, wrap provider-specific exceptions into business-level errors before returning responses to clients.

中文建议：

- 工具类只负责通用能力，不建议把业务规则堆进工具类。
- 密钥、连接字符串、第三方服务凭证不要写死在源码中。
- 文件、数据库、支付、网络请求相关工具在调用前应校验外部输入。
- 高并发服务中使用 Office、图片、反射、动态编译等重型能力前，建议做性能压测。
- 对外 API 不建议直接暴露第三方 SDK 异常，应转换为业务可理解的错误信息。

## License Notes / 许可说明

This project references multiple third-party packages. Some packages may have their own commercial, redistribution, or runtime restrictions. Please review the license of each dependency before publishing a product that includes this package.

本项目引用了多个第三方包。部分包可能有商业授权、再分发或运行时限制。将本包用于正式产品发布前，请核查每个依赖包的许可证。

## Support / 支持

For internal projects, keep the package version aligned with the solution version. For public distribution, publish immutable versions and document breaking changes clearly.

内部项目建议保持包版本与解决方案版本一致。公开发布时建议使用不可变版本，并清楚记录破坏性变更。
