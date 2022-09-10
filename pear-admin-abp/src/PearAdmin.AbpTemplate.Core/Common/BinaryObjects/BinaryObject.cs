using System;
using System.ComponentModel.DataAnnotations;
using Abp;
using Abp.Domain.Entities;

namespace PearAdmin.AbpTemplate.BinaryObjects
{
    /// <summary>
    /// 附件实体
    /// 后端与上传文件相关的代码包括领域实体层的BinaryObject，这里对其扩展，添加了文件类型、大小等相关字段
    /// </summary>
    public class BinaryObject : AggregateRoot<Guid>, IMayHaveTenant
    {
        /// <summary>
        /// 租户ID
        /// </summary>
        public virtual int? TenantId { get; set; }

        /// <summary>
        /// 文件类型【拓展字段】
        /// </summary>
        public virtual string ContentType { get; set; }

        /// <summary>
        /// 文件名称【拓展字段】
        /// </summary>
        public virtual string FileName { get; set; }

        /// <summary>
        /// 文件大小【拓展字段】
        /// </summary>
        public virtual long FileSize { get; set; }

        /// <summary>
        /// 二进制数据
        /// </summary>
        [Required]
        public virtual byte[] Bytes { get; set; }

        public BinaryObject()
        {
            Id = SequentialGuidGenerator.Instance.Create();
        }

        public BinaryObject(int? tenantId, byte[] bytes)
            : this()
        {
            TenantId = tenantId;
            Bytes = bytes;
        }
    }
}
