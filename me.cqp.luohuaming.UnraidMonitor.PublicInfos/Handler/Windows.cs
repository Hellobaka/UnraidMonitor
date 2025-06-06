using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Motherboard;
using LibreHardwareMonitor.Hardware.Storage;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Handler
{
    public class Windows : HandlerBase, IVisitor
    {
        private IHardware[] CPUs { get; set; } = [];

        private IHardware[] Disks { get; set; } = [];

        private IHardware Motherboard { get; set; }

        private IHardware System { get; set; }

        private IHardware Memory { get; set; }

        private IHardware[] GPUs { get; set; } = [];

        private IHardware[] Fans { get; set; } = [];

        private Dictionary<IHardware, ISensor[]> Sensors { get; set; } = [];

        private Computer Computer { get; set; }

        public Windows()
        {
            Computer = new()
            {
                IsCpuEnabled = true,
                IsGpuEnabled = false,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsStorageEnabled = true,
            };
            Computer.Open();
            Computer.Accept(this);
        }

        ~Windows()
        {
            Computer.Close();
        }

        public override CpuInfo GetCpuInfo()
        {
            if (Motherboard == null)
            {
                return null;
            }
            Motherboard.Update();
            var cpu = (Motherboard as Motherboard).SMBios.Processors.FirstOrDefault();
            using var searcher = new ManagementObjectSearcher("select MaxClockSpeed from Win32_Processor");
            var maxClockSpeed = searcher.Get().Cast<ManagementBaseObject>().FirstOrDefault()?["MaxClockSpeed"];
            var item = new CpuInfo()
            {
                MaxTurboSpeedGHz = cpu.MaxSpeed / 1000f,
                LogicalCores = cpu.ThreadCount,
                PhysicalCores = cpu.CoreCount,
                Model = cpu.Version,
                DateTime = DateTime.Now,
                BaseSpeedGHz = (uint)(maxClockSpeed ?? 0) / 1000f
            };
            item.CacheItem();
            return item;
        }

        public override CpuUsage[] GetCpuUsages()
        {
            List<CpuUsage> list = [];
            foreach (var item in CPUs)
            {
                item.Update();
                foreach (var sensor in item.Sensors.Where(x=>x.SensorType == SensorType.Load))
                {
                    CpuUsage usage = new()
                    {
                        DateTime = DateTime.Now,
                        CPUId = sensor.Name,
                        User = (double)(sensor.Value ?? 0),
                    };
                    list.Add(usage);
                    usage.CacheItem();
                }
            }
            return list.ToArray();
        }

        public override DiskInfo[] GetDiskInfos()
        {
            List<DiskInfo> diskInfos = [];
            foreach (AbstractStorage item in Disks)
            {
                item.Update();
                DiskInfo info = new()
                {
                    DateTime = DateTime.Now,
                    DeviceName = item.Identifier.ToString(),
                    RemarkName = item.Identifier.ToString(),
                    FileSystem = string.Join(",", item.DriveInfos.Select(x => x.DriveFormat).Distinct()),
                    Model = item.Name,
                    Temperature = (double)(item.Sensors.FirstOrDefault(x => x.Name == "Temperature")?.Value ?? 0),
                    Free = item.DriveInfos.Sum(x => x.TotalFreeSpace),
                    Total = item.DriveInfos.Sum(x => x.TotalSize),
                    Used = item.DriveInfos.Sum(x => x.TotalSize - x.TotalFreeSpace),
                    Type = DiskInfo.DiskType.Data,
                };
                diskInfos.Add(info);
                info.CacheItem();
            }
            int index = 0;

            using ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Availability FROM Win32_DiskDrive");
            foreach (ManagementObject disk in searcher.Get())
            {
                if(index >= diskInfos.Count)
                {
                    break;
                }

                var availability = disk["Availability"] ?? "0";
                diskInfos[index].PowerMode = availability.ToString() == "1" ? "yellow-on" : "green-blink";
            }
            return diskInfos.ToArray();
        }

        public override DiskMountInfo[] GetDiskMountInfos()
        {
            List<DiskMountInfo> diskMountInfos = [];
            foreach (AbstractStorage item in Disks)
            {
                item.Update();
                foreach (var drive in item.DriveInfos)
                {
                    DiskMountInfo info = new()
                    {
                        AvailableBytes = drive.AvailableFreeSpace,
                        DateTime = DateTime.Now,
                        MountPoint = drive.Name,
                        Name = drive.Name,
                        Size = FormatBytes(drive.TotalSize),
                        TotalBytes = drive.TotalSize,
                        UsedBytes = drive.TotalSize - drive.AvailableFreeSpace,
                        Type = drive.DriveFormat.ToString()
                    };
                    diskMountInfos.Add(info);
                    info.CacheItem();
                }
            }
            return diskMountInfos.ToArray();
        }

        public override DiskSmartInfo[] GetDiskSmartInfos()
        {
            List<DiskSmartInfo> smartInfos = [];
            foreach (AbstractStorage item in Disks)
            {
                item.Update();
                DiskSmartInfo info = null;
                if (item is NVMeGeneric nvme && nvme.Smart.IsValid)
                {
                    var nvmeSmartInfo = nvme.Smart.GetInfo();
                    var nvmeHealthInfo = nvme.Smart.GetHealthInfo();
                    info = new()
                    {
                        AtaVersion = "",
                        CapacityBytes = (long)nvmeSmartInfo.TotalCapacity,
                        DeviceModel = nvmeSmartInfo.Model,
                        DeviceName = nvme.Identifier.ToString(),
                        FormattedCapacity = FormatBytes((long)nvmeSmartInfo.TotalCapacity),
                        FormFactor = "NVME",
                        LogicalSectorSize = 0,
                        ModelFamily = nvmeSmartInfo.Model,
                        PhysicalSectorSize = 0,
                        RotationRate = "NVME",
                        SataVersion = "NVME",
                        SerialNumber = nvmeSmartInfo.Serial,
                        SmartEnabled = true,
                        SmartSupported = true,
                        Smart = new()
                        {
                            BytesRead = (long?)nvmeHealthInfo.DataUnitRead,
                            BytesWritten = (long?)nvmeHealthInfo.DataUnitWritten,
                            TemperatureCelsius = nvmeHealthInfo.Temperature,
                            PowerOnHours = (int?)nvmeHealthInfo.PowerOnHours,
                            PowerCycles = (int?)nvmeHealthInfo.PowerCycle,
                            Type = DiskSmartInfo.DiskType.SSD,
                        },
                    };
                    info.Smart.HealthStatus = nvmeHealthInfo.AvailableSpare switch
                    {
                        <= 50 => DiskSmartInfo.DiskHealth.Fatal,
                        < 70 => DiskSmartInfo.DiskHealth.Warning,
                        _ => DiskSmartInfo.DiskHealth.Good
                    }; 
                    if (info.Smart.HealthStatus != DiskSmartInfo.DiskHealth.Fatal)
                    {
                        if (nvmeHealthInfo.CriticalWarning != LibreHardwareMonitor.Interop.Kernel32.NVME_CRITICAL_WARNING.None
                            || nvmeHealthInfo.MediaErrors != 0)
                        {
                            info.Smart.HealthStatus = DiskSmartInfo.DiskHealth.Warning;
                        }
                    }
                }
                else if(item is GenericHardDisk hardDisk)
                {
                    var smartData = hardDisk.Smart.ReadSmartData();
                    var smartThresholds = hardDisk.Smart.ReadSmartThresholds();
                    info = new()
                    {
                        DeviceName = item.Identifier.ToString(),
                        DeviceModel = hardDisk.Name,
                        ModelFamily = hardDisk.Name,
                        SmartEnabled = true,
                        SmartSupported = true,
                    };
                    if (smartData.Length > 0)
                    {
                        info.Smart = new()
                        {
                            TemperatureCelsius = smartData.FirstOrDefault(x => x.Id == 0xC2).RawValue[0] == 0 ? smartData.FirstOrDefault(x => x.Id == 0xC2).RawValue[2] : smartData.FirstOrDefault(x => x.Id == 0xC2).RawValue[0],
                            BytesRead = ParseSmartRawValue(smartData.FirstOrDefault(x => x.Id == 0xF2).RawValue),
                            BytesWritten = ParseSmartRawValue(smartData.FirstOrDefault(x => x.Id == 0xF1).RawValue),
                            PowerOnHours = (int?)ParseSmartRawValue(smartData.FirstOrDefault(x => x.Id == 0x09).RawValue),
                            PowerCycles = (int?)ParseSmartRawValue(smartData.FirstOrDefault(x => x.Id == 0x0C).RawValue),
                            CurrentPendingSectors = (int?)ParseSmartRawValue(smartData.FirstOrDefault(x => x.Id == 0xC5).RawValue),
                            HeadFlyingHours = TimeSpan.FromHours((int)ParseSmartRawValue(smartData.FirstOrDefault(x => x.Id == 0xF0).RawValue)),
                            OfflineUncorrectableSectors = (int?)ParseSmartRawValue(smartData.FirstOrDefault(x => x.Id == 0xC6).RawValue),
                            ReallocatedSectors = (int?)ParseSmartRawValue(smartData.FirstOrDefault(x => x.Id == 0x05).RawValue),
                            Type = DiskSmartInfo.DiskType.HDD,
                        };
                    }
                }
                if (info != null)
                {
                    smartInfos.Add(info);
                    info.CacheItem();
                }
            }
            return smartInfos.ToArray();
        }

        public override Dockers[] GetDockers()
        {
            return [];
        }

        public override FanInfo[] GetFanInfos()
        {
            List<FanInfo> fans = [];
            foreach(var item in Fans)
            {
                item.Update();
                FanInfo fan = new()
                {
                    DateTime = DateTime.Now,
                    Name = item.Name,
                    ParentName = item.Parent.Name,
                    RPM = (int)(item.Sensors.FirstOrDefault(x => x.Name == "Speed")?.Value ?? 0)
                };
                fan.CacheItem();
                fans.Add(fan);
            }
            foreach (var item in Motherboard.SubHardware)
            {
                item.Update();
                foreach (var fan in item.Sensors.Where(x => x.SensorType == SensorType.Fan))
                {
                    FanInfo fanInfo = new()
                    {
                        DateTime = DateTime.Now,
                        Name = fan.Name,
                        ParentName = item.Name,
                        RPM = (int)(fan.Value ?? 0)
                    };
                    fans.Add(fanInfo);
                    fanInfo.CacheItem();
                }
            }
            return fans.ToArray();
        }

        public override MemoryInfo GetMemoryInfo()
        {
            if (Memory == null || Motherboard == null)
            {
                return null;
            }
            Motherboard.Update();
            Memory.Update();

            long total = (Motherboard as Motherboard).SMBios.MemoryDevices.Sum(x => x.Size) * 1024;
            long used = (long)(Memory.Sensors.FirstOrDefault(x => x.SensorType == SensorType.Load).Value / 100f * total);
            long free = total - used;
            MemoryInfo item = new()
            {
                Available = free,
                Total = total,
                Used = used,
                DateTime = DateTime.Now
            };
            item.CacheItem();
            return item;
        }

        public override MotherboardInfo GetMotherboardInfo()
        {
            if (Motherboard == null)
            {
                return null;
            }
            Motherboard.Update();
            var m = Motherboard as Motherboard;
            MotherboardInfo item = new()
            {
                DateTime = DateTime.Now,
                Manufacturer = m.Manufacturer.ToString(),
                ProductName = m.Name,
            };
            item.CacheItem();
            return item;
        }

        public override NetworkInterfaceInfo[] GetNetworkInterfaceInfos()
        {
            List<NetworkInterfaceInfo> list = [];
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up) // 只获取正在使用的网卡
                {
                    List<IPAddress> ipv6Addresses = [];
                    List<IPAddress> ipv4Addresses = [];

                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            ipv6Addresses.Add(ip.Address);
                        }
                        else if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipv4Addresses.Add(ip.Address);
                        }
                    }
                    NetworkInterfaceInfo info = new()
                    {
                        DateTime = DateTime.Now,
                        IpAddresses = ipv4Addresses.Select(ip => ip.ToString()).ToList(),
                        Ipv6Addresses = ipv6Addresses.Select(ip => ip.ToString()).ToList(),
                        MacAddress = BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes()),
                        Name = ni.Name,
                    };

                    list.Add(info);
                    info.CacheItem();
                }
            }

            return list.ToArray();
        }

        public override NetworkTrafficInfo[] GetNetworkTrafficInfos()
        {
            List<NetworkTrafficInfo> list = [];

            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                var statistics = item.GetIPv4Statistics();
                string name = item.Name;
                long recv = statistics.BytesReceived;
                long sent = statistics.BytesSent;
                if (recv == 0)
                {
                    continue;
                }
                NetworkTrafficInfo info = new()
                {
                    Name = name,
                    DateTime = DateTime.Now,
                    RxBytes = recv,
                    TxBytes = sent
                };
                list.Add(info);
                info.CacheItem();
            }
            return list.ToArray();
        }

        public override Notification[] GetNotifications()
        {
            return [];
        }

        public override SystemInfo GetSystemInfo()
        {
            string osName = "Unknown OS";
            string version = "Unknown Version";
            string edition = "Unknown Edition";

            using (var searcher = new ManagementObjectSearcher("SELECT Caption, Version, OSArchitecture FROM Win32_OperatingSystem"))
            {
                foreach (var os in searcher.Get())
                {
                    osName = os["Caption"]?.ToString() ?? osName;
                    version = os["Version"]?.ToString() ?? version;
                    edition = os["OSArchitecture"]?.ToString() ?? edition;
                }
            }
            string? releaseId = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "DisplayVersion", "")?.ToString();
            if (!string.IsNullOrEmpty(releaseId))
            {
                version = releaseId;
            }

            SystemInfo item = new()
            {
                SystemEdition = edition,
                SystemName = osName,
                Version = version,
            };
            item.CacheItem();
            return item;
        }

        public override SystemUptime GetSystemUptime()
        {
            var timespan = TimeSpan.FromMilliseconds(Environment.TickCount);
            SystemUptime item = new()
            {
                UpTimeDay = timespan.Days,
                UpTimeHour = timespan.Hours,
                UpTimeMinute = timespan.Minutes,
                UpTimeSecond = timespan.Seconds
            };
            item.CacheItem();
            return item;
        }

        public override TemperatureInfo[] GetTemperatureInfos()
        {
            List<TemperatureInfo> list = [];
            foreach(var item in CPUs.Concat(GPUs).Concat(Disks))
            {
                item.Update();
                foreach(var sensor in item.Sensors.Where(x=>x.SensorType == SensorType.Temperature))
                {
                    TemperatureInfo info = new()
                    {
                        Name = sensor.Name,
                        Temperature = sensor.Value ?? 0,
                        ParentName = item.Name,
                        DateTime = DateTime.Now
                    };
                    list.Add(info);
                    info.CacheItem();
                }
            }
            return list.ToArray();
        }

        public override UPSStatus GetUPS()
        {
            UPSStatus ups = new();
            ups.DateTime = DateTime.Now;
            using ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
            foreach (ManagementObject battery in searcher.Get())
            {
                ups.Status = battery["Availability"]?.ToString() == "2" ? "ONLINE" : "ONBATT";
                ups.BatteryLevel = (ushort)battery["EstimatedChargeRemaining"];
                ups.TimeLeft = (uint)battery["EstimatedRunTime"];
                ups.Model = battery["Caption"].ToString();
                ups.MaxPower = 0;
                ups.CurrentVoltage = 0;
                ups.CurrentLoad = 0;

                break;
            }
            ups.CacheItem();
            return ups;
        }

        public override VirtualMachine[] GetVirtualMachines()
        {
            return [];
        }

        private static string FormatBytes(long bytes)
        {
            const long TB = 1024L * 1024 * 1024 * 1024;
            const long GB = 1024L * 1024 * 1024;
            const long MB = 1024L * 1024;
            const long KB = 1024L;

            if (bytes >= TB)
            {
                return $"{bytes / (double)TB:F2} TB";
            }
            else if (bytes >= GB)
            {
                return $"{bytes / (double)GB:F2} GB";
            }
            else if (bytes >= MB)
            {
                return $"{bytes / (double)MB:F2} MB";
            }
            else
            {
                return bytes >= KB ? $"{bytes / (double)KB:F2} KB" : $"{bytes} B";
            }
        }

        public long ParseSmartRawValue(byte[] rawValues) => (long)BitConverter.ToUInt64([.. rawValues, .. new byte[2]], 0);

        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            CacheHardware(hardware);
            foreach (IHardware subHardware in hardware.SubHardware)
            {
                subHardware.Update();
                subHardware.Accept(this);
                CacheHardware(subHardware);
            }
        }

        private void CacheHardware(IHardware hardware)
        {
            switch (hardware.HardwareType)
            {
                case HardwareType.Motherboard:
                    Motherboard = hardware;
                    break;
                case HardwareType.Cpu:
                    if (CPUs.Contains(hardware) is false)
                    {
                        CPUs = [.. CPUs, hardware];
                    }
                    break;
                case HardwareType.Memory:
                    Memory = hardware;
                    break;
                case HardwareType.GpuNvidia:
                case HardwareType.GpuAmd:
                case HardwareType.GpuIntel:
                    if (GPUs.Contains(hardware) is false)
                    {
                        GPUs = [.. GPUs, hardware];
                    }
                    break;
                case HardwareType.Storage:
                    if (Disks.Contains(hardware) is false)
                    {
                        Disks = [.. Disks, hardware];
                    }
                    break;

                case HardwareType.Cooler:
                    if (Fans.Contains(hardware) is false)
                    {
                        Fans = [.. Fans, hardware];
                    }
                    break;
                default:
                    break;
            }
        }

        public void VisitSensor(ISensor sensor)
        {
        }

        public void VisitParameter(IParameter parameter)
        {
        }
    }
}
