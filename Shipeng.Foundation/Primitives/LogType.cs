using System.ComponentModel;

namespace Shipeng.Util
{
    /// <summary>
    /// 系统日志类型
    /// </summary>
    public enum UserLogType
    {
        #region Base

        [Description("系统权限")]
        Base_Action = 0,

        [Description("系统参数")]
        Base_AppSecret = 1,

        [Description("测试")]
        Base_BuildTest = 2,

        [Description("公司管理")]
        Base_Company = 3,

        [Description("数据权限")]
        Base_DataPermissions = 4,

        [Description("数据库连接")]
        Base_DbLink = 5,

        [Description("部门管理")]
        Base_Department = 6,

        [Description("角色管理")]
        Base_Role = 7,

        [Description("角色权限管理")]
        Base_RoleAction = 8,

        [Description("快捷方式")]
        Base_Shortcut = 9,

        [Description("修改内容记录")]
        Base_UpdateLog = 10,

        [Description("后台用户管理")]
        Base_User = 11,

        [Description("日志管理")]
        Base_UserLog = 12,

        [Description("用户角色")]
        Base_UserRole = 13,

        [Description("App版本")]
        Base_AppVersion = 14,

        [Description("数据权限")]
        Base_DataAuthority = 15,

        [Description("微信小程序码参数")]
        Base_QRCodeParameters = 16,

        [Description("数据修改记录")]
        Base_HistoryData = 17,
        #endregion Base

        [Description("项目相册")]
        W_BuildAlbum = 50,

        [Description("楼盘项目信息")]
        W_BuildInformation = 51,

        [Description("楼栋信息")]
        W_FloorInformation = 52,

        [Description("房源/车位/店铺")]
        W_HouseInformation = 53,

        [Description("户型")]
        W_HouseTypeInformation = 54,

        [Description("位置周边")]
        W_LocationPerimeter = 55,

        [Description("取货点")]
        W_PickupPoints = 56,

        [Description("专属管家")]
        I_HouseExclusiveButler = 57,


        [Description("评论")]
        W_Comment = 100,

        [Description("反馈")]
        W_Feedback = 101,

        [Description("帮助中心")]
        W_HelpCenter = 103,

        [Description("敏感词")]
        W_KeyFilt = 104,

        [Description("关键词")]
        W_keyword = 105,

        [Description("友情链接")]
        W_Links = 106,

        [Description("文章管理")]
        W_News = 107,

        [Description("SEO")]
        W_Seo = 108,

        [Description("分类")]
        W_Sort = 109,

        [Description("公告")]
        W_Notice = 110,

        [Description("关注浏览")]
        W_Concern = 111,

        [Description("省市区")]
        W_StatisticsRegion = 120,

        [Description("处理结果")]
        W_Trial = 121,

        [Description("项目规则")]
        W_Allocation = 122,

        [Description("产品参数")]
        W_Parameter = 123,

        [Description("产品参数模型")]
        W_ParameterModel = 124,


        [Description("折扣信息")]
        W_DiscountItem = 150,

        [Description("图片视频")]
        W_Img = 151,

        [Description("上传文件")]
        W_Upload = 152,

        [Description("消息")]
        W_MessageOrdinary = 153,

        [Description("用户消息")]
        W_MessageOrdinaryUser = 154,

        [Description("消息模板")]
        W_MessageTemplate = 155,

        [Description("申请记录")]
        W_Apply = 156,

        [Description("会员管理")]
        W_User = 157,

        [Description("用户关注记录")]
        W_UserFollows = 158,

        [Description("浏览记录页面路径")]
        W_UserLocusPage = 159,

        [Description("会员浏览记录")]
        W_UserLocus = 160,

        [Description("支付记录")]
        W_PayLogs = 161,

        [Description("物料广告")]
        W_MaterialAdverCode = 162,

        [Description("物料广告浏览记录")]
        W_MaterialAdverCodeLog = 163,

        [Description("积分记录")]
        W_IntegralRecord = 167,

        [Description("收货地址")]
        W_UserShippingAddress = 168,

        [Description("分享")]
        W_QRShare = 169,

