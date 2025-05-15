using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class UPSStatus
    {
        public string Model { get; set; }

        public double MaxPower { get; set; }

        public double CurrentLoad { get; set; }

        public double TimeLeft { get; set; }

        public double CurrentVoltage { get; set; }

        public double BatteryLevel { get; set; }

        /// <summary>
        /// ONLINE/ONBATT
        /// </summary>
        public string Status { get; set; }

        public double CurrentPower => MaxPower * (CurrentLoad / 100);

        public DateTime DateTime { get; set; }

        public static UPSStatus ParseFromApcAccess(string input)
        {
            var lines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            UPSStatus status = new();
            foreach(var line in lines)
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
