using EFCore.Sharding;

namespace Shipeng.Util
{
    public class DatabaseOptions
    {
        public string ConnectionString { get; set; }
        public DatabaseType DatabaseType { get; set; }
    }
}
