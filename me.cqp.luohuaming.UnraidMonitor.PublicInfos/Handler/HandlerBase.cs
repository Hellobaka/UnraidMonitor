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
                string key = property.Name; // 例如 "CPUInfo"
                if (key == "Instance")
                {
                    continue;
                }
                int interval = (int)property.GetValue(null);

                string timerFieldName = key + "Timer";       // 例如 "CPUInfoTimer"
                string methodName = "Get" + key;             // 例如 "GetCPUInfo"

                var t = instanceType.GetProperty(timerFieldName);
                var collectMethod = instanceType.GetMethod(methodName);

                if (t == null || collectMethod == null)
                {
                    Debugger.Break();
                    continue;
                }

                (t.GetValue(this) as Timer)?.Dispose();

                if (interval > 0)
                {
                    Timer timer = new(_ =>
                    {
                        var data = collectMethod.Invoke(this, null);
                        this.InsertData(data);
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
            var db = DBHelper.Instance;
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