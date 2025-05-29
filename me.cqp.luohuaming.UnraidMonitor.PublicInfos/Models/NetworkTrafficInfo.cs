using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class NetworkTrafficInfo : MonitorDataBase
    {
        public string Name { get; set; } = "";

        public long RxBytes { get; set; }

        public long TxBytes { get; set; }

        private static Regex InterfaceBlockRegex { get; } = new(@"^\d+:\s*([^\s:]+):.*?(?=^\d+:|\z)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);

        private static Regex RxBytesRegex { get; } = new(@"RX:\s+[^\n]*\n\s*([0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex TxBytesRegex { get; } = new(@"TX:\s+[^\n]*\n\s*([0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static NetworkTrafficInfo[] ParseFromIPS(string input)
        {
            var list = new List<NetworkTrafficInfo>();
            foreach (Match m in InterfaceBlockRegex.Matches(input))
            {
                var name = m.Groups[1].Value.Trim();
                var block = m.Value;

                var rxMatch = RxBytesRegex.Match(block);
                var txMatch = TxBytesRegex.Match(block);

                var rx = rxMatch.Success ? rxMatch.Groups[1].Value.Trim() : "0";
                var tx = txMatch.Success ? txMatch.Groups[1].Value.Trim() : "0";

                list.Add(new NetworkTrafficInfo
                {
                    Name = name,
                    RxBytes = long.TryParse(rx , out long l) ? l : 0,
                    TxBytes = long.TryParse(tx, out l) ? l : 0,
                    DateTime = DateTime.Now
                });
            }
            return list.ToArray();
        }
    }
}