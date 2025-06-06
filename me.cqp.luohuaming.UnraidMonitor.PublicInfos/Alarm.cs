using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos
{
    public class AlarmManager
    {
        public event Action<string> OnAlarmPost;

        public event Action<string> OnAlarmRecover;

        public static AlarmManager Instance { get; set; } = new AlarmManager();

        private List<AlarmRuleBase> Rules { get; set; } = [];

        public static void LoadRules(string path)
        {
            try
            {
                List<AlarmRuleBase> rules = JsonConvert.DeserializeObject<List<AlarmRuleBase>>(File.ReadAllText(path));
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
                            rule.LastAlarmTime = DateTime.Now;
                            rule.Alarmed = true;
                            OnAlarmPost?.Invoke(rule.GetAlarm(value));
                        }
                    }
                    else
                    {
                        rule.Alarmed = false;
                        if (rule.CanAlarmRecover)
                        {
                            OnAlarmRecover?.Invoke(rule.GetRecover(value));
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

    public abstract class AlarmRuleBase
    {
        public bool Enabled { get; set; }

        public bool Alarmed { get; set; }

        /// <summary>
        /// 报警间隔
        /// </summary>
        public TimeSpan AlarmInterval { get; set; } = TimeSpan.FromHours(1);

        public string AlarmNotifyFormat { get; set; }

        public bool CanAlarmRecover { get; set; }

        public string ClassName { get; set; }

        /// <summary>
        /// 第一个无效点时间
        /// </summary>
        public DateTime FirstInvalidTime { get; set; }

        /// <summary>
        /// 异常值数量
        /// </summary>
        public int InvalidValueCount { get; set; }

        /// <summary>
        /// 上次报警时间
        /// </summary>
        public DateTime LastAlarmTime { get; set; } = DateTime.MinValue;

        public string PropertyName { get; set; }

        public string RecoverNotifyFormat { get; set; }

        public bool IsTimeRangeAlarm { get; set; }

        /// <summary>
        /// 报警启用开始时间段
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// 报警启用结束时间段
        /// </summary>
        public TimeSpan EndTime { get; set; }

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

        public string Format(string pattern, Dictionary<string, object> variableList)
        {
            return string.IsNullOrEmpty(pattern) || variableList == null
                ? pattern
                : PlaceholderRegex.Replace(pattern, match =>
                {
                    var key = match.Groups[1].Value;
                    var format = match.Groups[2].Success ? match.Groups[2].Value : null;

                    if (!variableList.TryGetValue(key, out var value) || value == null)
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

        public virtual Dictionary<string, object> GetVariableList(double value)
        {
            return [];
        }

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

        public override Dictionary<string, object> GetVariableList(double value)
        {
            return new()
            {
                { "Value", value },
                { "Min", Min },
                { "Max", Max },
                { "FirstInvalidTime", FirstInvalidTime },
                { "LastAlarmTime", LastAlarmTime },
                { "InvalidValueCount", InvalidValueCount },
            };
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

        public override Dictionary<string, object> GetVariableList(double value)
        {
            return new()
            {
                { "Value", value },
                { "MaxDelta", MaxDelta },
                { "FirstInvalidTime", FirstInvalidTime },
                { "LastAlarmTime", LastAlarmTime },
                { "InvalidValueCount", InvalidValueCount },
            };
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

        public override Dictionary<string, object> GetVariableList(double value)
        {
            return new()
            {
                { "Value", value },
                { "Threshold", Threshold },
                { "FirstInvalidTime", FirstInvalidTime },
                { "LastAlarmTime", LastAlarmTime },
                { "InvalidValueCount", InvalidValueCount },
            };
        }
    }
}