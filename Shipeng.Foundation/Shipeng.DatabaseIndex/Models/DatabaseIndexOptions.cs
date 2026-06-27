namespace Shipeng.DatabaseIndex
{
    /// <summary>
    /// 数据库索引自动检查配置。
    ///
    /// appsettings.json 示例：
    ///
    /// "DatabaseIndex": {
    ///   "EnableAutoEnsure": false,
    ///   "StartupDelaySeconds": 5,
    ///   "ThrowWhenFailed": false,
    ///   "AssemblyNamePrefixes": [ "Shipeng." ]
    /// }
    ///
    /// 建议：
    /// 1. 开发环境可以 EnableAutoEnsure = true。
    /// 2. 测试环境可以 EnableAutoEnsure = true。
    /// 3. 生产环境首次上线或新增索引时可以临时打开。
    /// 4. 生产稳定运行后建议关闭，避免每次启动都检查。
    /// </summary>
    public class DatabaseIndexOptions
    {
        /// <summary>
        /// 是否在应用启动后自动检查并创建索引。
        ///
        /// true:
        /// 程序启动后由 DatabaseIndexHostedService 自动执行。
        ///
        /// false:
        /// 程序启动时不自动执行。
        /// 可以后续通过后台接口或手动调用 IDatabaseIndexEnsureService.EnsureAsync() 执行。
        /// </summary>
        public bool EnableAutoEnsure { get; set; } = false;

        /// <summary>
        /// 启动延迟秒数。
        ///
        /// 为什么需要延迟：
        /// 1. 程序刚启动时，数据库连接、MongoDB连接、依赖注入、日志等可能还在初始化。
        /// 2. 延迟几秒可以避免启动瞬间给数据库造成压力。
        /// 3. 如果部署环境启动比较慢，可以设置大一点，比如 10 秒。
        /// </summary>
        public int StartupDelaySeconds { get; set; } = 5;

        /// <summary>
        /// 索引创建失败时是否抛出异常。
        ///
        /// true:
        /// 只要有一个索引创建失败，就抛出异常。
        /// 适合开发、测试、首次上线时使用，能及时暴露问题。
        ///
        /// false:
        /// 失败只记录日志，不中断应用启动。
        /// 适合生产稳定运行后使用，避免索引问题导致整站无法启动。
        /// </summary>
        public bool ThrowWhenFailed { get; set; } = false;

        /// <summary>
        /// 自动扫描 Provider 时允许加载的程序集名前缀。
        ///
        /// 作用：
        /// 1. 避免扫描所有第三方程序集，提高启动性能。
        /// 2. 避免误扫描系统程序集、NuGet程序集。
        ///
        /// 默认只扫描 Shipeng. 开头的程序集。
        /// 如果后续你的模块工程不是 Shipeng. 开头，可以在配置中加进去。
        /// </summary>
        public List<string> AssemblyNamePrefixes { get; set; } = new List<string> { "Shipeng." };

        /// <summary>
        /// 是否尝试从程序运行目录加载符合前缀的 dll。
        ///
        /// true:
        /// 会扫描 AppContext.BaseDirectory 下的 dll，把 Shipeng.*.dll 加载进来，
        /// 这样即使某些业务程序集暂时还没被 CLR 加载，也能扫描到 Provider。
        ///
        /// false:
        /// 只扫描当前 AppDomain 已经加载的程序集，速度最快，但可能漏掉未加载的业务程序集。
        /// </summary>
        public bool LoadAssembliesFromBaseDirectory { get; set; } = true;
    }
}