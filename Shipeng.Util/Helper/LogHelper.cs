using System;
using System.IO;
using System.Threading.Tasks;

namespace Shipeng.Util
{
    /// <summary>
    /// 日志帮助类
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// 写入日志到本地TXT文件 注：日志文件名为"A_log.txt",目录为根目录
        /// </summary>
        /// <param name="log"> 日志内容 </param>
        public static void WriteLog_HearTxt(string log)
        {
            DateTime nowdate = DateTime.Now;
            Task.Run(() =>
            {
                try
                {
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "Hearder" + nowdate.ToString("yyyy-MM-dd") + "_log.txt");
                    string logContent = $"{DateTime.Now.ToCstTime().ToString("yyyy-MM-dd HH:mm:ss:ffff")} :{log}\r\n\r\n"; ;
                    File.AppendAllText(filePath, logContent);
                }
                catch (Exception)
                {
                }
            });
        }

        /// <summary>
        /// 写入日志到本地TXT文件 注：日志文件名为"A_log.txt",目录为根目录
        /// </summary>
        /// <param name="log"> 日志内容 </param>
        public static void WriteLog_LocalTxt(string log)
        {
            DateTime nowdate = DateTime.Now;
            Task.Run(() =>
            {
                try
                {
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "WeChatMessage" + nowdate.ToString("yyyy-MM-dd") + "_log.txt");
                    string logContent = $"{DateTime.Now.ToCstTime().ToString("yyyy-MM-dd HH:mm:ss:ffff")} :{log}\r\n\r\n"; ;
                    File.AppendAllText(filePath, logContent);
                }
                catch (Exception)
                {
                }
            });
        }

        public static void WriteLog_LocalJPushTxt(string log)
        {
            DateTime nowdate = DateTime.Now;
            Task.Run(() =>
            {
                try
                {
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "JPush" + nowdate.ToString("yyyy-MM-dd") + "_log.txt");
                    string logContent = $"{DateTime.Now.ToCstTime().ToString("yyyy-MM-dd HH:mm:ss:ffff")} :{log}\r\n\r\n";
                    File.AppendAllText(filePath, logContent);
                }
                catch (Exception)
                {
                }
            });
        }

        public static void WriteLog_LocalSocketTxt(string log)
        {
            DateTime nowdate = DateTime.Now;
            Task.Run(() =>
            {
                try
                {
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "Socket" + nowdate.ToString("yyyy-MM-dd") + "_log.txt");
                    string logContent = $"{DateTime.Now.ToCstTime().ToString("yyyy-MM-dd HH:mm:ss:ffff")} :{log}\r\n\r\n"; ;
                    File.AppendAllText(filePath, logContent);
                }
                catch (Exception)
                {
                }
            });
        }

        public static void WriteLog_WeChartTxtB(string log)
        {
            DateTime nowdate = DateTime.Now;
            Task.Run(() =>
            {
                try
                {
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "WeChartPay" + nowdate.ToString("yyyy-MM-dd") + "_log.txt");
                    string logContent = $"{DateTime.Now.ToCstTime().ToString("yyyy-MM-dd HH:mm:ss:ffff")} :{log}\r\n\r\n"; ;
                    File.AppendAllText(filePath, logContent);
                }
                catch (Exception)
                {
                }
            });
        }

        /// <summary>
        /// 定时器日志
        /// </summary>
        /// <param name="log"></param>
        public static void WriteLog_TimerTaskTxt(string log)
        {
            DateTime nowdate = DateTime.Now;
            Task.Run(() =>
            {
                try
                {
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "WriteLog_TimerTaskTxt" + nowdate.ToString("yyyy-MM-dd") + "_log.txt");
                    string logContent = $"{DateTime.Now.ToCstTime().ToString("yyyy-MM-dd HH:mm:ss:ffff")} :{log}\r\n\r\n"; ;
                    File.AppendAllText(filePath, logContent);
                }
                catch (Exception)
                {
                }
            });
        }

        /// <summary>
        /// 小程序红包
        /// </summary>
        /// <param name="log"></param>
        public static void WriteLog_WeChartSendredpackTxt(string log)
        {
            DateTime nowdate = DateTime.Now;
            Task.Run(() =>
            {
                try
                {
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "WeChartSendredpack" + nowdate.ToString("yyyy-MM-dd") + "_log.txt");
                    string logContent = $"{DateTime.Now.ToCstTime().ToString("yyyy-MM-dd HH:mm:ss:ffff")} :{log}\r\n\r\n"; ;
                    File.AppendAllText(filePath, logContent);
                }
                catch (Exception)
                {
                }
            });
        }

        public static void WriteLog_WeChartsendminiprogramhbTxt(string log)
        {
            DateTime nowdate = DateTime.Now;
            Task.Run(() =>
            {
                try
                {
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "WeChartPaysendminiprogramhb" + nowdate.ToString("yyyy-MM-dd") + "_log.txt");
                    string logContent = $"{DateTime.Now.ToCstTime().ToString("yyyy-MM-dd HH:mm:ss:ffff")} :{log}\r\n\r\n"; ;
                    File.AppendAllText(filePath, logContent);
                }
                catch (Exception)
                {
                }
            });
        }

        public static void WriteLog_CourtCaseAppTxt(string log)
        {
            DateTime nowdate = DateTime.Now;
            Task.Run(() =>
            {
                try
                {
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "CourtCaseApp" + nowdate.ToString("yyyy-MM-dd") + "_log.txt");
                    string logContent = $"{DateTime.Now.ToCstTime().ToString("yyyy-MM-dd HH:mm:ss:ffff")} :{log}\r\n\r\n"; ;
                    File.AppendAllText(filePath, logContent);
                }
                catch (Exception)
                {
                }
            });
        }

        public static void WriteLog_WechatAppTxt(string log)
        {
            DateTime nowdate = DateTime.Now;
            Task.Run(() =>
            {
                try
                {
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "WeChatApp" + nowdate.ToString("yyyy-MM-dd") + "_log.txt");
                    string logContent = $"{DateTime.Now.ToCstTime().ToString("yyyy-MM-dd HH:mm:ss:ffff")} :{log}\r\n\r\n"; ;
                    File.AppendAllText(filePath, logContent);
                }
                catch (Exception)
                {
                }
            });
        }

        public static void WriteLog_DingxinbaoTxt(string log)
        {
            DateTime nowdate = DateTime.Now;
            Task.Run(() =>
            {
                try
                {
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "Dingxinbao" + nowdate.ToString("yyyy-MM-dd") + "_log.txt");
                    string logContent = $"{DateTime.Now.ToCstTime().ToString("yyyy-MM-dd HH:mm:ss:ffff")} :{log}\r\n\r\n"; ;
                    File.AppendAllText(filePath, logContent);
                }
                catch (Exception)
                {
                }
            });
        }

        /// <summary>
        /// 微信客服消息
        /// </summary>
        /// <param name="log"></param>
        public static void WriteLog_CustomerServiceMessageTxt(string log)
        {
            DateTime nowdate = DateTime.Now;
            Task.Run(() =>
            {
                try
                {
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "CustomerServiceMessage" + nowdate.ToString("yyyy-MM-dd") + "_log.txt");
                    string logContent = $"{DateTime.Now.ToCstTime().ToString("yyyy-MM-dd HH:mm:ss:ffff")} :{log}\r\n\r\n"; ;
                    File.AppendAllText(filePath, logContent);
                }
                catch (Exception)
                {
                }
            });
        }
    }
}