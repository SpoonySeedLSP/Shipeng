using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yitter.IdGenerator;

namespace Shipeng.EntityFrameworkCore
{
    /// <summary>
    /// 实体抽象基类
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// id
        /// </summary>
        [Key]
        [Column("F_Id", TypeName = "varchar(50)")]
        public virtual string Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("F_CreatorTime", TypeName = "timestamp")]
        public virtual DateTime CreatorTime { get; set; }

        /// <summary>
        /// 创建用户id
        /// </summary>
        [Column("F_CreatorUserId", TypeName = "varchar(50)")]
        public virtual string CreatorUserId { get; set; }

        /// <summary>
        /// 启用标识
        /// </summary>
        [Column("F_EnabledMark", TypeName = "int2")]
        public virtual int? EnabledMark { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [Column("F_LastModifyTime", TypeName = "timestamp")]
        public virtual DateTime? LastModifyTime { get; set; }

        /// <summary>
        /// 修改用户id
        /// </summary>
        [Column("F_LastModifyUserId", TypeName = "varchar(50)")]
        public virtual string LastModifyUserId { get; set; }

        /// <summary>
        /// 删除标志
        /// </summary>
        [Column("F_DeleteMark", TypeName = "int2")]
        public virtual int? DeleteMark { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        [Column("F_DeleteTime", TypeName = "timestamp")]
        public virtual DateTime? DeleteTime { get; set; }

        /// <summary>
        /// 删除用户id
        /// </summary>
        [Column("F_DeleteUserId", TypeName = "varchar(50)")]
        public virtual string DeleteUserId { get; set; }

        /// <summary>
        /// 创建
        /// </summary>
        public virtual void Creator()
        {
            var userId = App.User.FindFirst("UserId")?.Value;
            CreatorTime = DateTime.Now;
            Id = YitIdHelper.NextId().ToString();
            EnabledMark = EnabledMark == null ? 1 : EnabledMark;
            if (!string.IsNullOrEmpty(userId))
            {
                CreatorUserId = userId;
            }
        }

        /// <summary>
        /// 创建
        /// </summary>
        public virtual void Create()
        {
            var userId = App.User.FindFirst("UserId")?.Value;
            CreatorTime = DateTime.Now;
            Id = Id == null ? YitIdHelper.NextId().ToString() : Id;
            EnabledMark = EnabledMark == null ? 1 : EnabledMark;
            if (!string.IsNullOrEmpty(userId))
            {
                CreatorUserId = CreatorUserId == null ? userId : CreatorUserId;
            }
        }

        /// <summary>
        /// 修改
        /// </summary>
        public virtual void LastModify()
        {
            var userId = App.User.FindFirst("UserId")?.Value;
            LastModifyTime = DateTime.Now;
            if (!string.IsNullOrEmpty(userId))
            {
                LastModifyUserId = userId;
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        public virtual void Delete()
        {
            var userId = App.User.FindFirst("UserId")?.Value;
            DeleteTime = DateTime.Now;
            DeleteMark = 1;
            if (!string.IsNullOrEmpty(userId))
            {
                DeleteUserId = userId;
            }
        }
    }
}