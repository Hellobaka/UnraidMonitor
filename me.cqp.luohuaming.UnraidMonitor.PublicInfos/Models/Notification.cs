using System;
using System.ComponentModel;
using System.Linq;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class Notification : MonitorDataBase
    {
        [Description("事件级别信息")]
        public string Event { get; set; } = "";

        [Description("消息级别信息")]
        public string Message { get; set; } = "";

        [Description("提示级别信息")]
        public string Subject { get; set; } = "";

        [Description("重要级别信息")]
        public string Importance { get; set; } = "";

        public static Notification ParseFromUnreadFile(string input)
        {
            var lines = input.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var notification = new Notification();
            notification.DateTime = DateTime.Now;
            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length != 2)
                {
                    continue;
                }
                var key = parts.FirstOrDefault()?.Trim();
                var value = parts.Last()?.Trim();
                if (key.Equals("event", StringComparison.OrdinalIgnoreCase))
                {
                    notification.Event = value;
                }
                else if (key.Equals("subject", StringComparison.OrdinalIgnoreCase))
                {
                    notification.Subject = value;
                }
                else if (key.Equals("description", StringComparison.OrdinalIgnoreCase))
                {
                    notification.Message = value;
                }
                else if (key.Equals("importance", StringComparison.OrdinalIgnoreCase))
                {
                    notification.Importance = value;
                }
                else if (key.Equals("timestamp", StringComparison.OrdinalIgnoreCase)
                    && long.TryParse(value, out long l))
                {
                    notification.DateTime = CommonHelper.ParseTimeStampSToDateTime(l);
                }
            }

            return notification;
        }
    }
}
