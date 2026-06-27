namespace Shipeng.Util
{
    public class JWTPayload
    {
        public string CompanyId { get; set; }
        public string UserId { get; set; }
        public DateTime Expire { get; set; }
    }
}
