using System.Collections.Generic;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class TemperatureInfo
    {
        public string ParentName { get; set; } = "";

        public string Name { get; set; } = "";

        public double Temperature { get; set; }

        public static TemperatureInfo[] ParseFromSensor(string input)
        {
            List<TemperatureInfo> list = new();
            string parent = "";
            foreach (var item in input.Split('\n'))
            {
                if (!string.IsNullOrWhiteSpace(item) && !item.Contains(":"))
                {
                    parent = item.Trim();
                }

                if (item.Contains("°C") || item.Contains("°F"))
                {
                    var name = item.Split(':')[0].Trim();
                    var temp = item.Split(':')[1].Trim().Split(' ')[0];
                    list.Add(new TemperatureInfo()
                    {
                        ParentName = parent,
                        Name = name,
                        Temperature = double.TryParse(temp, out double value) ? value : -1
                    });
                }
            }
            return list.ToArray();
        }
    }
}