        [Description("分享后的浏览记录")]
        W_QRShareLog = 170,

        [Description("购物车")]
        W_UserShoppingCart = 171,

        #region 产品订单
        [Description("产品表")]
        W_Commodity = 200,

        [Description("产品规格表")]
        W_CommoditySKU = 201,

        [Description("订单表")]
        W_CommodityOrder = 202,

        [Description("订单明细表")]
        W_CommodityOrderList = 203,

        [Description("订单退款记录表")]
        W_CommodityOrderRefund = 204,

        [Description("拼团信息")]
        W_CommodityGroupInfo = 205,
        #endregion

        #region 流程
        [Description("流程模板节点明细表")]
        F_WorkflowTempLable = 300,

        [Description("流程模板节点明细表")]
        F_WorkflowInstanceTempLable = 301,

        [Description("流程模板节点审核用户明细表")]
        F_WorkflowInstanceUserTempLable = 302,

        [Description("流程模板节点明细表")]
        F_Workflow = 303,

        [Description("流程节点明细表")]
        F_WorkflowInstance = 304,

        [Description("流程操作节点用户明细表")]
        F_WorkflowInstanceUser = 305,

        [Description("流程判断条件类型")]
        F_WorkflowInstanceConditionTypes = 306,
        #endregion

        [Description("小程序模板")]
        X_Template = 350,

        [Description("小程序页面")]
        X_Page = 351,

        [Description("小程序按钮")]
        X_Button = 352,

        [Description("小程序")]
        X_Applet = 353,

        [Description("等级信息")]
        W_Levelinfo = 354,

        [Description("小程序页面按钮分组")]
        X_ButtonGroup = 355,

        [Description("小区设施")]
        R_Facilities = 400,

        [Description("设施巡检记录")]
        R_InspectionRecord = 401,

        [Description("客户/设施维修记录")]
        R_RepairsMaintain = 402,

        [Description("维修记录明细")]
        R_RepairsMaintainList = 403,

        [Description("报修项目、巡检项目配置")]
        R_RepairsSet = 404,

        [Description("设施巡检计划")]
        R_RepairsPatrolPlan = 405,

        [Description("设施巡检计划巡检人明细")]
        R_RepairsPatrolPlanList = 406,

        [Description("设施巡检路线")]
        R_RepairsRoute = 407,

        [Description("设施巡检路线设施明细")]
        R_RepairsRouteList = 408,

        [Description("业主信息")]
        O_HouseOwner = 450,

        [Description("房屋访客记录")]
        O_VisitorLetter = 451,

        [Description("业主绑定明细信息")]
        O_HouseOwnerRelation = 452,

        [Description("缴费明细")]
        E_ExpenseHouseList = 500,

        [Description("房屋缴费清单")]
        E_ExpenseHouseSet = 501,

        [Description("水电抄表记录")]
        E_ExpenseStatement = 502,

        [Description("费用项配置")]
        E_ExpenseSet = 503,

        [Description("凭据模板")]
        E_EvidenceTemplable = 504,

        [Description("车辆信息")]
        V_VehicleInformation = 550,

        [Description("停车场")]
        V_VehicleParking = 551,

        [Description("车辆申请记录")]
        V_VehicleApply = 552,

        [Description("物品调拨记录")]
        C_WareAllotRecord = 600,

        [Description("仓库信息")]
        C_Warehouse = 601,

        [Description("物品清单明细")]
        C_WareInventoryList = 602,

        [Description("物品出库入库")]
        C_WareStorageRecords = 603,

        [Description("供货商")]
        C_WareSupplier = 604,

        [Description("仓库物品清单")]
        C_WareInventory = 605,

        [Description("广告")]
        W_Adver = 1000
    }

    /// <summary>
    /// 业务类型:0为浏览,1:关注,2:点赞:3:推荐,4:预定
    /// </summary>
    public enum ConcernTypes
    {
        [Description("浏览")]
        Browse = 0,

        [Description("收藏")]
        Follow = 1,

        [Description("点赞")]
        PointPraise = 2,

        [Description("推荐")]
        Recommend = 3,

        [Description("预定")]
        Order = 4,
    }
}
