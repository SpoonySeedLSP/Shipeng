using JPushSDK.Jiguang.JPush;
using JPushSDK.Jiguang.JPush.Model;

namespace Shipeng.Util.Helper
{
    public static class JPushHelper
    {
        /// <summary>
        /// 极光推送
        /// </summary>
        /// <param name="type">0 全部推送  1 指定设备推送（1000个以内） 2 根据角色推送</param>
        /// <param name="appId"></param>
        /// <param name="masterSecret"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="registrationId">设备id ["170976fa8a2ea2842ed"]</param>
        /// <param name="tags">角色 ["",""]</param>
        /// <param name="extras">extras 附加参数  "extras": {"pages": "/pages/my/members/index","param": "213112123"}</param>
        public static bool Send(int type, string appId, string masterSecret, string title, string content, List<string> registrationId = null, List<string> tags = null, Dictionary<string, object> extras = null)
        {
            //string app_key = "18dac586246dfed7592f2f5c";
            //string master_secret = "b243966ca859d82081e2195f";
            //new List<string> { "170976fa8a2ea2842ed" }

            HttpClient httpClient = new HttpClient();
            JPushClient client = new JPushClient(httpClient, appId, masterSecret);
            switch (type)
            {
                case 0:
                    return ExecutePushByALL(client, title, content, extras);
                case 1:
                    return ExecutePushByUserId(client, registrationId, title, content, extras);
                case 2:
                    return ExecutePustByTags(client, tags, title, content, extras);
                default:
                    return true;
            }
        }

        /// <summary>
        /// 根据Tags推送(根据角色推送)
        /// </summary>
        /// <param name="client"></param>
        /// <param name="tags"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="Extras"></param>
        public static bool ExecutePustByTags(JPushClient client, List<string> tags, string title, string content, Dictionary<string, object> Extras)
        {
            //audience : { "tag" : [ "tag1", "tag2" ]} 
            TagsAudienceData audienceData = new TagsAudienceData
            {
                tag = tags.ToArray()
            };
            PushPayload pushPayload = new PushPayload()
            {
                Platform = "all",
                Audience = audienceData,
                Notification = new Notification
                {
                    Android = new Android
                    {
                        Alert = content,
                        Title = title,
                        Extras = Extras
                    },
                    IOS = new IOS
                    {
                        Alert = content,
                        Badge = title,
                        Extras = Extras
                    },
                },
                Message = new Message
                {
                    Title = title,
                    Content = content,
                    Extras = Extras
                },
                Options = new Options
                {
                    TimeToLive = 864000//单位秒,最大值10天
                }

            };
            HttpResponse response = client.SendPush(pushPayload);
            if (response.StatusCode.ToString() != "200")
            {
                LogHelper.WriteLog_LocalJPushTxt($"{DateTime.Now}" + pushPayload.ToJson());
                LogHelper.WriteLog_LocalJPushTxt($"{DateTime.Now}" + response.ToJson());
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 推送所有人
        /// </summary>
        /// <param name="client"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="Extras"></param>
        public static bool ExecutePushByALL(JPushClient client, string title, string content, Dictionary<string, object> Extras)
        {
            PushPayload pushPayload = new PushPayload()
            {
                Platform = "all",
                Audience = "all",
                Notification = new Notification
                {
                    Android = new Android
                    {
                        Alert = content,
                        Title = title,
                        Extras = Extras
                    },
                    IOS = new IOS
                    {
                        Alert = content,
                        Badge = title,
                        Extras = Extras
                    },
                },
                Message = new Message
                {
                    Title = title,
                    Content = content,
                    Extras = Extras
                },
                Options = new Options
                {
                    TimeToLive = 864000//单位秒,最大值10天
                }
            };
            HttpResponse response = client.SendPush(pushPayload);
            if (response.StatusCode.ToString() != "200")
            {
                LogHelper.WriteLog_LocalJPushTxt($"{DateTime.Now}" + pushPayload.ToJson());
                LogHelper.WriteLog_LocalJPushTxt($"{DateTime.Now}" + response.ToJson());
                return false;
            }
            return true;
        }

        /// <summary>
        /// 推送到个人,每次推送最多1000人
        /// </summary>
        /// <param name="client"></param>
        /// <param name="registration_id"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="Extras"></param>
        public static bool ExecutePushByUserId(JPushClient client, List<string> registration_id, string title, string content, Dictionary<string, object> Extras)
        {
            //audience: { "registration_id" : [ "111", "22" ]}
            //    设备标识。一次推送最多 1000 个。
            RegistrationAudienceData data = new RegistrationAudienceData
            {
                registration_id = registration_id.ToArray()
            };
            PushPayload pushPayload = new PushPayload()
            {
                Platform = "all",
                Audience = data,
                Notification = new Notification
                {
                    Android = new Android
                    {
                        Alert = content,
                        Title = title,
                        Extras = Extras
                    },
                    //IOS = new IOS
                    //{
                    //    Alert = content,
                    //    Badge = title,
                    //    Extras = Extras
                    //},
                },
                Message = new Message
                {
                    Title = title,
                    Content = content,
                    Extras = Extras
                },
                Options = new Options
                {
                    TimeToLive = 864000//单位秒,最大值10天
                }
            };
            HttpResponse response = client.SendPush(pushPayload);
            //{"StatusCode":400,"Headers":[{"Key":"Date","Value":["Mon, 21 Jun 2021 02:02:12 GMT"]},{"Key":"Connection","Value":["keep-alive"]},{"Key":"X-Rate-Limit-Limit","Value":["600"]},{"Key":"X-Rate-Limit-Remaining","Value":["599"]},{"Key":"X-Rate-Limit-Reset","Value":["60"]},{"Key":"Server","Value":["elb"]}],"Content":"{\"error\":{\"code\":1003,\"message\":\"Empty registration_id is not allowed!\"}}"}
            if (response.StatusCode.ToString() != "200")
            {
                LogHelper.WriteLog_LocalJPushTxt($"{DateTime.Now}" + pushPayload.ToJson());
                LogHelper.WriteLog_LocalJPushTxt($"{DateTime.Now}" + response.ToJson());
                return false;
            }
            return true;
        }



        public class TagsAudienceData
        {
            public string[] tag { get; set; }
        }

        public class RegistrationAudienceData
        {
            public string[] registration_id { get; set; }
        }
    }
}
