using ServiceStack.Redis;

namespace Shipeng.Util.Cache
{
    /// <summary>
    ///  Redis list的实现为一个双向链表，即可以支持反向查找和遍历，
    ///  更方便操作，不过带来了部分额外的内存开销，
    ///  Redis内部的很多实现，包括发送缓冲队列等也都是用的这个数据结构
    ///  Author:李仕鹏
    /// </summary>
    public class RedisListService : RedisBase
    {
        #region 赋值
        /// <summary>
        /// 从左侧向list中添加值
        /// </summary>
        public void LPush(string key, string value)
        {
            IClient.PushItemToList(key, value);
        }

        /// <summary>
        /// 从左侧向list中添加值，并设置过期时间
        /// </summary>
        public void LPush(string key, string value, DateTime dt)
        {

            IClient.PushItemToList(key, value);
            IClient.ExpireEntryAt(key, dt);
        }

        /// <summary>
        /// 从左侧向list中添加值，设置过期时间
        /// </summary>
        public void LPush(string key, string value, TimeSpan sp)
        {
            IClient.PushItemToList(key, value);
            IClient.ExpireEntryIn(key, sp);
        }

        /// <summary>
        /// 从右侧向list中添加值
        /// </summary>
        public void RPush(string key, string value)
        {
            IClient.PrependItemToList(key, value);
        }
    
        /// <summary>
        /// 从右侧向list中添加值，并设置过期时间
        /// </summary>   
        public void RPush(string key, string value, DateTime dt)
        {
            IClient.PrependItemToList(key, value);
            IClient.ExpireEntryAt(key, dt);
        }

        /// <summary>
        /// 从右侧向list中添加值，并设置过期时间
        /// </summary>       
        public void RPush(string key, string value, TimeSpan sp)
        {
            IClient.PrependItemToList(key, value);
            IClient.ExpireEntryIn(key, sp);
        }
   
        /// <summary>
        /// 添加key/value
        /// </summary>    
        public void Add(string key, string value)
        {
            IClient.AddItemToList(key, value);
        }
    
        /// <summary>
        /// 添加key/value ,并设置过期时间
        /// </summary> 
        public void Add(string key, string value, DateTime dt)
        {
            IClient.AddItemToList(key, value);
            IClient.ExpireEntryAt(key, dt);
        }
       
        /// <summary>
        /// 添加key/value。并添加过期时间
        /// </summary> 
        public void Add(string key, string value, TimeSpan sp)
        {
            IClient.AddItemToList(key, value);
            IClient.ExpireEntryIn(key, sp);
        }
    
        /// <summary>
        /// 为key添加多个值
        /// </summary> 
        public void Add(string key, List<string> values)
        {
            IClient.AddRangeToList(key, values);
        }
   
        /// <summary>
        /// 为key添加多个值，并设置过期时间
        /// </summary> 
        public void Add(string key, List<string> values, DateTime dt)
        {
            IClient.AddRangeToList(key, values);
            IClient.ExpireEntryAt(key, dt);
        }
     
        /// <summary>
        /// 为key添加多个值，并设置过期时间
        /// </summary> 
        public void Add(string key, List<string> values, TimeSpan sp)
        {
            IClient.AddRangeToList(key, values);
            IClient.ExpireEntryIn(key, sp);
        }
        #endregion

        #region 获取值
        /// <summary>
        /// 获取list中key包含的数据数量
        /// </summary> 
        public long Count(string key)
        {
            return IClient.GetListCount(key);
        }
      
        /// <summary>
        /// 获取key包含的所有数据集合
        /// </summary> 
        public List<string> Get(string key)
        {
            return IClient.GetAllItemsFromList(key);
        }
      
        /// <summary>
        /// 获取key中下标为star到end的值集合
        /// </summary> 
        public List<string> Get(string key, int star, int end)
        {
            return IClient.GetRangeFromList(key, star, end);
        }
        #endregion

        #region 阻塞命令
        /// <summary>
        ///  阻塞命令：从list为key的尾部移除一个值，并返回移除的值，阻塞时间为sp
        /// </summary> 
        public string BlockingPopItemFromList(string key, TimeSpan? sp)
        {
            return IClient.BlockingPopItemFromList(key, sp);
        }
        
