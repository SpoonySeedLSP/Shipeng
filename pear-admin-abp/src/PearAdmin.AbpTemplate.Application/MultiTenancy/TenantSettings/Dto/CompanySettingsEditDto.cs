using System;
using System.ComponentModel.DataAnnotations;

namespace PearAdmin.AbpTemplate.MultiTenancy.TenantSetting.Dto
{
    /// <summary>
    /// 企业设置修改传输模型
    /// </summary>
    public class CompanySettingsEditDto
    {
        /// <summary>
        /// 统一社会信用代码
        /// </summary>
        [Required(ErrorMessage = "统一社会信用代码不能为空")]
        public string CreditCode { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        [Required(ErrorMessage = "公司名称不能为空")]
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
        [Required(ErrorMessage = "请选择成立日期")]
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

    }
}
