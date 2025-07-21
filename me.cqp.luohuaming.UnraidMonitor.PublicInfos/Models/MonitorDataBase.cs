using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models
{
    public class MonitorDataBase
    {
        [Description("最后更新时间")]
        public DateTime DateTime { get; set; } = DateTime.Now;

        public static Dictionary<string, List<(DateTime cacheTime, object data)>> Cache { get; set; } = [];

        public void CheckAlarms()
        {
            AlarmManager.Instance.Check(this);
        }

        public void CacheItem()
        {
            lock (Cache)
            {
                var entityType = GetType();
                string name = entityType.Name;
                DateTime time = DateTime.Now;
                if (Cache.TryGetValue(name, out var cache))
                {
                    for (int i = 0; i < cache.Count; i++)
                    {
                        if ((time - cache[i].cacheTime).TotalSeconds > AppConfig.CacheKeepSeconds)
                        {
                            cache.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            break;
                        }
                    }
                    cache.Add((time, this));
                }
                else
                {
                    Cache.Add(name, [(time, this)]);
                }
            }
        }
    }
}
