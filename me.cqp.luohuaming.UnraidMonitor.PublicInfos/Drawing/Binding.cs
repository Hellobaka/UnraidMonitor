using LiteDB;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Handler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing
{
    public enum ValueType
    {
        Instant,

        Max,

        Min,

        Avg,

        Sum,

        Count,
    }

    public enum ItemType
    {
        CPUInfo,

        CPUUsage,

        DiskInfo,

        DiskMountInfo,

        DiskSmartInfo,

        Dockers,

        FanInfo,

        MemoryInfo,

        MotherboardInfo,

        NetworkInterfaceInfo,

        NetworkTrafficInfo,

        Notification,

        SystemInfo,

        SystemUptime,

        TemperatureInfo,

        UPSStatus,

        VirtualMachine,

        Custom,
    }

    public enum BoolCondition
    {
        GTE,

        LTE,

        GT,

        LT,

        EQ
    }

    public enum TimeRange
    {
        Second,

        Minute,

        Hour,

        Day,

        Month,

        Year
    }

    public class BindingBase
    {
        public ItemType ItemType { get; set; } = ItemType.CPUInfo;

        public ValueType ValueType { get; set; } = ValueType.Instant;

        /// <summary>
        /// 绑定的路径，格式为：{"Item属性": "Model属性"}
        /// </summary>
        public Dictionary<string, string> BindingPath { get; set; } = [];

        public Dictionary<string, string> Conditions { get; set; }

        public Dictionary<string, List<object>> Value { get; set; }

        public string StringFormat { get; set; } = "";

        public TimeRange FromTimeRange { get; set; }

        public int FromTimeValue { get; set; }

        public TimeRange ToTimeRange { get; set; }

        public int ToTimeValue { get; set; }

        private DateTime From { get; set; }

        private DateTime To { get; set; }

        public List<T> GetMetrics<T>()
        {
            From = GetDateTime(FromTimeRange, FromTimeValue);
            To = GetDateTime(ToTimeRange, ToTimeValue);

            if (From > To)
            {
                throw new ArgumentException("开始时间不得大于结束时间");
            }

            // 从缓存或DB获取统计数据
            var name = typeof(T).Name;
            DateTime cacheEndTime = DateTime.Now;
            if (HandlerBase.Instance.Cache.TryGetValue(name, out var cache))
            {
                cacheEndTime = cache.FirstOrDefault().cacheTime;
            }
            var cacheData = GetDataFromCache<T>();
            if (From > cacheEndTime)
            {
                // 目标时间晚于缓存时间，可都从缓存获取
                return cacheData;
            }
            else if (To < cacheEndTime)
            {
                // 目标时间早于缓存时间，直接从DB获取
                return GetDataFromDB<T>();
            }
            else
            {
                // 目标时间在缓存时间范围内，合并缓存和DB数据
                var dbData = GetDataFromDB<T>();
                var timeProperty = typeof(T).GetProperty("DateTime", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                return cacheData.Concat(dbData).OrderBy(x => timeProperty.GetValue(x)).ToList();
            }
        }

        private List<T> GetDataFromDB<T>()
        {
            var db = DBHelper.Instance;
            return db.GetCollection<T>().Find(Query.And(Query.GTE("DateTime", From), Query.LTE("DateTime", To))).ToList();
        }

        private List<T> GetDataFromCache<T>()
        {
            var name = typeof(T).Name;
            return HandlerBase.Instance.Cache.TryGetValue(name, out var cache)
                ? cache.Where(x => x.cacheTime > From).OrderBy(x => x.cacheTime).Select(x => (T)x.data).ToList()
                : ([]);
        }

        /// <summary>
        /// 调用 GetMetrics 方法并解析结果为字典列表
        /// </summary>
        /// <returns>列表内容为：属性值 - 值</returns>
        public List<Dictionary<string, object>> CallGetMetricsAndParseResult()
        {
            var itemType = Type.GetType($"me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.{ItemType}");
            Dictionary<PropertyInfo, string> pathProperties = [];
            Dictionary<PropertyInfo, string> tags = [];
            foreach (var item in itemType.GetProperties())
            {
                if (BindingPath.TryGetValue(item.Name, out string p))
                {
                    pathProperties[item] = p;
                }
                if (Conditions.TryGetValue(item.Name, out string value))
                {
                    tags[item] = value;
                }
            }
            if (itemType == null || (pathProperties.Count == 0 && BindingPath.FirstOrDefault().Key != "$"))
            {
                Debugger.Break();
                return [];
            }
            var method = GetType().GetMethod("GetMetrics", BindingFlags.Public | BindingFlags.Instance);
            if (method != null)
            {
                var genericMethod = method.MakeGenericMethod(itemType);
                var data = genericMethod.Invoke(this, []);
                if (data is IEnumerable list)
                {
                    List<Dictionary<string, object>> r = [];
                    foreach (var item in list)
                    {
                        if (tags.All(x => x.Key.GetValue(item).ToString() == x.Value))
                        {
                            Dictionary<string, object> dict = [];
                            if (pathProperties.Count == 0 && BindingPath.FirstOrDefault().Key == "$")
                            {
                                dict.Add("$", item);
                            }
                            else
                            {
                                foreach (var property in pathProperties)
                                {
                                    dict.Add(property.Value, property.Key.GetValue(item));
                                }
                            }
                            r.Add(dict);
                        }
                    }
                    return r;
                }
                else
                {
                    Debugger.Break();
                }
            }
            else
            {
                Debugger.Break();
            }
            return [];
        }

        public void Get()
        {
            var data = CallGetMetricsAndParseResult();
            if (data.Count == 0)
            {
                Value = [];
                return;
            }
            foreach (var item in data)
            {
                foreach (var property in item)
                {
                    if (Value.TryGetValue(property.Key, out var v))
                    {
                        Value[property.Key].Add(property.Value);

                    }
                    else
                    {
                        Value.Add(property.Key, [property.Value]);
                    }
                }
            }
        }

        public double GetNumber(List<object> data) => ValueType switch
        {
            ValueType.Max => data.Max(x => (double)x),
            ValueType.Min => data.Min(x => (double)x),
            ValueType.Avg => data.Average(x => (double)x),
            ValueType.Sum => data.Sum(x => (double)x),
            ValueType.Count => data.Count(),
            _ => (double)data.Last(),
        };

        private static DateTime GetDateTime(TimeRange range, int value)
        {
            return range switch
            {
                TimeRange.Second => DateTime.Now.AddSeconds(-value),
                TimeRange.Minute => DateTime.Now.AddMinutes(-value),
                TimeRange.Hour => DateTime.Now.AddHours(-value),
                TimeRange.Day => DateTime.Now.AddDays(-value),
                TimeRange.Month => DateTime.Now.AddMonths(-value),
                TimeRange.Year => DateTime.Now.AddYears(-value),
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }
    }
}