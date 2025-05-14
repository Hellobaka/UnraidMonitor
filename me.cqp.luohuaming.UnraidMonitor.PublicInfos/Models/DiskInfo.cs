using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos
{
    public class DiskInfo
    {
        public enum DiskType
        {
            HDD,

            SSD,

            Unknown
        }

        public enum DiskHealth
        {
            Good,

            Warning,

            Fatal,

            Unknown
        }

        /// <summary>
        /// InLinux Name(/dev/sdb)
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 型号系列
        /// </summary>
        public string ModelFamily { get; set; }

        /// <summary>
        /// 设备型号
        /// </summary>
        public string DeviceModel { get; set; }

        /// <summary>
        /// 序列号
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// 字节容量
        /// </summary>
        public long CapacityBytes { get; set; }

        /// <summary>
        /// 格式化容量
        /// </summary>
        public string FormattedCapacity { get; set; }

        /// <summary>
        /// 逻辑扇区大小
        /// </summary>
        public int LogicalSectorSize { get; set; }

        /// <summary>
        /// 物理扇区大小
        /// </summary>
        public int PhysicalSectorSize { get; set; }

        /// <summary>
        /// 转速/类型
        /// </summary>
        public string RotationRate { get; set; }

        /// <summary>
        /// 外形尺寸
        /// </summary>
        public string FormFactor { get; set; }

        /// <summary>
        /// ATA版本
        /// </summary>
        public string AtaVersion { get; set; }

        /// <summary>
        /// SATA版本
        /// </summary>
        public string SataVersion { get; set; }

        /// <summary>
        /// SMART支持状态
        /// </summary>
        public bool SmartSupported { get; set; }

        /// <summary>
        /// SMART启用状态
        /// </summary>
        public bool SmartEnabled { get; set; }

        public SmartInfo Smart { get; set; }

        private static Regex ModelFamilyRegex { get; } =
            new(@"(?:Model Family|Model Number):\s+(.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex DeviceModelRegex { get; } =
            new(@"(?:Device Model|Model Number):\s+(.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex SerialNumberRegex { get; } =
            new(@"Serial Number:\s+(.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex CapacityRegex { get; } =
            new(@"(?:User Capacity|Total NVM Capacity):\s+([\d,]+) bytes? \[([^\]]+)\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex SectorSizeRegex { get; } =
            new(@"(?:Sector Sizes:\s+(\d+) bytes logical, (\d+) bytes physical|Namespace 1 Formatted LBA Size:\s+(\d+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex RotationRateRegex { get; } =
            new(@"Rotation Rate:\s+(.+ rpm|Solid State Device)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex FormFactorRegex { get; } =
            new(@"Form Factor:\s+(.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex AtaVersionRegex { get; } =
            new(@"ATA Version is:\s+(.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex SataVersionRegex { get; } =
            new(@"SATA Version is:\s+(.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex SmartAvailableRegex { get; } =
            new(@"SMART support is:\s+Available", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex SmartEnabledRegex { get; } =
            new(@"SMART support is:\s+Enabled", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static DiskInfo ParseSmartctl(string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? throw new ArgumentNullException(nameof(input))
                : new DiskInfo
                {
                    ModelFamily = ModelFamilyRegex.Match(input).Groups[1].Value.Trim(),
                    DeviceModel = DeviceModelRegex.Match(input).Groups[1].Value.Trim(),
                    SerialNumber = SerialNumberRegex.Match(input).Groups[1].Value.Trim(),
                    FormFactor = FormFactorRegex.Match(input).Groups[1].Value.Trim(),
                    AtaVersion = AtaVersionRegex.Match(input).Groups[1].Value.Trim(),
                    SataVersion = SataVersionRegex.Match(input).Groups[1].Value.Trim(),
                    RotationRate = RotationRateRegex.Match(input).Groups[1].Value.Trim(),
                    SmartSupported = SmartAvailableRegex.IsMatch(input),
                    SmartEnabled = SmartEnabledRegex.IsMatch(input),
                    CapacityBytes = ParseCapacity(CapacityRegex.Match(input)),
                    FormattedCapacity = CapacityRegex.Match(input).Groups[2].Value.Trim(),
                    LogicalSectorSize = int.TryParse(SectorSizeRegex.Match(input).Groups[1].Value, out int value) ? value : 0,
                    PhysicalSectorSize = int.TryParse(SectorSizeRegex.Match(input).Groups[2].Value, out value) ? value :0,
                    Smart = SmartInfo.Parse(input)
                };
        }

        private static long ParseCapacity(Match match)
        {
            return match.Success ?
                long.Parse(match.Groups[1].Value.Replace(",", "")) :
                0L;
        }

        public class SmartInfo
        {
            // 预编译正则表达式
            // 捕获: 1:ID#, 2:ATTRIBUTE_NAME, 3:VALUE, 4:WORST, 5:THRESH, 6:TYPE, 7:UPDATED, 8:WHEN_FAILED, 9:RAW_VALUE
            private static Regex HddAttributeRegex { get; } = new(
                @"^\s*(\d+)\s+(\w[\w\-_]+)\s+0x[0-9a-f]+\s+(\d+)\s+(\d+)\s+(\d+|-)\s+(\w+)\s+(\w+)\s+(\w+|-)\s+(\d+|.+)$",
                RegexOptions.Compiled | RegexOptions.Multiline);

            // 捕获: 1:Key, 2:Value
            private static Regex NvmeAttributeRegex { get; } = new(
                @"^([\w\s]+):\s+(.+)$",
                RegexOptions.Compiled | RegexOptions.Multiline);

            // 捕获温度数值，忽略单位或 Min/Max 信息
            private static Regex TemperatureRegex { get; } = new(
                @"(\d+)\s*°?C|(\d+)\s*\(Min/Max",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // 捕获写入/读取的数值 (忽略单位)
            private static Regex BytesWrittenRegex { get; } = new( // 这个Regex实际上在ParseStorageValue中使用，且只捕获数字
                @"(\d+)\s*\[?([\d.]+\s*[TGMK]?B)?\]?",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // 公共属性
            public DiskType Type { get; private set; }

            public DiskHealth HealthStatus { get; private set; } = DiskHealth.Unknown; // 默认Unknown

            public int? TemperatureCelsius { get; private set; }

            public long? BytesRead { get; private set; }

            public long? BytesWritten { get; private set; }

            public int? PowerOnHours { get; private set; }

            public int? PowerCycles { get; private set; }

            public string InterfaceSpeed { get; private set; }

            public TimeSpan? HeadFlyingHours { get; private set; } // HDD特有

            public int? ReallocatedSectors { get; private set; }    // HDD特有，用于健康度计算

            public int? CurrentPendingSectors { get; private set; } // HDD特有，用于健康度计算

            public int? OfflineUncorrectableSectors { get; private set; } // HDD特有，用于健康度计算

            // 存储所有原始属性，方便调试或显示
            public Dictionary<string, string> RawAttributes { get; } = new(StringComparer.OrdinalIgnoreCase);

            public static SmartInfo Parse(string input)
            {
                var info = new SmartInfo();

                if (string.IsNullOrWhiteSpace(input))
                {
                    info.HealthStatus = DiskHealth.Unknown; // 输入为空则未知
                    return info;
                }

                if (input.Contains("NVMe Log 0x02"))
                {
                    info.Type = DiskType.SSD;
                    ParseNvmeAttributes(input, info);
                    CalculateSsdHealth(info);
                }
                else if (input.Contains("Vendor Specific SMART Attributes"))
                {
                    info.Type = DiskType.HDD;
                    ParseHddAttributes(input, info);
                    CalculateHddHealth(info);
                }
                else
                {
                    info.Type = DiskType.Unknown;
                    info.HealthStatus = DiskHealth.Unknown; // 无法识别类型则未知
                }

                return info;
            }

            private static void ParseHddAttributes(string input, SmartInfo info)
            {
                var matches = HddAttributeRegex.Matches(input);
                foreach (Match match in matches)
                {
                    if (!match.Success)
                    {
                        continue;
                    }

                    string attributeName = match.Groups[2].Value.Trim();
                    string rawValue = match.Groups[9].Value.Trim();

                    // 将所有属性及原始值添加到RawAttributes字典
                    info.RawAttributes[attributeName] = rawValue;

                    // 提取关键指标到单独属性
                    switch (attributeName)
                    {
                        case "Temperature_Celsius":
                        case "Airflow_Temperature_Cel":
                            // Prefer Temperature_Celsius if both exist, otherwise use Airflow_Temperature_Cel
                            if (!info.TemperatureCelsius.HasValue)
                            {
                                info.TemperatureCelsius = ParseTemperature(rawValue);
                            }
                            break;

                        case "Total_LBAs_Read":
                            // Assuming LBA size is 512 bytes for calculation
                            info.BytesRead = ParseLbaToBytes(rawValue) * 512;
                            break;

                        case "Total_LBAs_Written":
                            // Assuming LBA size is 512 bytes for calculation
                            info.BytesWritten = ParseLbaToBytes(rawValue) * 512;
                            break;

                        case "Power_On_Hours":
                            info.PowerOnHours = ParseInt(rawValue);
                            break;

                        case "Power_Cycle_Count": // 修正属性名称
                            info.PowerCycles = ParseInt(rawValue);
                            break;

                        case "Head_Flying_Hours":
                            info.HeadFlyingHours = ParseTimeSpan(rawValue);
                            break;

                        case "Reallocated_Sector_Ct":
                            info.ReallocatedSectors = ParseInt(rawValue);
                            break;

                        case "Current_Pending_Sector": // 新增
                            info.CurrentPendingSectors = ParseInt(rawValue);
                            break;

                        case "Offline_Uncorrectable": // 新增
                            info.OfflineUncorrectableSectors = ParseInt(rawValue);
                            break;
                            // 添加其他需要的HDD属性解析
                    }
                }

                // 尝试从其他字段获取接口速度
                if (input.Contains("SATA"))
                {
                    if (input.Contains("SATA 3.3") || input.Contains("6Gb/s"))
                    {
                        info.InterfaceSpeed = "SATA 6Gbps";
                    }
                    else
                    {
                        info.InterfaceSpeed = input.Contains("3Gb/s") ? "SATA 3Gbps" : input.Contains("1.5Gb/s") ? "SATA 1.5Gbps" : "SATA";
                    }
                }
                // 您可能需要更复杂的逻辑来从SMART输出中确定准确的接口速度
            }

            private static void ParseNvmeAttributes(string input, SmartInfo info)
            {
                var matches = NvmeAttributeRegex.Matches(input);
                foreach (Match match in matches)
                {
                    if (!match.Success)
                    {
                        continue;
                    }

                    string key = match.Groups[1].Value.Trim();
                    string value = match.Groups[2].Value.Trim();
                    info.RawAttributes[key] = value;

                    // 提取关键指标
                    switch (key)
                    {
                        case "Temperature":
                        case "Temperature Sensor 1": // NVMe often has multiple sensors
                            if (!info.TemperatureCelsius.HasValue) // Take the first temperature found
                            {
                                info.TemperatureCelsius = ParseTemperature(value);
                            }
                            break;

                        case "Data Units Read":
                            // NVMe units are typically 512KB (1024 * 512 bytes)
                            info.BytesRead = ParseStorageValue(value) * 524288L; // Use L for long literal
                            break;

                        case "Data Units Written":
                            // NVMe units are typically 512KB
                            info.BytesWritten = ParseStorageValue(value) * 524288L; // Use L for long literal
                            break;

                        case "Power On Hours":
                            info.PowerOnHours = ParseInt(value);
                            break;

                        case "Power Cycles":
                            info.PowerCycles = ParseInt(value);
                            break;

                        case "Available Spare":
                            // Used for health calculation, raw value stored in RawAttributes
                            break;
                            // 添加其他需要的NVMe属性解析
                    }
                }

                // 检测PCIe版本
                if (input.Contains("PCIe Gen5"))
                {
                    info.InterfaceSpeed = "PCIe 5.0";
                }
                else if (input.Contains("PCIe Gen4"))
                {
                    info.InterfaceSpeed = "PCIe 4.0";
                }
                else if (input.Contains("PCIe Gen3"))
                {
                    info.InterfaceSpeed = "PCIe 3.0";
                }
                else if (input.Contains("PCIe"))
                {
                    info.InterfaceSpeed = "PCIe";
                }
            }

            /// <summary>
            /// 计算HDD健康状态，基于关键SMART原始值和温度。
            /// </summary>
            /// <param name="info">SmartInfo对象</param>
            private static void CalculateHddHealth(SmartInfo info)
            {
                info.HealthStatus = DiskHealth.Good; // 默认健康良好

                // 致命错误：重分配扇区、当前待映射扇区或脱机无法校正扇区非零
                if ((info.ReallocatedSectors.HasValue && info.ReallocatedSectors.Value > 0) ||
                    (info.CurrentPendingSectors.HasValue && info.CurrentPendingSectors.Value > 0) ||
                    (info.OfflineUncorrectableSectors.HasValue && info.OfflineUncorrectableSectors.Value > 0))
                {
                    info.HealthStatus = DiskHealth.Fatal;
                }

                // 您可以根据需要添加更多基于其他SMART属性的警告或致命判断逻辑
                // 例如：Spin Up Time (3), Command Timeout (188), UDMA CRC Error Count (199) 等
                // 需要从 RawAttributes 中获取这些值并进行判断
                if (info.HealthStatus != DiskHealth.Fatal && info.RawAttributes.TryGetValue("UDMA_CRC_Error_Count", out var crcErrorValueStr))
                {
                    if (ParseInt(crcErrorValueStr) > 0)
                    {
                        info.HealthStatus = DiskHealth.Warning; // CRC错误通常是连接问题或轻微错误，标记为Warning
                    }
                }
            }

            /// <summary>
            /// 计算SSD健康状态，基于Available Spare百分比。
            /// </summary>
            /// <param name="info">SmartInfo对象</param>
            private static void CalculateSsdHealth(SmartInfo info)
            {
                // 尝试获取Available Spare属性的原始值
                if (!info.RawAttributes.TryGetValue("Available Spare", out var spareValueStr))
                {
                    info.HealthStatus = DiskHealth.Unknown; // 找不到关键属性，健康状态未知
                    return;
                }

                // 解析Available Spare百分比
                int? sparePercent = ParseInt(spareValueStr.Replace("%", ""));

                if (!sparePercent.HasValue)
                {
                    info.HealthStatus = DiskHealth.Unknown; // 无法解析百分比，健康状态未知
                    return;
                }

                // 根据Available Spare百分比评估健康度
                info.HealthStatus = sparePercent.Value switch
                {
                    <= 50 => DiskHealth.Fatal,
                    < 70 => DiskHealth.Warning,
                    _ => DiskHealth.Good
                };

                // Critical Warning (0x00) 以及 0E 错误
                if (info.HealthStatus != DiskHealth.Fatal
                    && info.RawAttributes.TryGetValue("Critical Warning", out var criticalWarningStr)
                    && info.RawAttributes.TryGetValue("Media and Data Integrity Errors", out var error_0E))
                {
                    if (!criticalWarningStr.Trim().Equals("0x00") || error_0E != "0")
                    {
                        info.HealthStatus = DiskHealth.Warning;
                    }
                }
            }

            #region 辅助解析方法

            /// <summary>
            /// 从字符串中解析温度值。
            /// </summary>
            private static int? ParseTemperature(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    return null;
                }

                var match = TemperatureRegex.Match(input);
                if (!match.Success)
                {
                    return null;
                }

                // TemperatureRegex 捕获的是第一个数字 (Group 1) 或括号内的数字 (Group 2)
                // 取匹配成功的那个组的数值
                string tempStr = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;

                return int.TryParse(tempStr, out var result) ? result : null;
            }

            /// <summary>
            /// 从LBA计数字符串中解析LBA数量。
            /// </summary>
            private static long? ParseLbaToBytes(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    return null;
                }

                var numStr = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                return numStr == null ? null : long.TryParse(numStr, out var result) ? result : null;
            }

            /// <summary>
            /// 从NVMe存储量字符串中解析数字部分 (忽略单位)。
            /// </summary>
            private static long? ParseStorageValue(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    return null;
                }
                // 移除逗号，然后取第一个数字部分
                var numStr = input.Replace(",", "").Split([' ', '['], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                return numStr == null ? null : long.TryParse(numStr, out var result) ? result : null;
            }

            /// <summary>
            /// 从字符串中解析整数值。
            /// </summary>
            private static int? ParseInt(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    return null;
                }
                // 取第一个数字部分并尝试解析
                var numStr = input.Split([' '], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                return numStr == null ? null : int.TryParse(numStr, out var result) ? result : null;
            }

            /// <summary>
            /// 从特定格式的字符串中解析TimeSpan (如 "5335h+05m+03.690s")。
            /// </summary>
            private static TimeSpan? ParseTimeSpan(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    return null;
                }

                // 处理格式如 "5335h+05m+03.690s"
                if (input.Contains("h"))
                {
                    try
                    {
                        var parts = input.Split(['h', 'm', 's', '+'], StringSplitOptions.RemoveEmptyEntries);
                        int hours = 0;
                        int minutes = 0;
                        double seconds = 0;

                        if (parts.Length > 0)
                        {
                            hours = int.Parse(parts[0]);
                        }

                        if (parts.Length > 1)
                        {
                            minutes = int.Parse(parts[1]);
                        }

                        if (parts.Length > 2)
                        {
                            seconds = double.Parse(parts[2]);
                        }

                        return TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);
                    }
                    catch (FormatException)
                    {
                        // 解析失败
                        return null;
                    }
                }
                // 您可能需要添加对其他TimeSpan格式的支持
                return null;
            }

            #endregion 辅助解析方法
        }
    }
}