using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class DiskMountInfo : MonitorDataBase
    {
        [Description("挂载点")]
        public string MountPoint { get; set; } = "";

        [Description("磁盘挂载类型 (Linux) / 文件系统 (Windows)")]
        public string Type { get; set; } = "";

        [Description("助记名称 (Linux) / 盘符 (Windows)")]
        public string Name { get; set; } = "";

        [Description("总容量 (KB)")]
        public string Size { get; set; } = "";

        [Description("助记总容量")]
        public long TotalBytes { get; set; }

        [Description("总容量 (KB)")]
        public long UsedBytes { get; set; }

        [Description("可用容量 (KB)")]
        public long AvailableBytes { get; set; }

        [Description("容量使用率 (%)")]
        public double UsedPercent => TotalBytes == 0 ? 0 : Math.Round((double)UsedBytes / TotalBytes * 100, 2);

        private static Regex DiskRegex { get; } = new(@"^\s*([\w\-]+)\s+\d+:\d+\s+\d+\s+([\d.]+[TGMK]?)\s+\d+\s+(\w+)\s+(.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static DiskMountInfo[] ParseFromLsblk(string input)
        {
            var disks = new List<DiskMountInfo>();
            var regex = DiskRegex;

            foreach (var line in input.Split('\n'))
            {
                // 跳过表头和空行
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("NAME"))
                {
                    continue;
                }

                var match = regex.Match(line);
                if (match.Success)
                {
                    disks.Add(new DiskMountInfo
                    {
                        Name = match.Groups[1].Value.Trim(),
                        Size = match.Groups[2].Value.Trim(),
                        Type = match.Groups[3].Value.Trim(),
                        MountPoint = match.Groups[4].Value.Trim(),
                        DateTime = DateTime.Now
                    });
                }
            }

            return disks.ToArray();
        }

        public void ParseDiskFree(string input, string name)
        {
            foreach (var item in input.Split('\n'))
            {
                var splits = item.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                if (splits.Length >= 6 && splits[0] == name)
                {
                    AvailableBytes = long.Parse(splits[3]);
                    UsedBytes = long.Parse(splits[2]);
                    TotalBytes = long.Parse(splits[1]);
                    break;
                }
            }
        }
    }
}