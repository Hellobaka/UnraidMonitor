using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class DiskInfo
    {
        public string MountPoint { get; set; } = "";

        public string Name { get; set; } = "";

        public string Size { get; set; } = "";

        private static Regex DiskRegex { get; } = new(@"^\s*([\w\-]+)\s+\d+:\d+\s+\d+\s+([\d.]+[TGMK]?)\s+\d+\s+\w+\s+(\S+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static List<DiskInfo> ParseDiskInfo(string input)
        {
            var disks = new List<DiskInfo>();
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
                    disks.Add(new DiskInfo
                    {
                        Name = match.Groups[1].Value.Trim(),
                        Size = match.Groups[2].Value.Trim(),
                        MountPoint = match.Groups[3].Value.Trim()
                    });
                }
            }

            return disks;
        }
    }
}