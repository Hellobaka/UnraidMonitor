using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class DiskInfo : MonitorDataBase
    {
        public enum DiskType
        {
            Flash,

            Data,

            Cache,

            Parity,

            Unknown
        }

        [Description("磁盘模式 (Unraid)")]
        public DiskType Type { get; set; } = DiskType.Data;

        [Description("助记名称")]
        public string RemarkName { get; set; } = "";

        [Description("挂载名称")]
        public string DeviceName { get; set; } = "";

        [Description("磁盘型号")]
        public string Model { get; set; } = "";

        [Description("电源状态")]
        public string PowerMode { get; set; } = "";

        [Description("总容量 (KB)")]
        public long Total { get; set; }

        [Description("使用容量 (KB)")]
        public long Used { get; set; }

        [Description("剩余容量 (KB)")]
        public long Free { get; set; }

        [Description("温度 (°C)")]
        public double Temperature { get; set; }

        [Description("文件系统")]
        public string FileSystem { get; set; } = "";

        [Description("运行状态")]
        public bool Running => PowerMode.Contains("-on");

        [Description("容量使用率 (%)")]
        public double UsedPercent => Total == 0 ? 0 : Math.Round((double)Used / Total * 100, 2);

        public static DiskInfo[] ParseFromDiskIni(string input)
        {
            var disks = new List<DiskInfo>();
            var lines = input.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
            string currentGroup = "";
            DiskInfo currentDiskInfo = null;
            foreach (var line in lines)
            {
                if (line.StartsWith("["))
                {
                    if (currentDiskInfo != null)
                    {
                        disks.Add(currentDiskInfo);
                    }

                    currentGroup = line.Trim('[', ']', '\"');
                    currentDiskInfo = new DiskInfo();
                    currentDiskInfo.DateTime = DateTime.Now;
                    continue;
                }
                if (currentDiskInfo == null)
                {
                    continue;
                }
                var parts = line.Split(['='], 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim().Replace("\"", "");
                    switch (key)
                    {
                        case "device":
                            currentDiskInfo.DeviceName = value;
                            break;

                        case "name":
                            currentDiskInfo.RemarkName = value;
                            break;

                        case "id":
                            currentDiskInfo.Model = value;
                            break;

                        case "temp":
                            currentDiskInfo.Temperature = double.TryParse(value, out double d) ? d : 0;
                            break;

                        case "fsType":
                            currentDiskInfo.FileSystem = value;
                            break;

                        case "color":
                            currentDiskInfo.PowerMode = value;
                            break;

                        case "fsSize":
                            currentDiskInfo.Total = long.TryParse(value, out long l) ? l : 0;
                            break;

                        case "fsFree":
                            currentDiskInfo.Free = long.TryParse(value, out l) ? l : 0;
                            break;

                        case "fsUsed":
                            currentDiskInfo.Used = long.TryParse(value, out l) ? l : 0;
                            break;

                        case "type":
                            currentDiskInfo.Type = value switch
                            {
                                "Data" => DiskType.Data,
                                "Cache" => DiskType.Cache,
                                "Flash" => DiskType.Flash,
                                "Parity" => DiskType.Parity,
                                _ => DiskType.Unknown
                            };
                            break;

                        default:
                            break;
                    }
                }
            }
            return disks.ToArray();
        }
    }
}