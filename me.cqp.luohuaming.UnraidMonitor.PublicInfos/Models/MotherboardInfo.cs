using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class MotherboardInfo
    {
        public string Manufacturer { get; set; } = "";

        public string ProductName { get; set; } = "";

        private static Regex ManufacturerRegex { get; } = new(@"Manufacturer:\s*(.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex ProductNameRegex { get; } = new(@"Product Name:\s*(.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static MotherboardInfo ParseDmidecode(string input)
        {
            var info = new MotherboardInfo();

            var manufacturerMatch = ManufacturerRegex.Match(input);
            var productNameMatch = ProductNameRegex.Match(input);

            if (manufacturerMatch.Success)
            {
                info.Manufacturer = manufacturerMatch.Groups[1].Value.Trim();
            }

            if (productNameMatch.Success)
            {
                info.ProductName = productNameMatch.Groups[1].Value.Trim();
            }

            return info;
        }
    }
}