using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class FanInfo : MonitorDataBase
    {
        [Description("传感器组")]
        public string ParentName { get; set; } = "";

        [Description("风扇名称")]
        public string Name { get; set; } = "";

        [Description("风扇转速 (RPM)")]
        public int RPM { get; set; }

        public static FanInfo[] ParseFromSensor(string input)
        {
            List<FanInfo> list = [];
            string parent = "";
            foreach (var item in input.Split('\n'))
            {
                if (!string.IsNullOrWhiteSpace(item) && !item.Contains(":"))
                {
                    parent = item.Trim();
                }
                if (item.Contains("RPM"))
                {
                    var name = item.Split(':')[0].Trim();
                    var rpm = item.Split(':')[1].Trim().Split(' ')[0];
                    list.Add(new FanInfo()
                    {
                        ParentName = parent,
                        Name = name,
                        RPM = int.TryParse(rpm, out int value) ? value : -1,
                        DateTime = DateTime.Now
                    });
                }
            }
            return list.ToArray();
        }
    }
}
