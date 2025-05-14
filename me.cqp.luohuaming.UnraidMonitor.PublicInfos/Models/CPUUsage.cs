using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class CpuUsage
    {
        public string CPUId { get; set; }

        public double HardwareInterrupt { get; set; }

        public double Idle { get; set; }

        public double Nice { get; set; }

        public double SoftwareInterrupt { get; set; }

        public double Steal { get; set; }

        public double System { get; set; }

        public double TotalUsage => User + System + Nice + Wait + HardwareInterrupt + SoftwareInterrupt + Steal;

        public double User { get; set; }

        public double Wait { get; set; }

        public DateTime DateTime { get; set; }

        private static Regex CPUParseRegex { get; } = new(@"%Cpu(\d+)\s*:\s*([\d.]+)\s*us,\s*([\d.]+)\s*sy,\s*([\d.]+)\s*ni,\s*([\d.]+)\s*id,\s*([\d.]+)\s*wa,\s*([\d.]+)\s*hi,\s*([\d.]+)\s*si,\s*([\d.]+)\s*st", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static double GetTotalUsage(CpuUsage[] cpus)
        {
            if (cpus == null || cpus.Length == 0)
            {
                return 0;
            }

            return cpus.Sum(x => x.TotalUsage) / cpus.Length;
        }

        public static CpuUsage[] ParseFromTop(string input)
        {
            var cpuUsages = new List<CpuUsage>();
            var regex = CPUParseRegex;

            foreach (var line in input.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries))
            {
                var matches = regex.Matches(line);

                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        int cpuId = int.Parse(match.Groups[1].Value);
                        var usage = new CpuUsage
                        {
                            CPUId = $"CPU{cpuId}",
                            User = double.Parse(match.Groups[2].Value),
                            System = double.Parse(match.Groups[3].Value),
                            Nice = double.Parse(match.Groups[4].Value),
                            Idle = double.Parse(match.Groups[5].Value),
                            Wait = double.Parse(match.Groups[6].Value),
                            HardwareInterrupt = double.Parse(match.Groups[7].Value),
                            SoftwareInterrupt = double.Parse(match.Groups[8].Value),
                            Steal = double.Parse(match.Groups[9].Value)
                        };

                        cpuUsages.Add(usage);
                    }
                }
            }

            return cpuUsages.ToArray();
        }

        public override string ToString()
        {
            return $"User: {User}%, System: {System}%, Nice: {Nice}%, Idle: {Idle}%, Wait: {Wait}%, HI: {HardwareInterrupt}%, SI: {SoftwareInterrupt}%, Steal: {Steal}%";
        }
    }
}