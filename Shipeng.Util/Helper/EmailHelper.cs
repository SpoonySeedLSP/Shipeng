using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shipeng.Util
{
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
                    using SmtpClient client = new SmtpClient();
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
}
