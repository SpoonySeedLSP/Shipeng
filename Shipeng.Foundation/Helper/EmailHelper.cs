using MailKit.Net.Smtp;
using MimeKit;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Shipeng.Util
{
    /// <summary>
    /// 邮件帮助类
    /// Author:李仕鹏
    /// </summary>
    public static class EmailHelper
    {
        /// <summary>
        /// 发送者名称
        /// </summary>
        private static readonly string SendName = "今喜";

        /// <summary>
        /// 发送者账号
        /// </summary>
        private static readonly string SendAccountName = "service@quanguovip.cn";

        /// <summary>
        /// 发送者服务器地址：例如：smtp.163.com
        /// </summary>
        private static readonly string SmtpHost = "smtp.exmail.qq.com";

        /// <summary>
        /// 服务器端口号：例如：25
        /// </summary>
        private static readonly int SmtpPort = 25;

        /// <summary>
        /// 发送者登录邮箱账号的客户端授权码
        /// </summary>
        private static readonly string AuthenticatePassword = "2C8DiZJMkTCHEtmC";

        #region MimeKit发送邮件

        /// <summary>
        /// 邮件发送
        /// </summary>
        /// <param name="receiverAccountNameList"> 接收者账号 </param>
        /// <param name="mailSubject"> 邮件主题 </param>
        /// <param name="sendHtml"> 文本html(与sendText参数互斥，传此值则 sendText传null) </param>
        /// <param name="sendText"> 纯文本(与sendHtml参数互斥，传此值则 sendHtml传null) </param>
        /// <param name="accessoryList"> 邮件的附件 </param>
        public static async Task SendEmail(List<string> receiverAccountNameList, string mailSubject, string sendHtml, string sendText, List<MimePart> accessoryList = null)
        {
            if (receiverAccountNameList.Any() && accessoryList.Any())
            {
                try
                {
                    MimeMessage message = new MimeMessage();
                    message.From.Add(new MailboxAddress(SendName, SendAccountName));
                    List<MailboxAddress> mailboxAddressList = new List<MailboxAddress>();
                    //receiverAccountNameList.ForEach(f =>
                    //{
                    //    mailboxAddressList.Add(new MailboxAddress());
                    //});
                    message.To.AddRange(mailboxAddressList);

                    message.Subject = mailSubject;

                    Multipart alternative = new Multipart("alternative");
                    if (!string.IsNullOrWhiteSpace(sendText))
                    {
                        alternative.Add(new TextPart("plain")
                        {
                            Text = sendText
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(sendHtml))
                    {
                        alternative.Add(new TextPart("html")
                        {
                            Text = sendHtml
                        });
                    }
                    Multipart multipart = new Multipart("mixed")
                {
                    alternative
                };
                    if (accessoryList != null)
                    {
                        accessoryList?.ForEach(f =>
                        {
                            multipart.Add(f);
                        });
                    }
                    message.Body = multipart;
                    using MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient();
                    client.Connect(SmtpHost, SmtpPort, false);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(SendAccountName, AuthenticatePassword);
                    await client.SendAsync(message);
                    client.Disconnect(true);
                }
                catch (Exception e)
                {
                    throw new BusException("错误:" + e.Message);
                }
            }
        }

        #endregion MimeKit发送邮件
    }

    /// <summary>
    /// 发送邮件辅助类
    /// Author:李仕鹏
    /// </summary>
    public class SendMailHelper
    {
        /// <summary>
        /// SMTP邮件服务器字典
        /// </summary>
        public static Dictionary<string, string> emailHostDt = new Dictionary<string, string>
        {
            {"qq","smtp.qq.com"},{"163" ,"smtp.163.com"}
        };

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <param name="htmlMessage"></param>
        /// <returns></returns>
        public static async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            //设置邮件内容
            var mail = new MailMessage(
                new MailAddress("邮箱地址", "邮件发送方的名字"),
                new MailAddress(email)
                );
            mail.Subject = subject;
            mail.Body = htmlMessage;
            mail.IsBodyHtml = true;
            mail.BodyEncoding = Encoding.UTF8;
            mail.Priority = MailPriority.High;

            //设置SMTP服务器
            var smtp = new System.Net.Mail.SmtpClient("smtp.163.com", 25);
            smtp.UseDefaultCredentials = false;
            //开启身份验证
            //这个地方的账号是你的邮箱账号，密码是开启smtp功能时只展示一次的密码（要记住，不是你邮箱的登录密码）
            smtp.Credentials = new NetworkCredential("邮箱地址", "第三方验证密码");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            await smtp.SendMailAsync(mail);
        }

        #region 使用MialKit实现邮件发送功能
        
         /// <summary>
         /// 发送邮件
         /// </summary>
         /// <param name="name">发件人名字</param>
         /// <param name="receive">接收邮箱</param>
         /// <param name="sender">发送邮箱</param>
         /// <param name="password">邮箱密码</param>
         /// <param name="host">邮箱主机</param>
         /// <param name="port">邮箱端口</param>
         /// <param name="subject">邮件主题</param>
         /// <param name="body">邮件内容</param>
         /// <returns></returns>
        public static async Task<bool> SendMailAsync(string name, string receive, string sender, string password, string host, int port, string subject, string body)
        {
            try
            {
                //MimeMessage代表一封电子邮件的对象
                var message = new MimeMessage();
                //添加发件人地址 Name 发件人名字 sender 发件人邮箱
                message.From.Add(new MailboxAddress(name, sender));
                //添加收件人地址
                message.To.Add(new MailboxAddress("", receive));
                //设置邮件主题信息
                message.Subject = subject;
                //设置邮件内容
                var bodyBuilder = new BodyBuilder() { HtmlBody = body };
                message.Body = bodyBuilder.ToMessageBody();
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    // 出于演示目的，接受所有SSL证书(如果服务器支持STARTTLS)
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    // 注意:由于我们没有OAuth2令牌，禁用XOAUTH2认证机制
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.CheckCertificateRevocation = false;
                    //client.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    client.Connect(host, port, MailKit.Security.SecureSocketOptions.Auto);
                    // 注意:仅当SMTP服务器需要身份验证时才需要
                    client.Authenticate(sender, password);
                    await client.SendAsync(message);
                    client.Disconnect(true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("【SendMailHelper【SendMailAsync({0},{1},{2},{3},{4},{5},{6},{7})】】发送邮件发生异常：{8}",
                    name, receive, sender, password, host, port, subject, body, ex));
                return false;
            }
        }
        #endregion

        /// <summary>
        /// 根据实体类发送邮件
        /// </summary>
        /// <param name="mails"></param>
        public static async Task<bool> SendMailAsync(MailModel mails)
        {
            try
            {
                string[] sArray = mails.fromPerson.Split(new char[2] { '@', '.' });
                mails.host = emailHostDt[sArray[1]];
                //将发件人邮箱带入MailAddress中初始化
                MailAddress mailAddress = new MailAddress(mails.fromPerson);
                //创建Email的Message对象
                MailMessage mailMessage = new MailMessage();
                //判断收件人数组中是否有数据
                if (mails.recipientArry.Any())
                {
                    //循环添加收件人地址
                    foreach (var item in mails.recipientArry)
                    {
                        if (!string.IsNullOrEmpty(item))
                            mailMessage.To.Add(item.ToString());
                    }
                }
                //判断抄送地址数组是否有数据
                if (mails.mailCcArray.Any())
                {
                    //循环添加抄送地址
                    foreach (var item in mails.mailCcArray)
                    {
                        if (!string.IsNullOrEmpty(item))
                            mailMessage.To.Add(item.ToString());
                    }
                }
                //发件人邮箱
                mailMessage.From = mailAddress;
                //标题
                mailMessage.Subject = mails.mailTitle;
                //编码
                mailMessage.SubjectEncoding = Encoding.UTF8;
                //正文
                mailMessage.Body = mails.mailBody;
                //正文编码
                mailMessage.BodyEncoding = Encoding.Default;
                //邮件优先级
                mailMessage.Priority = MailPriority.High;
                //正文是否是html格式
                mailMessage.IsBodyHtml = mails.isbodyHtml;
                //取得Web根目录和内容根目录的物理路径
                string webRootPath = string.Empty;
                //实例化一个Smtp客户端
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
                //将发件人的邮件地址和客户端授权码带入以验证发件人身份
                smtp.Credentials = new NetworkCredential(mails.fromPerson, mails.code);
                //指定SMTP邮件服务器
                smtp.Host = mails.host;
                //邮件发送到SMTP服务器
                await smtp.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("【SendMailHelper【SendMailAsync({0})】】根据实体类发送邮件发生异常：{1}", mails, ex));
                return false;
            }
        }

    }

    /// <summary>
    /// 邮件模型
    /// </summary>
    public class MailModel
    {
        /// <summary>
        /// 发送人
        /// </summary>
        public string fromPerson { get; set; }

        /// <summary>
        /// 收件人地址(多人)
        /// </summary>
        public string[] recipientArry { get; set; }

        /// <summary>
        /// 抄送地址(多人)
        /// </summary>
        public string[] mailCcArray { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string mailTitle { get; set; }

        /// <summary>
        /// 正文
        /// </summary>
        public string mailBody { get; set; }

        /// <summary>
        /// 客户端授权码(可存在配置文件中)
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// SMTP邮件服务器
        /// </summary>
        public string host { get; set; }

        /// <summary>
        /// 正文是否是html格式
        /// </summary>
        public bool isbodyHtml { get; set; }
    }


}
