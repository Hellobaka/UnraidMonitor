using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Handler;
using System;
using System.IO;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos
{
    public class Tester
    {
        static void Main(string[] args)
        {
            MainSave.AppDirectory = Path.GetFullPath(".");
            MainSave.ImageDirectory = CommonHelper.GetAppImageDirectory();

            AppConfig appConfig = new(Path.Combine(MainSave.AppDirectory, "Config.json"));
            appConfig.LoadConfig();
            appConfig.EnableAutoReload();

            CommandIntervalConfig intervalConfig = new(Path.Combine(MainSave.AppDirectory, "Interval.json"));
            intervalConfig.LoadConfig();
            intervalConfig.EnableAutoReload();

            var handler = new Linux();
            handler.StartMonitor();
            Console.ReadLine();
            // -1. 系统启动时间
            var uptime = handler.GetSystemUptime();
            Console.WriteLine("[Uptime]");
            PrintProperties(uptime);
            handler.InsertData(uptime);

            // 0. 系统信息
            var systemInfo = handler.GetSystemInfo();
            Console.WriteLine("[SystemInfo]");
            PrintProperties(systemInfo);
            handler.InsertData(systemInfo);

            // 1. CPU信息
            var cpuInfo = handler.GetCpuInfo();
            Console.WriteLine("[CPUInfo]");
            PrintProperties(cpuInfo);
            handler.InsertData(cpuInfo);

            // 2.1. UPS信息
            var ups = handler.GetUPS();
            Console.WriteLine("[UPS]");
            PrintProperties(ups);
            handler.InsertData(ups);

            // 2. CPU使用率
            var cpuUsages = handler.GetCpuUsages();
            Console.WriteLine("[CPUUsage]");
            foreach (var item in cpuUsages ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.CpuUsage>())
                PrintProperties(item);
            handler.InsertData(cpuUsages);

            // 3. 内存信息
            var memoryInfo = handler.GetMemoryInfo();
            Console.WriteLine("[MemoryInfo]");
            PrintProperties(memoryInfo);

            // 4. 主板信息
            var motherboardInfo = handler.GetMotherboardInfo();
            Console.WriteLine("[MotherboardInfo]");
            PrintProperties(motherboardInfo);

            // 5. 磁盘挂载信息
            var diskMounts = handler.GetDiskMountInfos();
            Console.WriteLine("[DiskMountInfo]");
            foreach (var item in diskMounts ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.DiskMountInfo>())
                PrintProperties(item);

            // 5.1. 磁盘信息
            var disks = handler.GetDiskInfos();
            Console.WriteLine("[DiskInfo]");
            foreach (var item in disks ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.DiskInfo>())
                PrintProperties(item);

            // 6. Docker容器
            var dockers = handler.GetDockers();
            Console.WriteLine("[Dockers]");
            foreach (var item in dockers ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.Dockers>())
                PrintProperties(item);

            // 7. 虚拟机
            var vms = handler.GetVirtualMachines();
            Console.WriteLine("[VirtualMachine]");
            foreach (var item in vms ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.VirtualMachine>())
                PrintProperties(item);

            // 8. 风扇信息
            var fans = handler.GetFanInfos();
            Console.WriteLine("[FanInfo]");
            foreach (var item in fans ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.FanInfo>())
                PrintProperties(item);

            // 9. 网卡信息
            var nics = handler.GetNetworkInterfaceInfos();
            Console.WriteLine("[NetworkInterfaceInfo]");
            foreach (var item in nics ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.NetworkInterfaceInfo>())
                PrintProperties(item);

            // 10. 网络流量
            var traffics = handler.GetNetworkTrafficInfos();
            Console.WriteLine("[NetworkTrafficInfo]");
            foreach (var item in traffics ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.NetworkTrafficInfo>())
                PrintProperties(item);

            // 11. 温度信息
            var temps = handler.GetTemperatureInfos();
            Console.WriteLine("[TemperatureInfo]");
            foreach (var item in temps ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.TemperatureInfo>())
                PrintProperties(item);

            // 12. 磁盘SMART信息
            var diskSmart = handler.GetDiskSmartInfos();
            Console.WriteLine("[DiskSmartInfo]");
            foreach (var item in diskSmart ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.DiskSmartInfo>())
            {
                PrintProperties(item);
                PrintProperties(item.Smart);
            }
        }

        static void PrintProperties(object obj)
        {
            if (obj == null)
            {
                Console.WriteLine("  [null]");
                return;
            }
            var type = obj.GetType();
            foreach (var prop in type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                var value = prop.GetValue(obj);
                if (value is System.Collections.IEnumerable enumerable && !(value is string))
                {
                    Console.WriteLine($"  {prop.Name}:");
                    foreach (var sub in enumerable)
                    {
                        Console.WriteLine($"    - {sub}");
                    }
                }
                else
                {
                    Console.WriteLine($"  {prop.Name}: {value}");
                }
            }
            Console.WriteLine();
        }
    }
}
