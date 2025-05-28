using System;
using System.Linq;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class Notification
    {
        public string Event { get; set; } = "";

        public string Message { get; set; } = "";

        public string Subject { get; set; } = "";

        public string Importance { get; set; } = "";

        public DateTime DateTime { get; set; }

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
