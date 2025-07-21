using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class VirtualMachine : MonitorDataBase
    {
        [Description("虚拟机助记名称")]
        public string Name { get; set; }

        [Description("运行状态")]
        public bool Running { get; set; }

        /// <summary>
        /// name与mac可能为'-'
        /// </summary>
        [Description("IP 地址")]
        public List<(string name, string mac, IPAddress ip)> Networks { get; set; } = [];

        [Description("虚拟机图标")]
        public string? Icon { get; set; }

        public static VirtualMachine[] ParseFromVirsh(string input)
        {
            var result = new List<VirtualMachine>();
            var lines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                // 跳过表头和分隔线
                if (line.Trim().StartsWith("Id") || line.Trim().StartsWith("---"))
                    continue;

                // 用2个及以上空格分割
                var parts = Regex.Split(line.Trim(), @"\s{2,}");
                if (parts.Length == 3)
                {
                    result.Add(new VirtualMachine
                    {
                        Name = parts[1].Trim(),
                        Running = parts[2].Trim() == "running",
                        DateTime = DateTime.Now
                    });
                }
            }
            return result.ToArray();
        }

        public void ParseIPs(string input)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return;
            }
            Networks = [];
            var lines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("---"))
                {
                    continue;
                }
                var parts = Regex.Split(line.Trim(), @"\s{2,}");
                if (parts.Length != 4)
                {
                    continue;
                }
                if (parts[0] == "Name")
                {
                    continue;
                }
                if (IPAddress.TryParse(parts[3].Split('/').First(), out var ip))
                {
                    Networks.Add((parts[0], parts[1], ip));
                }
            }
        }

        public void ParseIcon(string input)
        {
            foreach (var line in input.Split('\n'))
            {
                if (line.Contains("vmtemplate") && line.Contains("icon="))
                {
                    Icon = line.Split([' '], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(x => x.StartsWith("icon=")).Split('=').Last();
                    break;
                }
            }
        }
    }
}