        /// <summary>
        ///  阻塞命令：从多个list中尾部移除一个值,并返回移除的值&key，阻塞时间为sp
        /// </summary> 
        public ItemRef BlockingPopItemFromLists(string[] keys, TimeSpan? sp)
        {
            return IClient.BlockingPopItemFromLists(keys, sp);
        }

        /// <summary>
        ///  阻塞命令：从list中keys的尾部移除一个值，并返回移除的值，阻塞时间为sp
        /// </summary> 
        public string BlockingDequeueItemFromList(string key, TimeSpan? sp)
        {
            return IClient.BlockingDequeueItemFromList(key, sp);
        }

        /// <summary>
        /// 阻塞命令：从多个list中尾部移除一个值，并返回移除的值&key，阻塞时间为sp
        /// </summary> 
        public ItemRef BlockingDequeueItemFromLists(string[] keys, TimeSpan? sp)
        {
            return IClient.BlockingDequeueItemFromLists(keys, sp);
        }

        /// <summary>
        /// 阻塞命令：从list中一个fromkey的尾部移除一个值，添加到另外一个tokey的头部，并返回移除的值，阻塞时间为sp
        /// </summary> 
        public string BlockingPopAndPushItemBetweenLists(string fromkey, string tokey, TimeSpan? sp)
        {
            return IClient.BlockingPopAndPushItemBetweenLists(fromkey, tokey, sp);
        }
        #endregion

        #region 删除
        /// <summary>
        /// 从尾部移除数据，返回移除的数据
        /// </summary> 
        public string PopItemFromList(string key)
        {
            var sa = IClient.CreateSubscription();
            return IClient.PopItemFromList(key);
        }
        
        /// <summary>
        /// 从尾部移除数据，返回移除的数据
        /// </summary> 
        public string DequeueItemFromList(string key)
        {
            return IClient.DequeueItemFromList(key);
        }

        /// <summary>
        /// 移除list中，key/value,与参数相同的值，并返回移除的数量
        /// </summary> 
        public long RemoveItemFromList(string key, string value)
        {
            return IClient.RemoveItemFromList(key, value);
        }
     
        /// <summary>
        /// 从list的尾部移除一个数据，返回移除的数据
        /// </summary> 
        public string RemoveEndFromList(string key)
        {
            return IClient.RemoveEndFromList(key);
        }
     
        /// <summary>
        /// 从list的头部移除一个数据，返回移除的值
        /// </summary> 
        public string RemoveStartFromList(string key)
        {
            return IClient.RemoveStartFromList(key);
        }
        #endregion

        #region 其它
        /// <summary>
        /// 从一个list的尾部移除一个数据，添加到另外一个list的头部，并返回移动的值
        /// </summary> 
        public string PopAndPushItemBetweenLists(string fromKey, string toKey)
        {
            return IClient.PopAndPushItemBetweenLists(fromKey, toKey);
        }

        public void TrimList(string key, int start, int end)
        {
            IClient.TrimList(key, start, end);
        }
        #endregion

        #region 发布订阅
        public void Publish(string channel, string message)
        {
            IClient.PublishMessage(channel, message);
        }

        public void Subscribe(string channel, Action<string, string, IRedisSubscription> actionOnMessage)
        {
            var subscription = IClient.CreateSubscription();
            subscription.OnSubscribe = c =>
            {
                Console.WriteLine($"订阅频道{c}");
                Console.WriteLine();
            };
            //取消订阅
            subscription.OnUnSubscribe = c =>
            {
                Console.WriteLine($"取消订阅 {c}");
                Console.WriteLine();
            };
            subscription.OnMessage += (c, s) =>
            {
                actionOnMessage(c, s, subscription);
            };
            Console.WriteLine($"开始启动监听 {channel}");
            subscription.SubscribeToChannels(channel); //blocking
        }
       
        public void UnSubscribeFromChannels(string channel)
        {
            var subscription = IClient.CreateSubscription();
            subscription.UnSubscribeFromChannels(channel);
        }
        #endregion
    }
}
