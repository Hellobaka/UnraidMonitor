using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Handler;
using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp;
using System.Collections.Generic;
using System.IO;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos
{
    public static class MainSave
    {
        /// <summary>
        /// 保存各种事件的数组
        /// </summary>
        public static List<IOrderModel> Instances { get; set; } = new List<IOrderModel>();
        public static CQLog CQLog { get; set; }
        public static CQApi CQApi { get; set; }
        public static string AppDirectory { get; set; }
        public static string ImageDirectory { get; set; }
        public static string UnraidMonitorImageSavePath => Path.Combine(ImageDirectory, "UnraidMonitor");

        public static HandlerBase MonitorAPI { get; set; }

        public static List<Commands> Commands { get; set; }
    }
}
