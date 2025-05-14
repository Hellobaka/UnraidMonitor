using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class SystemInfo
    {
        public string Version { get; set; }

        public string SystemName { get; set; }

        public string SystemEditon { get; set; }

        /// <summary>
        /// /var/local/emhttp/var.ini
        /// </summary>
        /// <param name="input"></param>
        public static SystemInfo ParseFromUnraidIni(string input)
        {
            var lines = input.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
            SystemInfo info = new();
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
                    info.SystemEditon = parts.Last().Trim();
                }
            }

            return info;
        }
    }
}
