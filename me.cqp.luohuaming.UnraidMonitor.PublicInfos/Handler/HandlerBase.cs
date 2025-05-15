using LiteDB;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Handler
{
    public class HandlerBase
    {
        public HandlerBase()
        {
            Instance = this;
        }

        public static HandlerBase Instance { get; set; }

        private Timer CpuInfoTimer { get; set; }

        private Timer CpuUsagesTimer { get; set; }

        private Timer MemoryInfoTimer { get; set; }

        private Timer MotherboardInfoTimer { get; set; }

        private Timer DiskMountInfoTimer { get; set; }

        private Timer DockersTimer { get; set; }

        private Timer VirtualMachinesTimer { get; set; }

        private Timer FanInfosTimer { get; set; }

        private Timer NetworkInterfaceInfosTimer { get; set; }

        private Timer NetworkTrafficInfoTsimer { get; set; }

        private Timer TemperatureInfosTimer { get; set; }

        private Timer DiskInfoTimer { get; set; }

        private Timer DiskSmartInfoTimer { get; set; }

        private Timer SystemInfosTimer { get; set; }

        private Timer SystemUptimeTimer { get; set; }

        private Timer UPSTimer { get; set; }

        public void StopMonitor()
        {
            var configType = typeof(CommandIntervalConfig);
            var fields = configType.GetFields(BindingFlags.Public | BindingFlags.Static);

            var instanceType = this.GetType();

            foreach (var field in fields)
            {
                string key = field.Name;
                string timerFieldName = key + "Timer";

                var timerField = instanceType.GetField(timerFieldName, BindingFlags.NonPublic | BindingFlags.Instance);

                if (timerField != null)
                {
                    (timerField.GetValue(this) as Timer)?.Dispose();
                    timerField.SetValue(this, null);
                }
            }
        }

        public void StartMonitor()
        {
            var configType = typeof(CommandIntervalConfig);
            var fields = configType.GetFields(BindingFlags.Public | BindingFlags.Static);

            var instanceType = this.GetType();

            foreach (var field in fields)
            {
                string key = field.Name; // 例如 "CPUInfo"
                int interval = (int)field.GetValue(null);

                string timerFieldName = key + "Timer";       // 例如 "CPUInfoTimer"
                string methodName = "Get" + key;             // 例如 "GetCPUInfo"

                var timerField = instanceType.GetField(timerFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                var collectMethod = instanceType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

                if (timerField == null || collectMethod == null)
                {
                    Debugger.Break();
                    continue;
                }

                (timerField.GetValue(this) as Timer)?.Dispose();

                if (interval > 0)
                {
                    Timer timer = new(_ =>
                    {
                        var data = collectMethod.Invoke(this, null);
                        this.InsertData(data);
                    }, null, 0, interval);

                    timerField.SetValue(this, timer);
                }
                else
                {
                    timerField.SetValue(this, null);
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

        public void InsertData(object data)
        {
            Array array = data as Array;
            if (data == null || (array != null && array.Length == 0))
            {
                return;
            }
            var entityType = data.GetType();
            if (array != null)
            {
                foreach (var item in array)
                {
                    item.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
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
            Type itemType = entityType.IsArray ? entityType.GetElementType() : entityType;
            using var db = DBHelper.GetInstance();
            MethodInfo method = db.GetType().GetMethods().FirstOrDefault(x => x.Name == "GetCollection" && x.IsGenericMethod && x.GetParameters().Length == 0);
            MethodInfo generic = method.MakeGenericMethod(itemType);
            object collection = generic.Invoke(db, []);
            var insertMethod = collection.GetType().GetMethods().FirstOrDefault(x => x.Name == "Insert" && x.ReturnType.Name == "BsonValue");
            if (insertMethod != null)
            {
                if (!entityType.IsArray)
                {
                    insertMethod.Invoke(collection, [data]);
                }
                else
                {
                    foreach (var item in array)
                    {
                        insertMethod.Invoke(collection, [item]);
                    }
                }
            }
            else
            {
                throw new Exception($"Insert method not found for type {entityType.Name}");
            }
        }
    }
}