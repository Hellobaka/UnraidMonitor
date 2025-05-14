using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class Dockers
    {
        public string ID { get; set; } = "";

        public string Name { get; set; }

        public string Image { get; set; }

        public bool Running { get; set; }

        public DateTime DateTime { get; set; }

        public static Dockers[] ParseFromDockerPs(string input)
        {
            List<Dockers> containers = [];
            try
            {
                JArray json = JArray.Parse(input);
                foreach (var item in json)
                {
                    containers.Add(new Dockers
                    {
                        ID = item["ID"].ToString(),
                        Name = item["Names"][0].ToString().TrimStart('/'),
                        Image = item["Labels"].ToString()?.Split(',').FirstOrDefault(x => x.Split('=')[0] == "net.unraid.docker.icon")?.Split('=').Last(),
                        Running = item["State"].ToString() == "running",
                        DateTime = DateTime.Now
                    });
                }
            }
            catch (Exception)
            {
                MainSave.CQLog.Error("获取Docker", $"Json解析失败: {input}");
            }
            return containers.ToArray();
        }
    }
}