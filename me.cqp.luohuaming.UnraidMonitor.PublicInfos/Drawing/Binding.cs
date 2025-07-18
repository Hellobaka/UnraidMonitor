using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Handler;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing
{
    public enum NumberConverter
    {
        [Description("无")]
        None,

        [Description("B 至 TB")]
        BytesToTB,

        [Description("B 至 GB")]
        BytesToGB,

        [Description("B 至 MB")]
        BytesToMB,

        [Description("B 至 KB")]
        BytesToKB,
    }

    public enum ValueType
    {
        [Description("即时")]
        Instant,

        [Description("最大值")]
        Max,

        [Description("最小值")]
        Min,

        [Description("平均值")]
        Avg,

        [Description("加合")]
        Sum,

        [Description("计数")]
        Count,

        [Description("差异_平均值")]
        Diff,

        [Description("差异_最大值")]
        DiffMax,

        [Description("差异_最小值")]
        DiffMin,
    }

    public enum ItemType
    {
        [Description("未设定")]
        Unknown,

        [Description("CPU 信息")]
        CpuInfo,

        [Description("CPU 占用率")]
        CpuUsage,

        [Description("磁盘占用率")]
        DiskInfo,

        [Description("分区")]
        DiskMountInfo,

        [Description("SMART 信息")]
        DiskSmartInfo,

        [Description("Docker")]
        Dockers,

        [Description("风扇")]
        FanInfo,

        [Description("内存占用率")]
        MemoryInfo,

        [Description("主板信息")]
        MotherboardInfo,

        [Description("网络信息")]
        NetworkInterfaceInfo,

        [Description("网络流量")]
        NetworkTrafficInfo,

        [Description("系统提醒")]
        Notification,

        [Description("系统信息")]
        SystemInfo,

        [Description("系统启动时长")]
        SystemUptime,

        [Description("温度信息")]
        TemperatureInfo,

        [Description("UPS 状态")]
        UPSStatus,

        [Description("虚拟机信息")]
        VirtualMachine,

        [Description("无定义")]
        Custom,
    }

    public enum BoolCondition
    {
        [Description("大于等于")]
        GTE,

        [Description("小于等于")]
        LTE,

        [Description("大于")]
        GT,

        [Description("小于")]
        LT,

        [Description("等于")]
        EQ
    }

    public enum TimeRange
    {
        [Description("秒")]
        Second,

        [Description("分钟")]
        Minute,

        [Description("小时")]
        Hour,

        [Description("天")]
        Day,

        [Description("月")]
        Month,

        [Description("年")]
        Year
    }

    public class Binding
    {
        public ItemType ItemType { get; set; } = ItemType.CpuInfo;

        /// <summary>
        /// 绑定的路径，格式为：{"Item属性": "Model属性"}
        /// </summary>
        public Dictionary<string, List<MultipleBinding>> BindingPath { get; set; } = [];

        public Dictionary<string, string> Conditions { get; set; } = [];

        public Dictionary<string, BindingResult> Value { get; set; } = [];

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
            if (MonitorDataBase.Cache.TryGetValue(name, out var cache))
            {
                foreach (var item in cache.Where(x => x.cacheTime > From).OrderBy(x => x.cacheTime))
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
                    // 解析绑定结果并填充 result 字典
                    foreach (var bind in BindingPath)
                    {
                        if (!result.TryGetValue(bind.Key, out var resultBinding))
                        {
                            resultBinding = new();
                            // 用于存储当前绑定项处理后的结果（数值或字符串），用于后续格式化输出和数值解析
                            object[] bindingResult = [];
                            bool hasString = bind.Value.Any(x => !x.IsNumber);
                            // 遍历每个绑定项，处理数值和字符串类型
                            foreach (var binding in bind.Value)
                            {
                                if (binding.RawValues.Length > 0)
                                {
                                    if (binding.IsNumber)
                                    {
                                        double number = 0;
                                        // 应用数值转换器
                                        binding.RawValues = ApplyConverter(binding.RawValues, binding.NumberConverter);
                                        // 差值计算
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
                                        // 字符串类型取最后一个值
                                        bindingResult = [.. bindingResult, binding.RawValues.Last()];
                                    }
                                }
                                // 累加原始值
                                resultBinding.RawValues = [.. binding.RawValues, .. resultBinding.RawValues];
                            }
                            try
                            {
                                if (bindingResult.Length > 0)
                                {
                                    // 若有字符串则数值为0，否则取第一个数值
                                    resultBinding.ParsedNumber = hasString ? 0 : (double)bindingResult.FirstOrDefault();
                                    // 格式化字符串输出
                                    resultBinding.FormattedString = string.Format(StringFormat, bindingResult);
                                }
                                else
                                {
                                    resultBinding.ParsedNumber = 0;
                                    int formatCount = StringFormat.Count(x => x == '{' && x != '}');
                                    resultBinding.FormattedString = string.Format(StringFormat, new string[formatCount]);
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

        public Binding Clone()
        {
            string item = JsonConvert.SerializeObject(this, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            });
            return JsonConvert.DeserializeObject<Binding>(item);
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