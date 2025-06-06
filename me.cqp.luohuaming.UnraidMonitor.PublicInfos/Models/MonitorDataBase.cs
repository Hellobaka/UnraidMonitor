using System;
using System.Collections.Generic;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class MonitorDataBase
    {
        public int Id { get; set; }

        public DateTime DateTime { get; set; } = DateTime.Now;
    }
}
