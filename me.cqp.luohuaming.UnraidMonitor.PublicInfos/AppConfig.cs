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

        public static string SSHHost { get; set; } = "";
      
        public static int SSHPort { get; set; } = 22;
      
        public static string SSHUserName { get; set; } = "";
      
        public static string SSHPassword { get; set; } = "";
      
        public static int SSHCommandTimeout { get; set; } = 10;
       
        public static string CustomFont { get; set; }
      
        public static string CustomFontPath { get; set; }

        public override void LoadConfig()
        {
            DisableAutoReload();
            CommandStatus = GetConfig("CommandStatus", "系统状态");
            SSHHost = GetConfig("SSHHost", "");
            SSHPort = GetConfig("SSHPort", 22);
            SSHUserName = GetConfig("SSHUserName", "");
            SSHPassword = GetConfig("SSHPassword", "");
            SSHCommandTimeout = GetConfig("SSHCommandTimeout", 10);
            CustomFont = GetConfig("CustomFont", "");
            CustomFontPath = GetConfig("CustomFontPath", "");
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

        public static int CPUInfo { get; set; } = 0;

        public static int CPUUsage { get; set; } = 1000;

        public static int DiskMountInfo { get; set; } = 0;

        public static int Dockers { get; set; } = 10000;

        public static int FanInfo { get; set; } = 1000;

        public static int MemoryInfo { get; set; } = 1000;

        public static int MotherboardInfo { get; set; } = 0;

        public static int NetworkInterfaceInfo { get; set; } = 0;

        public static int NetworkTrafficInfo { get; set; } = 1000;

        public static int TemperatureInfo { get; set; } = 5000;

        public static int VirtualMachine { get; set; } = 10000;

        public static int DiskInfo { get; set; } = 10000;

        public override void LoadConfig()
        {
            DisableAutoReload();

            CPUInfo = GetConfig("CPUInfo", 0);
            CPUUsage = GetConfig("CPUUsage", 1000);
            DiskMountInfo = GetConfig("DiskMountInfo", 0);
            Dockers = GetConfig("Dockers", 10000);
            FanInfo = GetConfig("FanInfo", 1000);
            MemoryInfo = GetConfig("MemoryInfo", 1000);
            MotherboardInfo = GetConfig("MotherboardInfo", 0);
            NetworkInterfaceInfo = GetConfig("NetworkInterfaceInfo", 0);
            NetworkTrafficInfo = GetConfig("NetworkTrafficInfo", 1000);
            TemperatureInfo = GetConfig("TemperatureInfo", 5000);
            VirtualMachine = GetConfig("VirtualMachine", 10000);
            DiskInfo = GetConfig("DiskInfo", 10000);

            EnableAutoReload();
            HandlerBase.Instance?.StopMonitor();
            HandlerBase.Instance?.StartMonitor();
        }
    }
}