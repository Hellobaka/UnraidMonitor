using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos
{
    public enum AlarmType
    {
        [Description("区间报警")]
        RangeAlarm,

        [Description("变化率报警")]
        RateOfChangeAlarm,

        [Description("持续时间阈值报警")]
        ThresholdAlarm
    }

    public class AlarmManager
    {
        public static event Action<AlarmRuleBase, string> OnAlarmPost;

        public static event Action<AlarmRuleBase, string> OnAlarmRecover;

        public static AlarmManager Instance { get; set; } = new AlarmManager();

        public List<AlarmRuleBase> Rules { get; set; } = [];

        public static void SaveRules(string path)
        {
            try
            {
                var rules = Instance.Rules.Select(x => x.Clone()).ToList();
                File.WriteAllText(path, JsonConvert.SerializeObject(rules, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented,
                }));
            }
            catch (Exception e)
            {
                MainSave.CQLog?.Error("注册数据监控", $"保存规则列表失败：{e}");
            }
        }

        public static void LoadRules(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return;
                }
                List<AlarmRuleBase> rules = JsonConvert.DeserializeObject<List<AlarmRuleBase>>(File.ReadAllText(path), new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
                foreach (var rule in rules)
                {
                    Instance.RegisterRule(rule);
                }
            }
            catch (Exception e)
            {
                MainSave.CQLog?.Error("注册数据监控", $"获取规则列表失败：{e}");
            }
        }

        public void Check(MonitorDataBase data)
        {
            foreach (var rule in Rules.Where(x => x.Enabled))
            {
                if (rule.TryGetValue(data, out double value)
                    && (rule.IsTimeRangeAlarm ? CheckTimeRange(rule) : true))
                {
                    if (rule.Check(value))
                    {
                        var span = DateTime.Now - rule.LastAlarmTime;
                        if (span > rule.AlarmInterval)
                        {
                            if (!rule.CanDuplicateAlarmPost && rule.Alarmed)
                            {
                                continue; // 如果不允许重复报警且已经处于报警状态，则跳过
                            }
                            rule.LastAlarmTime = DateTime.Now;
                            rule.Alarmed = true;
                            Task.Run(() => OnAlarmPost?.Invoke(rule, rule.GetAlarm(value)));
                        }
                    }
                    else
                    {
                        if (rule.Alarmed)
                        {
                            rule.Alarmed = false;
                            Task.Run(() => OnAlarmRecover?.Invoke(rule, rule.GetRecover(value)));
                        }
                    }
                }
            }
        }

        public void RegisterRule(AlarmRuleBase rule)
        {
            Rules.Add(rule);
        }

        public void UnregisterRule(AlarmRuleBase rule)
        {
            Rules.Remove(rule);
        }

        private static bool CheckTimeRange(AlarmRuleBase rule)
        {
            var now = DateTime.Now.TimeOfDay;
            return now >= rule.StartTime && now <= rule.EndTime;
        }
    }

    [AddINotifyPropertyChangedInterface]
    public abstract class AlarmRuleBase
    {
        public string Name { get; set; } = "Default Alarm";

        public bool Enabled { get; set; }

        /// <summary>
        /// 是否处于报警状态
        /// </summary>
        [JsonIgnore]
        public bool Alarmed { get; set; }

        /// <summary>
        /// 允许报警的最小间隔
        /// </summary>
        public TimeSpan AlarmInterval { get; set; } = TimeSpan.FromHours(1);

        /// <summary>
        /// 报警能否重复抛出
        /// </summary>
        public bool CanDuplicateAlarmPost { get; set; } = false;

        /// <summary>
        /// 报警抛出时文本的格式字符串
        /// </summary>
        /// <remarks>格式类似于 %Value:f2%</remarks>
        public string AlarmNotifyFormat { get; set; } = string.Empty;

        /// <summary>
        /// 启用报警的类名
        /// </summary>
        public string ClassName { get; set; } = string.Empty;

        /// <summary>
        /// 第一个无效点时间
        /// </summary>
        [JsonIgnore]
        public DateTime FirstInvalidTime { get; set; }

        /// <summary>
        /// 异常值数量
        /// </summary>
        [JsonIgnore]
        public int InvalidValueCount { get; set; }

        /// <summary>
        /// 上次报警时间
        /// </summary>
        [JsonIgnore]
        public DateTime LastAlarmTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// 启动报警的属性名称
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// 报警回复时的文本格式字符串
        /// </summary>
        public string RecoverNotifyFormat { get; set; } = string.Empty;

        /// <summary>
        /// 控制报警是否在特定时间段内启用
        /// </summary>
        public bool IsTimeRangeAlarm { get; set; }

        /// <summary>
        /// 报警启用开始时间段
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// 报警启用结束时间段
        /// </summary>
        public TimeSpan EndTime { get; set; }

        public bool EnableFilter { get; set; }

        public string FilterPropertyName { get; set; }

        public string FilterPropertyValue { get; set; }

        private static Regex PlaceholderRegex { get; } = new Regex(@"%([A-Za-z0-9_]+)(?::([a-zA-Z0-9]+))?%", RegexOptions.Compiled);

        /// <summary>
        /// 检查是否需要报警
        /// </summary>
        /// <param name="value">欲处理的值</param>
        /// <returns>True时报警 False时不报警</returns>
        public virtual bool Check(double value)
        {
            return false;
        }

        public string Format(string pattern, Dictionary<(string display, string value), object> variableList)
        {
            return string.IsNullOrEmpty(pattern) || variableList == null
                ? pattern
                : PlaceholderRegex.Replace(pattern, match =>
                {
                    var key = match.Groups[1].Value;
                    var format = match.Groups[2].Success ? match.Groups[2].Value : null;

                    var find = variableList.Any(x => x.Key.value.Equals(key, StringComparison.OrdinalIgnoreCase));
                    var v = variableList.FirstOrDefault(x => x.Key.value.Equals(key, StringComparison.OrdinalIgnoreCase));
                    var value = v.Value;
                    if (!find || value == null)
                    {
                        return match.Value; // 未找到变量，原样返回
                    }

                    if (format != null)
                    {
                        // 尝试格式化
                        if (value is IFormattable formattable)
                        {
                            try
                            {
                                return formattable.ToString(format, CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                return value.ToString(); // 格式化失败则用ToString
                            }
                        }
                        else
                        {
                            return value.ToString();
                        }
                    }
                    return value.ToString();
                });
        }

        public string GetAlarm(double value) => Format(AlarmNotifyFormat, GetVariableList(value));

        public string GetRecover(double value) => Format(RecoverNotifyFormat, GetVariableList(value));

        public virtual Dictionary<(string display, string value), object> GetVariableList(double value)
        {
            return [];
        }

        /// <summary>
        /// 从统计数据中，根据配置的类名与属性名获取目标值；同时启用筛选机制
        /// </summary>
        public bool TryGetValue(MonitorDataBase data, out double value)
        {
            var type = data.GetType();
            value = 0;
            if (type.Name != ClassName)
            {
                return false;
            }

            var prop = type.GetProperty(PropertyName);
            if (prop == null)
            {
                return false;
            }
            var valueObj = prop.GetValue(data);
            if (valueObj == null)
            {
                return false;
            }
            if (EnableFilter)
            {
                var filterProp = type.GetProperty(FilterPropertyName);
                if (filterProp == null)
                {
                    MainSave.CQLog.Warning("AlarmFilter", $"{Name} 的过滤属性 {FilterPropertyName} 未找到对应实例的属性");
                }
                else
                {
                    var filter = filterProp.GetValue(data);
                    if (filter.ToString() != FilterPropertyValue)
                    {
                        return false;
                    }
                }
            }
            try
            {
                value = Convert.ToDouble(valueObj);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public virtual AlarmRuleBase Clone()
        {
            throw new NotImplementedException();
        }

        public void CloneBase(AlarmRuleBase target)
        {
            target.Name = Name;
            target.Enabled = Enabled;
            target.Alarmed = Alarmed;
            target.AlarmInterval = AlarmInterval;
            target.AlarmNotifyFormat = AlarmNotifyFormat;
            target.CanDuplicateAlarmPost = CanDuplicateAlarmPost;
            target.ClassName = ClassName;
            target.FirstInvalidTime = FirstInvalidTime;
            target.InvalidValueCount = InvalidValueCount;
            target.LastAlarmTime = LastAlarmTime;
            target.IsTimeRangeAlarm = IsTimeRangeAlarm;
            target.PropertyName = PropertyName;
            target.RecoverNotifyFormat = RecoverNotifyFormat;
            target.StartTime = StartTime;
            target.EndTime = EndTime;
            target.FilterPropertyValue = FilterPropertyValue;
            target.FilterPropertyName = FilterPropertyName;
            target.EnableFilter = EnableFilter;
        }
    }

    /// <summary>
    /// 区间报警规则
    /// </summary>
    public class RangeAlarmRule : AlarmRuleBase
    {
        /// <summary>
        /// 是否区间内部报警（true：在区间内报警，false：在区间外报警）
        /// </summary>
        public bool AlarmInside { get; set; } = false;

        public double Max { get; set; }

        public double Min { get; set; }

        /// <summary>
        /// 持续超出阈值时长，0秒表示即刻报警
        /// </summary>
        public TimeSpan ThresholdTimeSpan { get; set; } = TimeSpan.FromMinutes(1);

        public override bool Check(double value)
        {
            bool alarm = AlarmInside ? (value >= Min && value <= Max) : (value < Min || value > Max);
            if (alarm)
            {
                InvalidValueCount++;
                if (FirstInvalidTime == DateTime.MinValue)
                {
                    FirstInvalidTime = DateTime.Now;
                }
                var span = DateTime.Now - FirstInvalidTime;
                return span > ThresholdTimeSpan;
            }
            else
            {
                FirstInvalidTime = DateTime.MinValue;
                InvalidValueCount = 0;
                return false;
            }
        }

        public override Dictionary<(string display, string value), object> GetVariableList(double value)
        {
            return new()
            {
                { ("当前值", "Value"), value },
                { ("报警下限", "Min"), Min },
                { ("报警上限", "Max"), Max },
                { ("首个异常值", "FirstInvalidTime"), FirstInvalidTime },
                { ("最后异常时间", "LastAlarmTime"), LastAlarmTime },
                { ("异常值个数", "InvalidValueCount"), InvalidValueCount },
            };
        }

        public override AlarmRuleBase Clone()
        {
            var clone = new RangeAlarmRule
            {
                Max = Max,
                Min = Min,
                AlarmInside = AlarmInside,
                ThresholdTimeSpan = ThresholdTimeSpan
            };
            CloneBase(clone);

            return clone;
        }
    }

    /// <summary>
    /// 变化率报警规则
    /// </summary>
    public class RateOfChangeAlarmRule : AlarmRuleBase
    {
        /// <summary>
        /// 允许的最大变化量
        /// </summary>
        public double MaxDelta { get; set; } = 10;

        private double? LastValue { get; set; } = null;

        public override bool Check(double value)
        {
            if (LastValue.HasValue)
            {
                double delta = Math.Abs(value - LastValue.Value);
                if (delta > MaxDelta)
                {
                    LastValue = value;
                    return true;
                }
            }
            LastValue = value;
            return false;
        }

        public override Dictionary<(string display, string value), object> GetVariableList(double value)
        {
            return new()
            {
                { ("当前值", "Value"), value },
                { ("最大允许变化值", "MaxDelta"), MaxDelta },
                { ("首个异常值", "FirstInvalidTime"), FirstInvalidTime },
                { ("最后异常时间", "LastAlarmTime"), LastAlarmTime },
                { ("异常值个数", "InvalidValueCount"), InvalidValueCount },
            };
        }

        public override AlarmRuleBase Clone()
        {
            var clone = new RateOfChangeAlarmRule
            {
                MaxDelta = MaxDelta
            };
            CloneBase(clone);
            return clone;
        }
    }

    /// <summary>
    /// 持续时间阈值规则
    /// </summary>
    public class ThresholdAlarmRule : AlarmRuleBase
    {
        public double Threshold { get; set; }

        /// <summary>
        /// 当前值如何与阈值进行比较
        /// </summary>
        public BoolCondition ThresholdBoolCondition { get; set; } = BoolCondition.GTE;

        /// <summary>
        /// 持续超出阈值时长，0秒表示即刻报警
        /// </summary>
        public TimeSpan ThresholdTimeSpan { get; set; } = TimeSpan.FromMinutes(1);

        public override bool Check(double value)
        {
            bool alarm = ThresholdBoolCondition switch
            {
                BoolCondition.GTE => value >= Threshold,
                BoolCondition.LTE => value <= Threshold,
                BoolCondition.GT => value > Threshold,
                BoolCondition.LT => value < Threshold,
                _ => Math.Round(value, 3) == Math.Round(Threshold, 3),
            };
            if (alarm)
            {
                InvalidValueCount++;
                if (FirstInvalidTime == DateTime.MinValue)
                {
                    FirstInvalidTime = DateTime.Now;
                }
                var span = DateTime.Now - FirstInvalidTime;
                if (span > ThresholdTimeSpan)
                {
                    return true;
                }
                return false;
            }
            else
            {
                FirstInvalidTime = DateTime.MinValue;
                InvalidValueCount = 0;
                return false;
            }
        }

        public override Dictionary<(string display, string value), object> GetVariableList(double value)
        {
            return new()
            {
                { ("当前值", "Value"), value },
                { ("阈值", "Threshold"), Threshold },
                { ("首个异常值", "FirstInvalidTime"), FirstInvalidTime },
                { ("最后异常时间", "LastAlarmTime"), LastAlarmTime },
                { ("异常值个数", "InvalidValueCount"), InvalidValueCount },
            };
        }

        public override AlarmRuleBase Clone()
        {
            var clone = new ThresholdAlarmRule
            {
                Threshold = Threshold,
                ThresholdBoolCondition = ThresholdBoolCondition,
                ThresholdTimeSpan = ThresholdTimeSpan
            };
            CloneBase(clone);
            return clone;
        }
    }
}