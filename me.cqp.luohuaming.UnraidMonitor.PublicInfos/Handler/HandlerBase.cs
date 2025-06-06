using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Handler
{
    public class HandlerBase
    {
        public HandlerBase()
        {
            Instance = this;
        }

        public static HandlerBase Instance { get; set; }

        public Timer CpuInfoTimer { get; set; }

        public Timer CpuUsagesTimer { get; set; }

        public Timer MemoryInfoTimer { get; set; }

        public Timer MotherboardInfoTimer { get; set; }

        public Timer DiskMountInfosTimer { get; set; }

        public Timer DockersTimer { get; set; }

        public Timer VirtualMachinesTimer { get; set; }

        public Timer FanInfosTimer { get; set; }

        public Timer NetworkInterfaceInfosTimer { get; set; }

        public Timer NetworkTrafficInfosTimer { get; set; }

        public Timer TemperatureInfosTimer { get; set; }

        public Timer DiskInfosTimer { get; set; }

        public Timer DiskSmartInfosTimer { get; set; }

        public Timer SystemInfoTimer { get; set; }

        public Timer SystemUptimeTimer { get; set; }

        public Timer UPSTimer { get; set; }

        public void StopMonitor()
        {
            var instanceType = this.GetType();

            foreach (var property in instanceType.GetProperties())
            {
                if (property.PropertyType.Name == "Timer")
                {
                    var timer = property.GetValue(this) as Timer;
                    timer?.Dispose();
                    property.SetValue(this, null);
                }
            }
        }

        public void StartMonitor()
        {
            var configType = typeof(CommandIntervalConfig);
            var properties = configType.GetProperties(BindingFlags.Public | BindingFlags.Static);

            var instanceType = this.GetType();

            foreach (var property in properties)
            {
                string key = property.Name; // CPUInfo
                if (key == "Instance")
                {
                    continue;
                }
                int interval = (int)property.GetValue(null);

                string timerFieldName = key + "Timer"; // CPUInfoTimer
                string methodName = "Get" + key; // GetCPUInfo

                var t = instanceType.GetProperty(timerFieldName);
                var collectMethod = instanceType.GetMethod(methodName);

                if (t == null || collectMethod == null)
                {
                    Debugger.Break();
                    continue;
                }

                (t.GetValue(this) as Timer)?.Dispose();

                if (interval >= 0)
                {
                    Timer timer = new(_ =>
                    {
                        Console.WriteLine($"Executing {collectMethod.Name}");
                        var data = collectMethod.Invoke(this, null);
                    }, null, 0, interval);

                    t.SetValue(this, timer);
                }
                else
                {
                    t.SetValue(this, null);
                }
            }
        }

        public virtual UPSStatus GetUPS()
        {
            throw new NotImplementedException();
        }

        public virtual DiskInfo[] GetDiskInfos()
        {
            throw new NotImplementedException();
        }

        public virtual CpuInfo GetCpuInfo()
        {
            throw new NotImplementedException();
        }

        public virtual CpuUsage[] GetCpuUsages()
        {
            throw new NotImplementedException();
        }

        public virtual MemoryInfo GetMemoryInfo()
        {
            throw new NotImplementedException();
        }

        public virtual MotherboardInfo GetMotherboardInfo()
        {
            throw new NotImplementedException();
        }

        public virtual DiskMountInfo[] GetDiskMountInfos()
        {
            throw new NotImplementedException();
        }

        public virtual Dockers[] GetDockers()
        {
            throw new NotImplementedException();
        }

        public virtual VirtualMachine[] GetVirtualMachines()
        {
            throw new NotImplementedException();
        }

        public virtual FanInfo[] GetFanInfos()
        {
            throw new NotImplementedException();
        }

        public virtual NetworkInterfaceInfo[] GetNetworkInterfaceInfos()
        {
            throw new NotImplementedException();
        }

        public virtual NetworkTrafficInfo[] GetNetworkTrafficInfos()
        {
            throw new NotImplementedException();
        }

        public virtual TemperatureInfo[] GetTemperatureInfos()
        {
            throw new NotImplementedException();
        }

        public virtual DiskSmartInfo[] GetDiskSmartInfos()
        {
            throw new NotImplementedException();
        }

        public virtual SystemInfo GetSystemInfo()
        {
            throw new NotImplementedException();
        }

        public virtual SystemUptime GetSystemUptime()
        {
            throw new NotImplementedException();
        }

        public virtual Notification[] GetNotifications()
        {
            throw new NotImplementedException();
        }
    }
}