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
    public enum NumberConverter
    {
        None,
        BytesToTB,
        BytesToGB,
        BytesToMB,
        BytesToKB,
    }

    public enum ValueType
    {
        Instant,

        Max,

        Min,

        Avg,

        Sum,

        Count,

        Diff,

        DiffMax,

        DiffMin,
    }

    public enum ItemType
    {
        CpuInfo,

        CpuUsage,

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
        public ItemType ItemType { get; set; } = ItemType.CpuInfo;

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

            // 从缓存获取统计数据
            return GetDataFromCache<T>().OrderBy(x => x.DateTime).ToList();
        }

        private List<T> GetDataFromCache<T>()
        {
            var name = typeof(T).Name;
            List<T> data = [];
            if( MonitorDataBase.Cache.TryGetValue(name, out var cache))
            {
                foreach(var item in cache.Where(x => x.cacheTime > From).OrderBy(x => x.cacheTime))
                {
                    if (item.data is Array arr)
                    {
                        foreach (var i in arr)
                        {
                            data.Add((T)i);
                        }
                    }
                    else
                    {
                        data.Add((T)item.data);
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// 调用 GetMetrics 方法并解析结果为字典列表
        /// </summary>
        /// <returns>列表内容为：属性值 - 值</returns>
        public Dictionary<string, BindingResult> CallGetMetricsAndParseResult()
        {
            var itemType = GetType().Assembly.GetTypes().FirstOrDefault(x => x.FullName == $"me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.{ItemType}");
            MultipleBinding[] multipleBindings = BindingPath.SelectMany(x => x.Value).Distinct().ToArray();
            Dictionary<MultipleBinding, PropertyInfo> pathProperties = [];// 模型属性，反射属性
            Dictionary<PropertyInfo, string> tags = [];
            foreach (var item in itemType.GetProperties())
            {
                // 反射Model所有属性，获取绑定路径和条件
                // 整理MultiBinding，获取所有绑定需求的路径，若当前模型属性在绑定需求中，则缓存反射信息
                foreach (var bind in multipleBindings.Where(x => x.Path == item.Name))
                {
                    string valueType = item.PropertyType.Name;
                    bind.IsNumber = valueType == "Int32" || valueType == "Int64" || valueType == "Double" || valueType == "Single";
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
                    foreach (var bind in BindingPath)
                    {
                        if (!result.TryGetValue(bind.Key, out var resultBinding))
                        {
                            resultBinding = new();
                            object[] bindingResult = [];
                            bool hasString = bind.Value.Any(x => !x.IsNumber);
                            foreach (var binding in bind.Value)
                            {
                                if (binding.RawValues.Length > 0)
                                {
                                    if (binding.IsNumber)
                                    {
                                        double number = 0;
                                        binding.RawValues = ApplyConverter(binding.RawValues, binding.NumberConverter);
                                        if (binding.ValueType == ValueType.Diff
                                            || binding.ValueType == ValueType.DiffMin
                                            || binding.ValueType == ValueType.DiffMax)
                                        {
                                            binding.RawValues = CalcDiff(binding.RawValues, binding.DiffUnit);
                                        }

                                        number = GetNumber(binding.RawValues, binding.ValueType);
                                        bindingResult = [.. bindingResult, number];
                                    }
                                    else
                                    {
                                        bindingResult = [.. bindingResult, binding.RawValues.Last()];
                                    }
                                }
                                resultBinding.RawValues = [.. binding.RawValues, .. resultBinding.RawValues];
                            }
                            try
                            {
                                if (bindingResult.Length > 0)
                                {
                                    resultBinding.ParsedNumber = hasString ? 0 : (double)bindingResult.FirstOrDefault();
                                    resultBinding.FormattedString = string.Format(StringFormat, bindingResult);
                                }
                                else
                                {
                                    resultBinding.ParsedNumber = 0;
                                    int formatCount = StringFormat.Count(x => x == '{' && x != '}');
                                    resultBinding.FormattedString = string.Format(StringFormat, new string[formatCount]); ;
                                }
                            }
                            catch { }
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

        private object[] ApplyConverter(object[] number, NumberConverter converter)
        {
            List<object> list = [];
            foreach (object value in number)
            {
                double n = Convert.ToDouble(value);
                object o = converter switch
                {
                    NumberConverter.BytesToTB => n / 1024 / 1024 / 1024 / 1024,
                    NumberConverter.BytesToGB => n / 1024 / 1024 / 1024,
                    NumberConverter.BytesToMB => n / 1024 / 1024,
                    NumberConverter.BytesToKB => n / 1024,
                    _ => n,
                };
                list.Add(o);
            }
            return list.ToArray();
        }

        public void Get()
        {
            Value = CallGetMetricsAndParseResult();
        }

        public double GetNumber(object[] data, ValueType valueType) => data.Length == 0 ? 0 : valueType switch
        {
            ValueType.DiffMax or ValueType.Max => data.Max(x => Convert.ToDouble(x)),
            ValueType.DiffMin or ValueType.Min => data.Min(x => Convert.ToDouble(x)),
            ValueType.Diff or ValueType.Avg => data.Average(x => Convert.ToDouble(x)),
            ValueType.Sum => data.Sum(x => Convert.ToDouble(x)),
            ValueType.Count => data.Count(),
            _ => Convert.ToDouble(data.Last()),
        };

        public object[] CalcDiff(object[] data, double diffUnit)
        {
            List<object> diff = [];
            for (int i = 0; i < data.Length; i++)
            {
                if (i % 2 == 1)
                {
                    double num1 = Convert.ToDouble(data[i - 1]);
                    double num2 = Convert.ToDouble(data[i]);
                    diff.Add((num2 - num1) / diffUnit);
                }
            }

            return diff.ToArray();
        }

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

        public NumberConverter NumberConverter { get; set; } = NumberConverter.None;

        public double DiffUnit { get; set; } = 1;

        [JsonIgnore]
        public bool IsNumber { get; set; }

        [JsonIgnore]
        public object[] RawValues { get; set; } = [];

        public override bool Equals(object obj)
        {
            if (obj is not MultipleBinding other || other.Path == null)
            {
                return false;
            }
            return other.Path == Path;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class BindingResult
    {
        [JsonIgnore]
        public object[] RawValues { get; set; } = [];

        [JsonIgnore]
        public double ParsedNumber { get; set; }

        [JsonIgnore]
        public string FormattedString { get; set; } = "";
    }
}