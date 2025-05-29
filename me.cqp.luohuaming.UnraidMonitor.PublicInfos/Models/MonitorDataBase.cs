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

        public virtual void Insert()
        {
            var db = DBHelper.Instance;
            dynamic dynObj = this;
            var sql = db.Insertable(dynObj).ToSqlString();
            db.Ado.ExecuteCommand(sql);
        }

        public static List<T> GetDataRange<T>(DateTime start, DateTime end) where T : MonitorDataBase, new()
        {
            var db = DBHelper.Instance;
            return db.Queryable<T>().Where(x => x.DateTime >= start && x.DateTime <= end).ToList();
        }
    }
}
