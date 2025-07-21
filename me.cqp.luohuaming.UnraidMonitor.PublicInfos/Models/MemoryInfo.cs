using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class MemoryInfo : MonitorDataBase
    {
        [Description("可用容量 (KB)")]
        public long Available { get; set; }

        [Description("Buffer 容量 (KB)")]
        public long BuffCache { get; set; }

        [Description("空闲容量 (KB)")]
        public long Free { get; set; }

        [Description("总容量 (KB)")]
        public long Total { get; set; }

        [Description("内存占用率 (%)")]
        public double UsagePercentage => (Total - Available) / (double)Total * 100;

        [Description("已使用容量 (KB)")]
        public long Used { get; set; }

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