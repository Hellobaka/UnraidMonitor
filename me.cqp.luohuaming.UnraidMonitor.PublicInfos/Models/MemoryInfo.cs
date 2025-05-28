using System;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class MemoryInfo
    {
        public long Available { get; set; }

        public long BuffCache { get; set; }

        public long Free { get; set; }

        public long Total { get; set; }

        public double UsagePercentage => (Total - Available) / (double)Total * 100;

        public long Used { get; set; }

        public DateTime DateTime { get; set; }

        private static Regex MemoryRegex { get; } = new(@"Mem:\s*(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static MemoryInfo ParseFromFree(string input)
        {
            var info = new MemoryInfo();
            info.DateTime = DateTime.Now;
            var match = MemoryRegex.Match(input);

            if (match.Success)
            {
                info.Total = long.Parse(match.Groups[1].Value);
                info.Used = long.Parse(match.Groups[2].Value);
                info.Free = long.Parse(match.Groups[3].Value);
                // shared = long.Parse(match.Groups[4].Value);
                info.BuffCache = long.Parse(match.Groups[5].Value);
                info.Available = long.Parse(match.Groups[6].Value);
            }

            return info;
        }
    }
}