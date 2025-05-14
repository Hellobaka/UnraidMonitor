using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class TemperatureInfo
    {
        public string ParentName { get; set; } = "";

        public string Name { get; set; } = "";

        public double Temperature { get; set; }

        public DateTime DateTime { get; set; }

        private static Regex TemperatureRegex { get; } = new Regex(@"^([A-Za-z0-9 _\-]+):?\s*([+-]?\d+\.\d)\s*C", RegexOptions.Compiled | RegexOptions.Singleline);

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

                if (TemperatureRegex.IsMatch(item))
                {
                    var match = TemperatureRegex.Match(item);
                    list.Add(new TemperatureInfo()
                    {
                        ParentName = parent,
                        Name = match.Groups[1].Value.Trim(),
                        Temperature = double.TryParse(match.Groups[2].Value.Trim(), out double value) ? value : -1
                    });
                }
            }
            return list.ToArray();
        }
    }
}
