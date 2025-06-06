using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models;
using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.EventArgs;
using Newtonsoft.Json;
using PropertyChanged;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos
{
    public interface IOrderModel
    {
        bool ImplementFlag { get; set; }

        /// <summary>
        /// 优先级，越高越优先处理
        /// </summary>
        int Priority { get; set; }

        string GetCommand();

        bool CanExecute(string destStr);

        FunctionResult Execute(CQGroupMessageEventArgs e);

        FunctionResult Execute(CQPrivateMessageEventArgs e);
    }

    public class Commands
    {
        public string Command { get; set; }

        public string StylePath { get; set; }

        [JsonIgnore]
        public DrawingStyle DrawingStyle => DrawingStyle.LoadFromFile(StylePath);
    }
}