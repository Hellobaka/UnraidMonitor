using System;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class SystemUptime
    {
        private static Regex UptimeRegexA { get; } = new(@"up\s+(\d+)\s+days?,\s+(\d+):(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex UptimeRegexB { get; } = new(@"up\s+(\d+):(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static TimeSpan ParseFromUptime(string input)
        {
            var matchA = UptimeRegexA.Match(input);
            var matchB = UptimeRegexB.Match(input);
            if (matchA.Success)
            {
                int days = int.Parse(matchA.Groups[1].Value);
                int hours = int.Parse(matchA.Groups[2].Value);
                int minutes = int.Parse(matchA.Groups[3].Value);

                return new TimeSpan(days, hours, minutes, 0);
            }
            else if(matchB.Success)
            {
                int hours = int.Parse(matchB.Groups[1].Value);
                int minutes = int.Parse(matchB.Groups[2].Value);

                return new TimeSpan(0, hours, minutes, 0);
            }

            return TimeSpan.Zero;
        }
    }
}
