using System;
using System.Collections.Generic;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class MonitorDataBase
    {
        public DateTime DateTime { get; set; } = DateTime.Now;

        public void CheckAlarms()
        {
            AlarmManager.Instance.Check(this);
        }
    }
}
