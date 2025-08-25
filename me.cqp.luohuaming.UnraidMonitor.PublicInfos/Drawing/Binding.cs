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
                // ToList防止源数组更改发生异常
                foreach (var item in cache.ToList().Where(x => x.cacheTime > From).OrderBy(x => x.cacheTime))
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
            var modelType = GetModelType();
            if (modelType == null)
            {
                Debugger.Break();
                return [];
            }

            var multiBindings = GetAllMultiBindings();
            var propertyMap = GetPropertyMap(modelType, multiBindings);
            var conditionMap = GetConditionMap(modelType);

            if (propertyMap.Count == 0 && !BindingPath.ContainsKey("$"))
            {
                Debugger.Break();
                return [];
            }

            var metrics = GetMetricsForModelType(modelType);
            if (metrics is not IEnumerable metricList)
            {
                Debugger.Break();
                return [];
            }

            // 清空上一次绑定结果
            ClearLastBindResult();

            // 收集满足条件的数据
            foreach (var item in metricList)
            {
                if (!IsMatchAllConditions(item, conditionMap))
                {
                    continue;
                }

                BindItemValues(item, propertyMap);
            }

            // 解析最终结果
            return ParseBindingResults();
        }

        private Type GetModelType()
        {
            return GetType().Assembly.GetTypes()
                .FirstOrDefault(x => x.FullName == $"me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.{ItemType}");
        }

        private MultipleBinding[] GetAllMultiBindings()
        {
            return BindingPath.SelectMany(x => x.Value).Distinct().ToArray();
        }

        private Dictionary<MultipleBinding, PropertyInfo> GetPropertyMap(Type modelType, MultipleBinding[] multiBindings)
        {
            var map = new Dictionary<MultipleBinding, PropertyInfo>();
            foreach (var prop in modelType.GetProperties())
            {
                foreach (var bind in multiBindings.Where(x => x.Path == prop.Name))
                {
                    bind.IsNumber = IsNumericType(prop.PropertyType);
                    map[bind] = prop;
                }
            }
            return map;
        }

        private Dictionary<PropertyInfo, string> GetConditionMap(Type modelType)
        {
            var map = new Dictionary<PropertyInfo, string>();
            foreach (var prop in modelType.GetProperties())
            {
                if (Conditions.TryGetValue(prop.Name, out string value))
                {
                    map[prop] = value;
                }
            }
            return map;
        }

        private object GetMetricsForModelType(Type modelType)
        {
            var method = GetType().GetMethod("GetMetrics", BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
            {
                return null;
            }

            var genericMethod = method.MakeGenericMethod(modelType);
            return genericMethod.Invoke(this, []);
        }

        private bool IsMatchAllConditions(object item, Dictionary<PropertyInfo, string> conditionMap)
        {
            return conditionMap.All(x => x.Key.GetValue(item)?.ToString() == x.Value);
        }

        private void BindItemValues(object item, Dictionary<MultipleBinding, PropertyInfo> propertyMap)
        {
            foreach (var kvp in BindingPath)
            {
                foreach (var binding in kvp.Value)
                {
                    if (propertyMap.TryGetValue(binding, out var prop))
                    {
                        var value = prop.GetValue(item);
                        binding.RawValues = [.. binding.RawValues, value];
                    }
                    else
                    {
                        Debugger.Break();
                    }
                }
            }
        }

        /// <summary>
        /// 解析所有绑定项的最终结果，生成绑定结果字典。
        /// </summary>
        /// <returns>key为绑定项名，value为绑定结果（包含原始值、数值、格式化字符串）</returns>
        private Dictionary<string, BindingResult> ParseBindingResults()
        {
            var result = new Dictionary<string, BindingResult>();

            // 遍历所有绑定路径（即所有需要输出的字段）
            foreach (var kvp in BindingPath)
            {
                var key = kvp.Key;         // 绑定项名
                var bindings = kvp.Value;  // 该项下的所有多重绑定

                // 初始化绑定结果（若已存在则复用）
                if (!result.TryGetValue(key, out var resultBinding))
                {
                    resultBinding = new BindingResult();
                }

                object[] bindingResult = [];    // 存储本次绑定项的处理后结果（可为数值或字符串）
                bool hasString = bindings.Any(x => !x.IsNumber); // 是否包含字符串类型

                // 处理每个绑定项
                foreach (var binding in bindings)
                {
                    // 只处理有采集到值的绑定
                    if (binding.RawValues.Length > 0)
                    {
                        if (binding.IsNumber)
                        {
                            // 1. 数值类型先应用单位转换器
                            binding.RawValues = ApplyConverter(binding.RawValues, binding.NumberConverter);

                            // 2. 若为差值类型，计算差值
                            if (binding.ValueType is ValueType.Diff
                                or ValueType.DiffMin
                                or ValueType.DiffMax)
                            {
                                binding.RawValues = CalcDiff(binding.RawValues, binding.DiffUnit);
                            }

                            // 3. 根据配置的统计类型（如平均、最大等）计算最终数值
                            double number = GetNumber(binding.RawValues, binding.ValueType);
                            bindingResult = [.. bindingResult, number];
                        }
                        else
                        {
                            // 字符串类型直接取最后一个值，如果这个值还是数组，则再取数组的最后一个元素
                            object lastValue = binding.RawValues.Last();
                            if (lastValue is Array arr && arr.Length > 0)
                            {
                                // 如果是数组且不为空，取数组的最后一个元素
                                bindingResult = [.. bindingResult, arr.GetValue(arr.Length - 1)];
                            }
                            else if (lastValue is IList list && list.Count > 0)
                            {
                                bindingResult = [.. bindingResult, list[list.Count - 1]];
                            }
                            else
                            {
                                // 否则直接取值
                                bindingResult = [.. bindingResult, lastValue];
                            }
                        }
                    }

                    // 累加原始值到结果，用于后续可能的调试或展示
                    resultBinding.RawValues = [.. binding.RawValues, .. resultBinding.RawValues];
                }

                try
                {
                    if (bindingResult.Length > 0)
                    {
                        // 若有字符串参与，则数值为0，否则取第一个数值
                        resultBinding.ParsedNumber = hasString ? 0 : (double)bindingResult.FirstOrDefault();

                        // 格式化输出字符串，支持多值格式化
                        resultBinding.FormattedString = string.Format(StringFormat, bindingResult);
                    }
                    else
                    {
                        // 没有任何结果时，给出默认值
                        resultBinding.ParsedNumber = 0;
                        int formatCount = StringFormat.Count(x => x is '{' and not '}');
                        resultBinding.FormattedString = string.Format(StringFormat, new string[formatCount]);
                    }
                }
                catch
                {
                }

                // 保存本次绑定项的结果
                result[key] = resultBinding;
            }
            return result;
        }

        private bool IsNumericType(Type type)
        {
            return type == typeof(int) || type == typeof(long) ||
                   type == typeof(double) || type == typeof(float);
        }

        private void ClearLastBindResult()
        {
            foreach (var item in BindingPath)
            {
                foreach (var binding in item.Value)
                {
                    binding.RawValues = [];
                }
            }
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

        public double GetNumber(object[] data, ValueType valueType)
        {
            return data.Length == 0 ? 0 : valueType switch
            {
                ValueType.DiffMax or ValueType.Max => data.Max(x => Convert.ToDouble(x)),
                ValueType.DiffMin or ValueType.Min => data.Min(x => Convert.ToDouble(x)),
                ValueType.Diff or ValueType.Avg => data.Average(x => Convert.ToDouble(x)),
                ValueType.Sum => data.Sum(x => Convert.ToDouble(x)),
                ValueType.Count => data.Count(),
                _ => Convert.ToDouble(data.Last()),
            };
        }

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
            return obj is MultipleBinding other && other.Path != null && other.Path == Path;
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