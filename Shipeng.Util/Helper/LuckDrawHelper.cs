namespace Shipeng.Util.Helper
{
    public static class LuckDrawHelper
    {
        /// <summary>
        /// 相同概率抽奖
        /// </summary>
        /// <param name="Length"> </param>
        /// <returns> </returns>
        public static int LuckDraw(int Length)
        {
            int count = (Length - 1) * 100;
            List<int> a = new List<int>();
            List<int> b = new List<int>();
            Random r = new Random();
            for (int i = 0; i < Length - 1; i++)
            {
                int sum = r.Next(0, Length - 1);
                a[i] = sum;
            }
            a = randomList(a);

            for (int i = 0; i < count - 1; i++)
            {
                int sum = r.Next(0, count - 1);
                b[i] = sum;
            }
            b = randomList(b);

            int target = r.Next(0, count - 1);

            while (b[target] >= Length)
            {
                target = r.Next(0, count - 1);
            }

            return b[target];
        }

        /// <summary>
        /// 随机排列数组
        /// </summary>
        /// <param name="a"> </param>
        private static List<int> randomList(List<int> a)
        {
            object[] b = new object[10];//保存a随机排序后的数据
            Random rand = new Random();
            List<int> list = new List<int>();
            for (int j = 0; j < 10; j++)
            {
                list.Add(a[j]);
            }
            //随机存入数据
            for (int i = 10; i > 0; i--)
            {
                int c = rand.Next(0, i);//产生随机数
                b[i - 1] = list[c];//随机选择一个数
                list.Remove(list[c]);//移除已经选择过的数
            }

            return list;
        }

        ///<summary>
        /// 红包算法
        /// </summary>
        /// <param name="_redPacket"></param>
        /// <returns></returns>
        public static List<double> GetMoneys(RedPacket _redPacket)
        {
            //人均最小金额
            double min = 0.1;
            if (_redPacket.remainMoney < _redPacket.remainCount * min)
            {
                return null;
            }

            int num = _redPacket.remainCount;
            List<double> array = new List<double>();
            Random r = new Random();
            for (int i = 0; i < num; i++)
            {
                if (_redPacket.remainCount == 1)
                {
                    _redPacket.remainCount--;
                    array.Add(Convert.ToDouble(_redPacket.remainMoney.ToString("0.0")));
                    Console.WriteLine(string.Format("第{0}个红包：{1}元", i + 1, Convert.ToDouble(_redPacket.remainMoney.ToString("0.0"))));
                }
                else
                {
                    //(_redPacket.remainMoney - (_redPacket.remainCount - 1) * min)：保存剩余金额可以足够的去分配剩余的红包数量
                    double max = (_redPacket.remainMoney - (_redPacket.remainCount - 1) * min) / _redPacket.remainCount * 2;
                    double money = r.NextDouble() * max;
                    money = Convert.ToDouble((money <= min ? min : money).ToString("0.0"));
                    _redPacket.remainCount--;
                    _redPacket.remainMoney -= money;
                    array.Add(money);
                    Console.WriteLine(string.Format("第{0}个红包：{1}元", i + 1, money));
                }
            }
            //再次随机
            return array.OrderBy(o => Guid.NewGuid()).ToList();
        }

        public class RedPacket
        {
            /// <summary>
            /// 剩余的红包数量
            /// </summary>
            public int remainCount { get; set; }

            /// <summary>
            /// 剩余的钱
            /// </summary>
            public double remainMoney { get; set; }
        }

        public static int LotteryDrawRendom(List<int> RendomArr)
        {
            int index = 0;
            RendomArr.Sort((x, y) => { return new Random().Next(-1, 1); });
            int val = RandomHelper.Next(0, RendomArr.Count * 100);

            index = RendomArr[val];

            return index;
        }
    }
}
