using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class VirtualMachine
    {
        public string Name { get; set; }

        public bool Running { get; set; }

        /// <summary>
        /// name与mac可能为'-'
        /// </summary>
        public List<(string name, string mac, IPAddress ip)> Networks { get; set; } = [];

        public string? Icon { get; set; }

        public DateTime DateTime { get; set; }

        public static VirtualMachine[] ParseFromVirsh(string input)
        {
            var lines = input.Split('\n');
            var vms = new List<VirtualMachine>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("---"))
                {
                    continue;
                }
                var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    continue;
                }
                if (parts[0] == "Id")
                {
                    continue;
                }
                var vm = new VirtualMachine
                {
                    Name = parts[1],
                    Running = parts[2].Equals("running", StringComparison.OrdinalIgnoreCase),
                };
                vms.Add(vm);
            }
            return vms.ToArray();
        }

        public void ParseIPs(string input)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return;
            }
            Networks = [];
            foreach (var line in input.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("---"))
                {
                    continue;
                }
                var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    continue;
                }
                if (parts[0] == "Name")
                {
                    continue;
                }
                if (IPAddress.TryParse(parts[3], out var ip))
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
