using System.Collections.Generic;

namespace Shipeng.Util
{
    public class DataEntity
    {
        public string name { get; set; }

        public List<DataEntity> child { get; set; }
    }

    //{IsEnableRobotSuccessful:是否添加加机器人让拼团成功，true会定时添加机器人让未拼团成功的数据拼团成功，false不开启;    IsEnableRobotGenerated:是否开启生成机器人拼团数据，true会默认生成机器人未拼团成功数据，false不开启;ExpireTime 团购默认拼团过期时间，单位是小时}
    public class W_GroupBuyingSetting
    {
        /// <summary>
        /// 是否添加机器人让拼团成功，true会定时添加机器人让未拼团成功的数据拼团成功，false不开启
        /// </summary>
        public bool IsEnableRobotSuccessful { get; set; }

        /// <summary>
        /// 是否开启生成机器人拼团数据，true会默认生成机器人未拼团成功数据，false不开启
        /// </summary>
        public bool IsEnableRobotGenerated { get; set; }

        /// <summary>
        /// 团购默认拼团过期时间，单位是小时
        /// </summary>
        public int ExpireTime { get; set; }
    }
}
