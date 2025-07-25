﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class SystemInfo : MonitorDataBase
    {
        [Description("系统软件版本")]
        public string Version { get; set; }

        [Description("系统名称")]
        public string SystemName { get; set; }

        [Description("系统版本")]
        public string SystemEdition { get; set; }

        /// <summary>
        /// /var/local/emhttp/var.ini
        /// </summary>
        /// <param name="input"></param>
        public static SystemInfo ParseFromUnraidIni(string input)
        {
            var lines = input.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
            SystemInfo info = new();
            info.DateTime = DateTime.Now;
            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if(parts.Length != 2)
                {
                    continue;
                }
                if(parts.First() == "version")
                {
                    info.Version = parts.Last().Trim();
                }
                if(parts.First() == "NAME")
                {
                    info.SystemName = parts.Last().Trim();
                }
                if(parts.First() == "regTy")
                {
                    info.SystemEdition = parts.Last().Trim();
                }
            }

            return info;
        }
    }
}
