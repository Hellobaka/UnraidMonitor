using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Handler;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models;
using Newtonsoft.Json;
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

    public class Binding
    {
        public ItemType ItemType { get; set; } = ItemType.CPUInfo;

        public ValueType ValueType { get; set; } = ValueType.Instant;

        /// <summary>
        /// 绑定的路径，格式为：{"Item属性": "Model属性"}
        /// </summary>
        public Dictionary<string, MultipleBinding[]> BindingPath { get; set; } = [];

        public Dictionary<string, string> Conditions { get; set; }

        public Dictionary<string, BindingResult> Value { get; set; }

        public string StringFormat { get; set; } = "";

        public TimeRange FromTimeRange { get; set; }

        public int FromTimeValue { get; set; }

        public TimeRange ToTimeRange { get; set; }

        public int ToTimeValue { get; set; }

        private DateTime From { get; set; }

        private DateTime To { get; set; }

        public List<T> GetMetrics<T>() where T : MonitorDataBase, new()
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

        private List<T> GetDataFromDB<T>() where T : MonitorDataBase, new()
        {
            return MonitorDataBase.GetDataRange<T>(From, To);
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
        public Dictionary<string, BindingResult> CallGetMetricsAndParseResult()
        {
            var itemType = Type.GetType($"me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.{ItemType}");
            MultipleBinding[] multipleBindings = BindingPath.SelectMany(x => x.Value).Distinct().ToArray();
            Dictionary<MultipleBinding, PropertyInfo> pathProperties = [];// 模型属性，反射属性
            Dictionary<PropertyInfo, string> tags = [];
            foreach (var item in itemType.GetProperties())
            {
                // 反射Model所有属性，获取绑定路径和条件
                // 整理MultiBinding，获取所有绑定需求的路径，若当前模型属性在绑定需求中，则缓存反射信息
                var bind = multipleBindings.FirstOrDefault(x => x.Path == item.Name);
                if (bind != null)
                {
                    bind.IsNumber = item.Name == "Int32" || item.Name == "Double" || item.Name == "Single";
                    pathProperties[bind] = item;
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
                // Item属性键，此次绑定结果
                Dictionary<string, BindingResult> result = [];
                // 从缓存以及数据库获取数据后，解析结果
                if (data is IEnumerable list)
                {
                    foreach (var item in list)
                    {
                        if (tags.All(x => x.Key.GetValue(item).ToString() == x.Value))
                        {
                            // 要求满足所有条件，说明此Item可以进行绑定
                            // 按照BindingPath进行绑定
                            foreach (var bind in BindingPath)
                            {
                                foreach (var binding in bind.Value)
                                {
                                    if (pathProperties.TryGetValue(binding, out var propertyInfo))
                                    {
                                        var value = propertyInfo.GetValue(item);
                                        binding.RawValues = [.. binding.RawValues, value];
                                    }
                                    else
                                    {
                                        Debugger.Break();
                                    }
                                }
                            }
                        }
                    }
                    // 解析绑定结果
                    foreach(var bind in BindingPath)
                    {
                        if (!result.TryGetValue(bind.Key, out var resultBinding))
                        {
                            resultBinding = new();
                            object[] bindingResult = [];
                            bool hasString = bind.Value.Any(x => !x.IsNumber);
                            foreach(var binding in bind.Value)
                            {
                                resultBinding.RawValues = [..binding.RawValues, ..resultBinding.RawValues];
                                if (binding.IsNumber)
                                {
                                    bindingResult = [.. bindingResult, GetNumber(binding.RawValues, binding.ValueType)];
                                }
                            }
                            resultBinding.ParsedNumber = hasString ? 0 : (double)bindingResult.FirstOrDefault();
                            resultBinding.FormattedString = string.Format(StringFormat, bindingResult);
                            result[bind.Key] = resultBinding;
                        }
                    }
                    return result;
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
            Value = CallGetMetricsAndParseResult();
        }

        public double GetNumber(object[] data, ValueType valueType) => data.Length == 0 ? 0 : valueType switch
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

    /// <summary>
    /// 多值绑定，仅可用于文本多绑，数字
    /// </summary>
    public class MultipleBinding
    {
        public string Path { get; set; }

        public ValueType ValueType { get; set; } = ValueType.Instant;

        public bool IsNumber { get; set; }

        public object[] RawValues { get; set; } = [];

        public override bool Equals(object obj)
        {
            MultipleBinding other = obj as MultipleBinding;
            return other.Path == Path;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class BindingResult
    {
        public object[] RawValues { get; set; }

        public double ParsedNumber { get; set; }

        public string FormattedString { get; set; }
    }
}