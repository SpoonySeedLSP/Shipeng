namespace Shipeng.HRMS.Domain
{
    public class LogoInfoEntity
    {
        public string title { get; set; }
        public string href { get; set; }
        public string image { get; set; }
        public LogoInfoEntity()
        {
            title = "Shipeng.HRMS";
            href = "";
            image = "../icon/favicon.ico";
        }
    }
}