using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class DiskMountInfo
    {
        public string MountPoint { get; set; } = "";

        public string Name { get; set; } = "";

        public string Size { get; set; } = "";

        public long TotalBytes { get; set; }

        public long UsedBytes { get; set; }

        public long AvaliableBytes { get; set; }

        public double UsedPercent => TotalBytes == 0 ? 0 : Math.Round((double)UsedBytes / TotalBytes * 100, 2);

        private static Regex DiskRegex { get; } = new(@"^\s*([\w\-]+)\s+\d+:\d+\s+\d+\s+([\d.]+[TGMK]?)\s+\d+\s+\w+\s+(\S+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static List<DiskMountInfo> ParseDiskInfo(string input)
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
                        MountPoint = match.Groups[3].Value.Trim()
                    });
                }
            }

            return disks;
        }

        public void ParseDiskFree(string input)
        {
            if (string.IsNullOrWhiteSpace(MountPoint))
            {
                return;
            }
            foreach (var item in input.Split('\n'))
            {
                var splits = item.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                if (splits.Length >= 6 && splits[5] == MountPoint)
                {
                    AvaliableBytes = long.Parse(splits[3]);
                    UsedBytes = long.Parse(splits[2]);
                    TotalBytes = long.Parse(splits[1]);
                    break;
                }
            }
        }
    }
}