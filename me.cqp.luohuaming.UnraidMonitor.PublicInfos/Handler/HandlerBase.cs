using LiteDB;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Handler
{
    public class HandlerBase
    {
        public HandlerBase()
        {
            StartMonitor();
            Instance = this;
        }

        public static HandlerBase Instance { get; set; }

        private Timer CPUInfoTimer { get; set; }

        private Timer CPUUsageTimer { get; set; }

        private Timer MemoryInfoTimer { get; set; }

        private Timer MotherboardInfoTimer { get; set; }

        private Timer DiskMountInfoTimer { get; set; }

        private Timer DockersTimer { get; set; }

        private Timer VirtualMachinesTimer { get; set; }

        private Timer FanInfoTimer { get; set; }

        private Timer NetworkInterfaceInfoTimer { get; set; }

        private Timer NetworkTrafficInfoTimer { get; set; }

        private Timer TemperatureInfoTimer { get; set; }

        private Timer DiskInfoTimer { get; set; }

        public void StopMonitor()
        {
            CPUInfoTimer?.Dispose();
            CPUUsageTimer?.Dispose();
            MemoryInfoTimer?.Dispose();
            MotherboardInfoTimer?.Dispose();
            DiskMountInfoTimer?.Dispose();
            DockersTimer?.Dispose();
            VirtualMachinesTimer?.Dispose();
            FanInfoTimer?.Dispose();
            NetworkInterfaceInfoTimer?.Dispose();
            NetworkTrafficInfoTimer?.Dispose();
            TemperatureInfoTimer?.Dispose();
            DiskInfoTimer?.Dispose();
        }

        public void StartMonitor()
        {
            CPUInfoTimer = new(_ => InsertData(GetCpuInfo()) , null, 0, CommandIntervalConfig.CPUInfo);
            CPUUsageTimer = new(_ => InsertData(GetCpuUsages()) , null, 0, CommandIntervalConfig.CPUUsage);
            MemoryInfoTimer = new(_ => InsertData(GetMemoryInfo()) , null, 0, CommandIntervalConfig.MemoryInfo);
            MotherboardInfoTimer = new(_ => InsertData(GetMotherboardInfo()) , null, 0, CommandIntervalConfig.MotherboardInfo);
            DiskMountInfoTimer = new(_ => InsertData(GetDiskMountInfo()) , null, 0, CommandIntervalConfig.DiskMountInfo);
            DockersTimer = new(_ => InsertData(GetDockers()) , null, 0, CommandIntervalConfig.Dockers);
            VirtualMachinesTimer = new(_ => InsertData(GetVirtualMachines()) , null, 0, CommandIntervalConfig.VirtualMachine);
            FanInfoTimer = new(_ => InsertData(GetFanInfos()) , null, 0, CommandIntervalConfig.FanInfo);
            NetworkInterfaceInfoTimer = new(_ => InsertData(GetNetworkInterfaceInfos()) , null, 0, CommandIntervalConfig.NetworkInterfaceInfo);
            NetworkTrafficInfoTimer = new(_ => InsertData(GetNetworkTrafficInfos()) , null, 0, CommandIntervalConfig.NetworkTrafficInfo);
            TemperatureInfoTimer = new(_ => InsertData(GetTemperatureInfos()) , null, 0, CommandIntervalConfig.TemperatureInfo);
            DiskInfoTimer = new(_ => InsertData(GetDiskInfos()) , null, 0, CommandIntervalConfig.DiskInfo);
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

        public virtual DiskMountInfo[] GetDiskMountInfo()
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

        public virtual DiskInfo[] GetDiskInfos()
        {
            throw new NotImplementedException();
        }

        public void InsertData(object data)
        {
            if (data == null || data is not Array arr || arr.Length == 0)
            {
                return;
            }
            var entityType = data.GetType();
            if (data is Array)
            {
                foreach(var item in arr)
                {
                    entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .FirstOrDefault(x => x.Name == "DateTime")?
                        .SetValue(item, DateTime.Now);
                }
            }
            else
            {
                entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(x => x.Name == "DateTime")?
                    .SetValue(data, DateTime.Now);
            }
            using var db = DBHelper.GetInstance();
            MethodInfo method = typeof(LiteDatabase).GetMethod("GetCollection", [typeof(string)]);
            MethodInfo generic = method.MakeGenericMethod(entityType);
            object collection = generic.Invoke(db, [entityType.Name]);
            var insertMethod = collection.GetType().GetMethod("Insert");
            if (insertMethod != null)
            {
                insertMethod.Invoke(collection, [data]);
            }
            else
            {
                throw new Exception($"Insert method not found for type {entityType.Name}");
            }
        }
    }
}