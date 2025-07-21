using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class UPSStatus : MonitorDataBase
    {
        [Description("UPS 型号")]
        public string Model { get; set; } = "";

        [Description("最大允许功率 (W)")]
        public double MaxPower { get; set; }

        [Description("当前负载功率 (W)")]
        public double CurrentLoad { get; set; }

        [Description("剩余可用时长 (分钟)")]
        public double TimeLeft { get; set; }

        [Description("当前电压 (V)")]
        public double CurrentVoltage { get; set; }

        [Description("电量百分比 (%)")]
        public double BatteryLevel { get; set; }

        /// <summary>
        /// ONLINE/ONBATT
        /// </summary>
        [Description("仅电池状态")]
        public string Status { get; set; } = "";

        [Description("输出功率 (W)")]
        public double CurrentPower => MaxPower * (CurrentLoad / 100);

        public static UPSStatus ParseFromApcAccess(string input)
        {
            var lines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            UPSStatus status = new();
            status.DateTime = DateTime.Now;
            foreach (var line in lines)
            {
                var parts = line.Split([':'], 2);

                var key = parts[0].Trim();
                var value = parts[1].Trim();
                switch (key)
                {
                    case "MODEL":
                        status.Model = value;
                        break;

                    case "STATUS":
                        status.Status = value;
                        break;

                    case "LOADPCT":
                        status.CurrentLoad = double.TryParse(value.Split(' ').First(), out double d) ? d : 0;
                        break;

                    case "BCHARGE":
                        status.BatteryLevel = double.TryParse(value.Split(' ').First(), out d) ? d : 0;
                        break;

                    case "TIMELEFT":
                        status.TimeLeft = double.TryParse(value.Split(' ').First(), out d) ? d : 0;
                        break;

                    case "LINEV":
                        status.CurrentVoltage = double.TryParse(value.Split(' ').First(), out d) ? d : 0;
                        break;

                    case "NOMPOWER":
                        status.MaxPower = double.TryParse(value.Split(' ').First(), out d) ? d : 0;
                        break;

                    default:
                        break;
                }
            }
            return status;
        }
    }
}
