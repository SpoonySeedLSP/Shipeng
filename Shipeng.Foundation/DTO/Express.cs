namespace Shipeng.Util
{
    public class ExpressKDNDTO
    {
        public string LogisticCode { get; set; }
        public string EBusinessID { get; set; }
        public string Code { get; set; }
        public bool Success { get; set; }
        public List<Shipper> Shippers { get; set; }
    }

    public class Shipper
    {
        public string ShipperName { get; set; }
        public string ShipperCode { get; set; }
    }

    public class OrderTracesData
    {
        public string OrderId { get; set; }

        public string GoodsImg { get; set; }

        //public W_UserShippingAddress UserShipping { get; set; }

        /// <summary>
        /// 快递号
        /// </summary>
        public string LogisticCode { get; set; }
        /// <summary>
        /// 快递类型code
        /// </summary>
        public string ShipperCode { get; set; }
        /// <summary>
        /// 快递名称
        /// </summary>
        public string ShipperName { get; set; }

        /// <summary>
        /// 物流列表
        /// </summary>
        public List<TraceData> Traces { get; set; }
        public int State { get; set; }
        public string Reason { get; set; }
        public bool Success { get; set; }

    }

    public class TraceData
    {
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime AcceptTime { get; set; }

        /// <summary>
        /// 物流描述
        /// </summary>
        public string AcceptStation { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }

    public class ExpressKD100DTO
    {
        public string ComCode { get; set; }
        public string Num { get; set; }
        public List<SingleDTO> Auto { get; set; }
    }
    public class MessageModel<T>
    {
        public bool Success { get; set; } = false;
        public string Msg { get; set; } = "请求异常";
        public T Response { get; set; }
    }

    /// <summary>
    /// 物流信息参数字段
    /// </summary>
    public class ParamDTO
    {
        public string Com { get; set; }
        public string Num { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Phone { get; set; }
    }

    public class DataDTO
    {
        public string Time { get; set; }
        public string Ftime { get; set; }
        public string Context { get; set; }
    }

    public class SingleDTO
    {
        public string ComCode { get; set; }
        public string Id { get; set; }
        public string NoCount { get; set; }
        public string NoPre { get; set; }
        public string StartTime { get; set; }
    }

    public class TestDateDTO
    {
        public List<HttpDataDTO> Data { get; set; }
    }

    public class HttpDataDTO
    {
        public string CreateDate { get; set; }
        public string JobName { get; set; }
        public string JobOther { get; set; }
    }

    public class ResponseDataDto
    {
        public string Message { get; set; }
        public string Ischeck { get; set; }
        public string Nu { get; set; }
        public string Condition { get; set; }
        public string Com { get; set; }
        public string Status { get; set; }
        public string State { get; set; }
        public bool Result { get; set; }
        public string ReturnCode { get; set; }
        public List<DataDTO> Data { get; set; }
    }

    /// <summary>
    /// 提取码DTO
    /// </summary>
    public class ClaimGoodsCodeDTO
    {
        /// <summary>
        /// 提取码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 提取二维码(Base64)
        /// </summary>
        public string QRcode { get; set; }
    }
}
