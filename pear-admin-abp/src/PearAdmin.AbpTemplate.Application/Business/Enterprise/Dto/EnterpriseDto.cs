using Abp.Application.Services.Dto;
using System;

namespace PearAdmin.AbpTemplate.Business.Enterprise.Dto
{
    /// <summary>
    /// 企业信息传输模型
    /// </summary>
    public class EnterpriseDto: EntityDto
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 宿主id
        /// </summary>
        public int? HostId { get; set; }

        /// <summary>
        /// 租户id
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// 统一社会信用代码
        /// </summary>
        public string CreditCode { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 组织机构代码
        /// </summary>
        public string CompanyCode { get; set; }

        /// <summary>
        /// 法定代表人
        /// </summary>
        public string LegalRepresentative { get; set; }

        /// <summary>
        /// 登记状态
        /// </summary>
        public string RegistrationStatus { get; set; }

        /// <summary>
        /// 成立日期
        /// </summary>
        public DateTime RegisterDate { get; set; }

        /// <summary>
        /// 注册资本
        /// </summary>
        public string RegisteredCapital { get; set; }

        /// <summary>
        /// 实缴资本
        /// </summary>
        public string ContributedCapital { get; set; }

        /// <summary>
        /// 工商注册号
        /// </summary>
        public string RegisteredCode { get; set; }

        /// <summary>
        /// 纳税人识别号
        /// </summary>
        public string TaxpayerIdentificationNumber { get; set; }

        /// <summary>
        /// 企业类型
        /// </summary>
        public string EnterpriseType { get; set; }

        /// <summary>
        /// 所属行业
        /// </summary>
        public string Industry { get; set; }

        /// <summary>
        /// 营业期限
        /// </summary>
        public string BusinessTerm { get; set; }

        /// <summary>
        /// 纳税人资质
        /// </summary>
        public string QualificationTaxpayer { get; set; }

        /// <summary>
        /// 所属地区
        /// </summary>
        public string EachDistrict { get; set; }

        /// <summary>
        /// 登记机关
        /// </summary>
        public string RegistrationAuthority { get; set; }

        /// <summary>
        /// 公司地址
        /// </summary>
        public string CompanyAddress { get; set; }

        /// <summary>
        /// 经营范围
        /// </summary>
        public string ScopeBusiness { get; set; }

        /// <summary>
        /// 公司简介
        /// </summary>
        public string CompanyProfile { get; set; }

        /// <summary>
        /// 公司信息
        /// </summary>
        public string CompanyInformation { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
}
