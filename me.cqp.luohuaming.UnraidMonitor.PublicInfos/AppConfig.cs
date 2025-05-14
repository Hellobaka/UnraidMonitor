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

        public override void LoadConfig()
        {
            CommandStatus = GetConfig("CommandStatus", "系统状态");
            SSHHost = GetConfig("SSHHost", "");
            SSHPort = GetConfig("SSHPort", 22);
            SSHUserName = GetConfig("SSHUserName", "");
            SSHCommandTimeout = GetConfig("SSHCommandTimeout", 10);
        }
    }
}