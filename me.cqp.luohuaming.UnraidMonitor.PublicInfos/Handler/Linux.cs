using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Handler
{
    public class Linux : HandlerBase
    {
        private SshCommandQueue SshCommand { get; set; } = new(AppConfig.SSHHost, AppConfig.SSHPort, AppConfig.SSHUserName, AppConfig.SSHPassword);

        private DiskMountInfo[] CacheDiskMountInfo { get; set; } = null;

        public Linux()
            : base()
        {
        }

        public override CpuInfo GetCpuInfo()
        {
            string command = "dmidecode -t processor";
            var (error, output) = SshCommand.EnqueueCommand(command).Result;
            if (string.IsNullOrEmpty(output))
            {
                MainSave.CQLog?.Error("获取CPUInfo", $"指令执行失败：{error}");
                return null;
            }
            var item = CpuInfo.ParseDmidecode(output);
            item.CacheItem();
            return item;
        }

        public override CpuUsage[] GetCpuUsages()
        {
            string command = "COLUMNS=200 TERM=dumb top -1 -n 1 -b";
            var (error, output) = SshCommand.EnqueueCommand(command).Result;
            if (string.IsNullOrEmpty(output))
            {
                MainSave.CQLog?.Error("获取CpuUsage", $"指令执行失败：{error}");
                return null;
            }
            var usages = CpuUsage.ParseFromTop(output);
            var total = new CpuUsage()
            {
                CPUId = "CPUTotal",
                User = CpuUsage.GetTotalUsage(usages),
            };
            CpuUsage[] items = [total, .. usages];
            foreach (var item in items)
            {
                item.CacheItem();
            }
            return items;
        }

        public override DiskMountInfo[] GetDiskMountInfos()
        {
            string command = "lsblk";
            var (error, output) = SshCommand.EnqueueCommand(command).Result;
            if (string.IsNullOrEmpty(output))
            {
                MainSave.CQLog?.Error("获取DiskMountInfo", $"指令执行失败：{error}");
                return null;
            }
            var array = DiskMountInfo.ParseFromLsblk(output);
            CacheDiskMountInfo = array;

            command = $"df";
            (error, output) = SshCommand.EnqueueCommand(command).Result;
            if (string.IsNullOrEmpty(output))
            {
                MainSave.CQLog?.Warning("获取磁盘空间", $"指令执行失败：{error}");
                return array;
            }
            int i = 1;
            foreach (var item in array)
            {
                if (item.Type != "disk" || item.MountPoint == "/boot")
                {
                    continue;
                }
                string diskName = $"/dev/{item.Name}";
                if (item.Name.StartsWith("sd"))
                {
                    diskName = $"/dev/md{i}p1";
                    i++;
                }
                item.ParseDiskFree(output, diskName);
                item.CacheItem();
            }

            return array;
        }

        public override Dockers[] GetDockers()
        {
            string command = "docker ps --all --no-trunc --format='{{json .}}' | jq -s";
            var (error, output) = SshCommand.EnqueueCommand(command).Result;
            if (string.IsNullOrEmpty(output))
            {
                MainSave.CQLog?.Error("获取Dockers", $"指令执行失败：{error}");
                return null;
            }
            var items = Dockers.ParseFromDockerPs(output);
            foreach (var item in items)
            {
                item.CacheItem();
            }
            return items;
        }

        public override FanInfo[] GetFanInfos()
        {
            string command = "sensors";
            var (error, output) = SshCommand.EnqueueCommand(command).Result;
            if (string.IsNullOrEmpty(output))
            {
                MainSave.CQLog?.Error("获取FanInfos", $"指令执行失败：{error}");
                return null;
            }
            var items = FanInfo.ParseFromSensor(output);
            foreach (var item in items)
            {
                item.CacheItem();
            }
            return items;
        }

        public override MemoryInfo GetMemoryInfo()
        {
            string command = "free";
            var (error, output) = SshCommand.EnqueueCommand(command).Result;
            if (string.IsNullOrEmpty(output))
            {
                MainSave.CQLog?.Error("获取MemoryInfo", $"指令执行失败：{error}");
                return null;
            }
            var item = MemoryInfo.ParseFromFree(output);
            item.CacheItem();
            return item;
        }

        public override MotherboardInfo GetMotherboardInfo()
        {
            string command = "dmidecode -t baseboard";
            var (error, output) = SshCommand.EnqueueCommand(command).Result;
            if (string.IsNullOrEmpty(output))
            {
                MainSave.CQLog?.Error("获取MotherboardInfo", $"指令执行失败：{error}");
                return null;
            }
            var item = MotherboardInfo.ParseDmidecode(output);
            item.CacheItem();
            return item;
        }

        public override NetworkInterfaceInfo[] GetNetworkInterfaceInfos()
        {
            string command = "ip a";
            var (error, output) = SshCommand.EnqueueCommand(command).Result;
            if (string.IsNullOrEmpty(output))
            {
                MainSave.CQLog?.Error("获取NetworkInterfaceInfos", $"指令执行失败：{error}");
                return null;
            }
            var items = NetworkInterfaceInfo.ParseFromIPA(output);
            foreach (var item in items)
            {
                item.CacheItem();
            }
            return items;
        }

        public override NetworkTrafficInfo[] GetNetworkTrafficInfos()
        {
            string command = "ip -s link show";
            var (error, output) = SshCommand.EnqueueCommand(command).Result;
            if (string.IsNullOrEmpty(output))
            {
                MainSave.CQLog?.Error("获取NetworkTrafficInfos", $"指令执行失败：{error}");
                return null;
            }
            var items = NetworkTrafficInfo.ParseFromIPS(output);
            foreach (var item in items)
            {
                item.CacheItem();
            }
            return items;
        }

        public override TemperatureInfo[] GetTemperatureInfos()
        {
            string command = "sensors";
            var (error, output) = SshCommand.EnqueueCommand(command).Result;
            if (string.IsNullOrEmpty(output))
            {
                MainSave.CQLog?.Error("获取TemperatureInfos", $"指令执行失败：{error}");
                return null;
            }

            var items = TemperatureInfo.ParseFromSensor(output);
            foreach (var item in items)
            {
                item.CacheItem();
            }
            return items;
        }

        public override DiskSmartInfo[] GetDiskSmartInfos()
        {
            if (CacheDiskMountInfo == null || CacheDiskMountInfo.Length == 0)
            {
                GetDiskMountInfos();
            }

            if (CacheDiskMountInfo == null || CacheDiskMountInfo.Length == 0)
            {
                return [];
            }
            List<DiskSmartInfo> list = [];
            foreach (var item in CacheDiskMountInfo)
            {
                if (item.Type != "disk" || item.MountPoint == "/boot")
                {
                    continue;
                }

                string command = $"smartctl -a /dev/{item.Name}";
                var (error, output) = SshCommand.EnqueueCommand(command).Result;
                if (string.IsNullOrEmpty(output))
                {
                    MainSave.CQLog?.Error("获取SMART", $"指令执行失败：{error}");
                    continue;
                }

                var info = DiskSmartInfo.ParseSmartctl(output);
                if (info != null)
                {
                    list.Add(info);
                    info.CacheItem();
                }
            }

            return list.ToArray();
        }

        public override VirtualMachine[] GetVirtualMachines()
        {
            string command = "virsh list --all";
            var (error, output) = SshCommand.EnqueueCommand(command).Result;
            if (string.IsNullOrEmpty(output))
            {
                MainSave.CQLog?.Error("获取VirtualMachines", $"指令执行失败：{error}");
                return null;
            }
            var result = VirtualMachine.ParseFromVirsh(output);
            foreach (var item in result)
            {
                if (item.Running)
                {
                    command = $"virsh domifaddr \"{item.Name}\" --source agent";
                    (error, output) = SshCommand.EnqueueCommand(command).Result;
                    if (string.IsNullOrEmpty(output))
                    {
                        MainSave.CQLog?.Warning("获取VirtualMachines IP", $"指令执行失败：{error}");
                        continue;
                    }
                    item.ParseIPs(output);
                }

                command = $"virsh dumpxml \"{item.Name}\"";
                (error, output) = SshCommand.EnqueueCommand(command).Result;
                if (string.IsNullOrEmpty(output))
                {
                    MainSave.CQLog?.Warning("获取VirtualMachines Icon", $"指令执行失败：{error}");
                    continue;
                }
                item.ParseIcon(output);
                item.CacheItem();
            }

            return result;
        }

        public override SystemInfo GetSystemInfo()
        {
            string command = "cat /var/local/emhttp/var.ini";
            var (error, output) = SshCommand.EnqueueCommand(command).Result;
            if (string.IsNullOrEmpty(output))
            {
                MainSave.CQLog?.Error("获取SystemInfo", $"指令执行失败：{error}");
                return null;
            }

            var item = SystemInfo.ParseFromUnraidIni(output);
            item.CacheItem();
            return item;
        }

        public override SystemUptime GetSystemUptime()
        {
            string command = "uptime";
            var (error, output) = SshCommand.EnqueueCommand(command).Result;
            if (string.IsNullOrEmpty(output))
            {
                MainSave.CQLog?.Error("获取SystemInfo", $"指令执行失败：{error}");
                return new();
            }

            var item = SystemUptime.ParseFromUptime(output);
            item.CacheItem();
            return item;
        }

        public override DiskInfo[] GetDiskInfos()
        {
            string command = "cat /var/local/emhttp/disks.ini";
            var (error, output) = SshCommand.EnqueueCommand(command).Result;
            if (string.IsNullOrEmpty(output))
            {
                MainSave.CQLog?.Error("获取DiskInfo", $"指令执行失败：{error}");
                return [];
            }

            var items = DiskInfo.ParseFromDiskIni(output);
            foreach (var item in items)
            {
                item.CacheItem();
            }
            return items;
        }

        public override UPSStatus GetUPS()
        {
            string command = "/sbin/apcaccess";
            var (error, output) = SshCommand.EnqueueCommand(command).Result;
            if (string.IsNullOrEmpty(output))
            {
                MainSave.CQLog?.Error("获取UPSStatus", $"指令执行失败：{error}");
                return null;
            }

            var item = UPSStatus.ParseFromApcAccess(output);
            item.CacheItem();
            return item;
        }

        public override Notification[] GetNotifications()
        {
            string command = "ls /tmp/notifications/unread";
            var (error, output) = SshCommand.EnqueueCommand(command).Result;
            if (string.IsNullOrEmpty(output))
            {
                MainSave.CQLog?.Error("获取Notification", $"指令执行失败：{error}");
                return [];
            }
            List<Notification> list = [];
            foreach (var item in output.Replace("\r", " ").Replace("\n", " ").Split([' '], StringSplitOptions.RemoveEmptyEntries))
            {
                command = $"cat /tmp/notifications/unread/{item}";
                (error, output) = SshCommand.EnqueueCommand(command).Result;
                if (string.IsNullOrEmpty(output))
                {
                    MainSave.CQLog?.Error("获取Notification", $"指令执行失败：{error}");
                    continue;
                }
                Notification notification = Notification.ParseFromUnreadFile(output);
                if (notification != null)
                {
                    list.Add(notification);
                    notification.CacheItem();
                }
            }
            return list.ToArray();
        }
    }
}
