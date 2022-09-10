using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace PearAdmin.AbpTemplate.Resource.DataDictionaries
{
    /// <summary>
    /// 资源_数据字典项
    /// </summary>
    [Table("Resource_DataDictionaryItem")]
    public class DataDictionaryItem : AggregateRoot<int>, IMustHaveTenant
    {
        public const int MaxCodeLength = 5;
        public const int MaxNameLength = 30;
        public const int MaxDescribeLength = 500;

        public DataDictionaryItem()
        {

        }

        public DataDictionaryItem(int hostId,int tenantId, int dataDictionaryId)
        {
            HostId = hostId;
            TenantId = tenantId;
            DataDictionaryId = dataDictionaryId;
        }

        public static DataDictionaryItem Builder(int hostId,int tenantId, int dataDictionaryId)
        {
            return new DataDictionaryItem(hostId,tenantId, dataDictionaryId);
        }

        public DataDictionaryItem SetNameAndCode(string name, string code,string describe="")
        {
            Code = code;
            Name = name;
            Describe = describe;
            return this;
        }

        /// <summary>
        /// 宿主id
        /// </summary>
        public int? HostId { get; set; }

        /// <summary>
        /// 租户Id
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// 企业id
        /// </summary>
        public long EnterpriseId { get; set; }

        /// <summary>
        /// 业务代码
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// 类型项名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(500)]
        public string Describe { get; set; }

        /// <summary>
        /// 数据字典Id
        /// </summary>
        public int DataDictionaryId { get; private set; }
    }
}
