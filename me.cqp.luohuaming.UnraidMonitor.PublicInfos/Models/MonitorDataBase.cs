using SqlSugar;
using System;
using System.Collections.Generic;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class MonitorDataBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public DateTime DateTime { get; set; } = DateTime.Now;
    }
}
