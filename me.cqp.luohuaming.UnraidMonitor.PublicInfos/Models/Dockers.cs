using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class Dockers : MonitorDataBase
    {
        [Description("容器 ID")]
        public string ContainerID { get; set; } = "";

        [Description("容器助记名称")]
        public string Name { get; set; }

        [Description("镜像名称")]
        public string Image { get; set; }

        [Description("容器运行状态")]
        public bool Running { get; set; }

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
                        ContainerID = item["ID"].ToString(),
                        Name = item["Names"].ToString().TrimStart('/'),
                        Image = item["Labels"].ToString()?.Split(',').FirstOrDefault(x => x.Split('=')[0] == "net.unraid.docker.icon")?.Split('=').Last(),
                        Running = item["State"].ToString() == "running",
                        DateTime = DateTime.Now
                    });
                }
            }
            catch (Exception)
            {
                MainSave.CQLog?.Error("获取Docker", $"Json解析失败: {input}");
            }
            return containers.ToArray();
        }
    }
}