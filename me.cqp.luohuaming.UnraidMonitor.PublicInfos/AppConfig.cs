using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Handler;
using System;
using System.Collections.Generic;
using System.IO;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos
{
    public class AppConfig : ConfigBase
    {
        public AppConfig(string path)
            : base(path)
        {
            LoadConfig();
            Instance = this;
        }

        public static AppConfig Instance { get; private set; }

        public static string CommandStatus { get; set; } = "";

        public static string MonitorOSType { get; set; } = "";

        public static string SSHHost { get; set; } = "";
      
        public static int SSHPort { get; set; } = 22;
      
        public static string SSHUserName { get; set; } = "";
      
        public static string SSHPassword { get; set; } = "";
      
        public static int SSHCommandTimeout { get; set; } = 10;
      
        public static int CacheKeepSeconds { get; set; } = (int)TimeSpan.FromDays(1).TotalSeconds;
       
        public static string FallbackFont { get; set; }

        public static List<long> GroupList { get; set; } = new List<long>();

        public static List<long> BlackList { get; set; } = new List<long>();

        public static List<long> PersonList { get; set; } = new List<long>();

        public override void LoadConfig()
        {
            DisableAutoReload();
            CommandStatus = GetConfig("CommandStatus", "系统状态");
            MonitorOSType = GetConfig("MonitorOSType", "Linux");
            SSHHost = GetConfig("SSHHost", "");
            SSHPort = GetConfig("SSHPort", 22);
            SSHUserName = GetConfig("SSHUserName", "");
            SSHPassword = GetConfig("SSHPassword", "");
            SSHCommandTimeout = GetConfig("SSHCommandTimeout", 10);
            FallbackFont = GetConfig("FallbackFont", "微软雅黑");
            CacheKeepSeconds = GetConfig("CacheKeepSeconds", (int)TimeSpan.FromDays(1).TotalSeconds);
            GroupList = GetConfig("GroupList", new List<long>());
            PersonList = GetConfig("PersonList", new List<long>());
            BlackList = GetConfig("BlackList", new List<long>());
            EnableAutoReload();
        }
    }

    public class CommandIntervalConfig : ConfigBase
    {
        public CommandIntervalConfig(string path)
            : base(path)
        {
            LoadConfig();
            Instance = this;
        }

        public static CommandIntervalConfig Instance { get; private set; }

        public static int CpuInfo { get; set; } = 0;

        public static int CpuUsages { get; set; } = 1000;

        public static int DiskMountInfos { get; set; } = 0;

        public static int Dockers { get; set; } = 10000;

        public static int FanInfos { get; set; } = 1000;

        public static int MemoryInfo { get; set; } = 1000;

        public static int MotherboardInfo { get; set; } = 0;

        public static int NetworkInterfaceInfos { get; set; } = 0;

        public static int NetworkTrafficInfos { get; set; } = 1000;

        public static int TemperatureInfos { get; set; } = 5000;

        public static int VirtualMachines { get; set; } = 10000;

        public static int DiskSmartInfos { get; set; } = -1;

        public static int DiskInfos { get; set; } = 10000;

        public static int SystemInfo { get; set; } = 0;

        public static int SystemUptime { get; set; } = 10000;

        public static int UPS { get; set; } = 5000;

        public override void LoadConfig()
        {
            DisableAutoReload();

            CpuInfo = GetConfig("CpuInfo", 0);
            CpuUsages = GetConfig("CpuUsages", 1000);
            DiskMountInfos = GetConfig("DiskMountInfos", 0);
            Dockers = GetConfig("Dockers", 10000);
            FanInfos = GetConfig("FanInfos", 1000);
            MemoryInfo = GetConfig("MemoryInfo", 1000);
            MotherboardInfo = GetConfig("MotherboardInfo", 0);
            NetworkInterfaceInfos = GetConfig("NetworkInterfaceInfos", 0);
            NetworkTrafficInfos = GetConfig("NetworkTrafficInfos", 1000);
            TemperatureInfos = GetConfig("TemperatureInfos", 5000);
            VirtualMachines = GetConfig("VirtualMachines", 10000);
            DiskInfos = GetConfig("DiskInfos", 10000);
            DiskSmartInfos = GetConfig("DiskSmartInfos", -1);
            SystemUptime = GetConfig("SystemUptime", 10000);
            SystemInfo = GetConfig("SystemInfo", 0);
            UPS = GetConfig("UPS", 5000);

            EnableAutoReload();
            HandlerBase.Instance?.StopMonitor();
            HandlerBase.Instance?.StartMonitor();
        }
    }
}