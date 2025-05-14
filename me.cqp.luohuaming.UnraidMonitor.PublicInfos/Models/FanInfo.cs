using System;
using System.Collections.Generic;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class FanInfo
    {
        public string ParentName { get; set; } = "";

        public string Name { get; set; } = "";

        public int RPM { get; set; }

        public DateTime DateTime { get; set; }

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
                        RPM = int.TryParse(rpm, out int value) ? value : -1
                    });
                }
            }
            return list.ToArray();
        }
    }
}
