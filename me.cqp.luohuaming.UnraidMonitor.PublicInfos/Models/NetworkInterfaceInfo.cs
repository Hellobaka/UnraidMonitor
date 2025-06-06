using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class NetworkInterfaceInfo : MonitorDataBase
    {
        public List<string> IpAddresses { get; set; } = [];

        public List<string> Ipv6Addresses { get; set; } = [];

        public string MacAddress { get; set; } = "";

        public string Name { get; set; } = "";

        private static Regex InterfaceRegex { get; } = new(@"^\d+:\s+([^:]+):", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex IPv4Regex { get; } = new(@"inet\s+([0-9.]+)(?:/\d+)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex IPv6Regex { get; } = new(@"inet6\s+([0-9a-fA-F:]+)(?:/\d+)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex MACRegex { get; } = new(@"link/(?:ether|loopback)\s+([0-9a-fA-F:]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static NetworkInterfaceInfo[] ParseFromIPA(string input)
        {
            var interfaces = new List<NetworkInterfaceInfo>();
            NetworkInterfaceInfo currentInterface = null;

            foreach (var line in input.Split('\n'))
            {
                var interfaceMatch = InterfaceRegex.Match(line);
                if (interfaceMatch.Success)
                {
                    currentInterface = new NetworkInterfaceInfo
                    {
                        Name = interfaceMatch.Groups[1].Value.Trim(),
                        DateTime = DateTime.Now
                    };
                    interfaces.Add(currentInterface);
                    continue;
                }

                if (currentInterface == null)
                {
                    continue;
                }

                var macMatch = MACRegex.Match(line);
                if (macMatch.Success)
                {
                    currentInterface.MacAddress = macMatch.Groups[1].Value.Trim();
                    continue;
                }

                var ipv4Match = IPv4Regex.Match(line);
                if (ipv4Match.Success)
                {
                    currentInterface.IpAddresses.Add(ipv4Match.Groups[1].Value.Trim());
                    continue;
                }

                var ipv6Match = IPv6Regex.Match(line);
                if (ipv6Match.Success)
                {
                    currentInterface.Ipv6Addresses.Add(ipv6Match.Groups[1].Value.Trim());
                }
            }

            return interfaces.ToArray();
        }
    }
}