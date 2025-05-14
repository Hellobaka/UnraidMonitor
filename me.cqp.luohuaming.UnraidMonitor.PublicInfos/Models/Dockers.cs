using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class Dockers
    {
        public string ID { get; set; } = "";

        public string Name { get; set; }

        public string Image { get; set; }

        public string? Icon { get; set; }

        public bool Running { get; set; }

        public static Dockers[] ParseFromDockerPs(string input)
        {
            List<Dockers> containers = [];
            var lines = input.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                // 跳过表头行
                if (line.StartsWith("CONTAINER ID"))
                {
                    continue;
                }

                // 分割列（处理两个以上空格作为分隔符）
                var parts = Regex.Split(line.Trim(), @"\s{2,}");

                if (parts.Length >= 7)
                {
                    containers.Add(new Dockers
                    {
                        ID = parts[0],
                        Image = parts[1],
                        Running = !parts[4].Contains("Exit"),
                        Name = parts[6]
                    });
                }
            }

            return containers.ToArray();
        }

        public void ParseIcon(string input)
        {
            try
            {
                Icon = JArray.Parse(input)[0]["Config"]["Labels"]["net.unraid.docker.icon"]?.ToString();
            }
            catch { }
        }
    }
}