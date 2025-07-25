﻿using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class MotherboardInfo : MonitorDataBase
    {
        [Description("制造商")]
        public string Manufacturer { get; set; } = "";

        [Description("型号")]
        public string ProductName { get; set; } = "";

        private static Regex ManufacturerRegex { get; } = new(@"Manufacturer:\s*(.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex ProductNameRegex { get; } = new(@"Product Name:\s*(.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static MotherboardInfo ParseDmidecode(string input)
        {
            var info = new MotherboardInfo();
            info.DateTime = DateTime.Now;
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