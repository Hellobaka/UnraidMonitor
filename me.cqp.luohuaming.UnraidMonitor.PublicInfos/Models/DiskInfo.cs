using System;
using System.Collections.Generic;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class DiskInfo
    {
        public enum DiskType
        {
            Flash,

            Data,

            Cache,

            Parity,

            Unknown
        }

        public DiskType Type { get; set; } = DiskType.Data;

        public string RemarkName { get; set; } = "";

        public string DeviceName { get; set; } = "";

        public string Model { get; set; } = "";

        public string PowerMode { get; set; }

        public long Total { get; set; }

        public long Used { get; set; }

        public long Free { get; set; }

        public double Temperature { get; set; }

        public string FileSystem { get; set; }

        public bool Running => PowerMode.Contains("-on");

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