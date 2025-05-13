using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class CpuInfo
    {
        public int BaseSpeedMHz { get; set; }

        public int LogicalCores { get; set; }

        public int MaxTurboSpeedMHz { get; set; }

        public string Model { get; set; } = "";

        public int PhysicalCores { get; set; }

        private static Regex BaseSpeedRegex { get; } = new(@"Current Speed:\s*(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex CoreRegex { get; } = new(@"Core Count:\s*(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex LogicCoreRegex { get; } = new(@"Thread Count:\s*(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex MaxSpeedRegex { get; } = new(@"Max Speed:\s*(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex ModelRegex { get; } = new(@"Version:\s*(.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static CpuInfo ParseDmidecode(string input)
        {
            var info = new CpuInfo();

            var modelMatch = ModelRegex.Match(input);
            if (modelMatch.Success)
            {
                info.Model = modelMatch.Groups[1].Value.Trim();
            }

            var coreMatch = CoreRegex.Match(input);
            if (coreMatch.Success)
            {
                info.PhysicalCores = int.Parse(coreMatch.Groups[1].Value);
            }

            var threadMatch = LogicCoreRegex.Match(input);
            if (threadMatch.Success)
            {
                info.LogicalCores = int.Parse(threadMatch.Groups[1].Value);
            }

            var baseSpeedMatch = BaseSpeedRegex.Match(input);
            if (baseSpeedMatch.Success)
            {
                info.BaseSpeedMHz = int.Parse(baseSpeedMatch.Groups[1].Value);
            }

            var maxSpeedMatch = MaxSpeedRegex.Match(input);
            if (maxSpeedMatch.Success)
            {
                info.MaxTurboSpeedMHz = int.Parse(maxSpeedMatch.Groups[1].Value);
            }

            return info;
        }
    }
}