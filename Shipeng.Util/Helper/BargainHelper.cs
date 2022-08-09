using System;

namespace Shipeng.Util
{
    /// <summary>
    /// 砍价
    /// </summary>
    public static class BargainHelper
    {
        /// <summary>
        /// 最小砍价金额
        /// </summary>
        private static readonly int minmoney = 10;

        /// <summary>
        /// 最大砍价金额
        /// </summary>
        private static readonly int maxmoney = 10 * 10;

        /// <summary>
        /// 最后一次砍价金额
        /// </summary>
        private static readonly double times = 3.1;

        /// <summary>
        ///砍价合法性校验
        ///控制金额过小或者过大
        /// </summary>
        /// <param name="money"></param>
        /// <param name="surplusKnife"></param>
        /// <returns></returns>
        public static bool isRight(int money, int surplusKnife)
        {
            double avg = money / surplusKnife;
            //小于最小金额
            if (avg < minmoney)
            {
                return false;
            }
            else if (avg > maxmoney)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 随机分配一个金额
        /// </summary>
        /// <param name="money"> 砍价金额 </param>
        /// <param name="minS"> 最小金额 </param>
        /// <param name="maxS"> 最大金额 </param>
        /// <param name="surplusKnife"> </param>
        /// <returns> </returns>
        public static int randomReducePrice(int money, int minS, int maxS, int surplusKnife)
        {
            //若只剩一次，则直接返回
            if (surplusKnife == 1)
            {
                return money;
            }
            //如果最大金额和最小金额相等，直接返回金额
            if (minS == maxS)
            {
                return minS;
            }
            int max = maxS > money ? money : maxS;
            //分配砍价正确情况，允许砍价的最大值
            int maxY = money - (surplusKnife - 1) * minS;
            //分配砍价正确情况，允许砍价最小值
            int minY = money - (surplusKnife - 1) * maxS;
            //随机产生砍价的最小值
            int min = minS > minY ? minS : minY;
            //随机产生砍价的最大值
            max = max > maxY ? maxY : max;
            //随机产生一个砍价
            return RandomHelper.Next(min, max);
        }

        public static int splitReducePrice(int money, int surplusKnife)
        {
            //金额合法性分析
            if (!isRight(money, surplusKnife))
            {
                return 0;
            }
            //每次砍价的最大的金额为平均金额的TIMES倍
            int max = (int)(money * times / surplusKnife);
            max = max > maxmoney ? maxmoney : max;
            int one = randomReducePrice(money, minmoney, max, surplusKnife);
            return one;
        }

        /**
     * 预砍价----获取一个随机金额
     * @param surplusMoney 当前待砍金额
     * @param surplusKnife 当前待砍刀数
     * */

        public static int bargain(int surplusMoney, int surplusKnife)
        {
            //预算这次砍掉的钱
            int bargainMoney = 0;
            try
            {
                bargainMoney = splitReducePrice(surplusMoney, surplusKnife);
                if (surplusMoney < bargainMoney)
                {
                    bargainMoney = surplusMoney;
                }
                return bargainMoney;
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }

    public class HdUser
    {
        public string Name { get; set; }
        public string remark { get; set; }
    }
}
