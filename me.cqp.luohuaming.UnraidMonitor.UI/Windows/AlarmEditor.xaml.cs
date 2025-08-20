using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.UI.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Windows
{
    /// <summary>
    /// AlarmEditor.xaml 的交互逻辑
    /// </summary>
    public partial class AlarmEditor : Window, INotifyPropertyChanged
    {
        public AlarmEditor()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AlarmRuleBase AlarmInstance { get; set; }

        public Array BoolConditionValues => Enum.GetValues(typeof(BoolCondition));

        public Array AlarmTypeValues => Enum.GetValues(typeof(AlarmType));

        public Array BindingItemValues { get; set; } = Enum.GetValues(typeof(ItemType));

        public ObservableCollection<DisplayKeyValuePair> BindingProperties { get; set; } = [];
        
        public DisplayKeyValuePair AlarmPropertyName { get; set; }

        public ItemType AlarmItemType { get; set; } = ItemType.Unknown;

        public AlarmType AlarmType { get; set; } = AlarmType.RangeAlarm;

        public bool FormLoaded { get; set; }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            AlarmInstance.ClassName = AlarmItemType.ToString();
            AlarmInstance.PropertyName = AlarmPropertyName.Value;
            if (!ValidateAlarmInstance(AlarmInstance, out string error))
            {
                MessageBox.Show(error, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DialogResult = true;
            Close();
        }

        private bool ValidateAlarmInstance(AlarmRuleBase alarm, out string error)
        {
            error = string.Empty;
            if (alarm == null)
            {
                error = "Alarm 不能为 null。";
                return false;
            }
            if (string.IsNullOrWhiteSpace(alarm.Name))
            {
                error = "Alarm 名称不能为空。";
                return false;
            }
            if (string.IsNullOrEmpty(alarm.ClassName) || string.IsNullOrEmpty(alarm.PropertyName))
            {
                error = "请选择有效的绑定属性。";
                return false;
            }
            if (alarm.IsTimeRangeAlarm && (alarm.EndTime <= alarm.StartTime))
            {
                error = "结束时间不能早于开始时间。";
                return false;
            }
            if (alarm is RangeAlarmRule range
                && (range.Min >= range.Max))
            {
                error = "最小值不能大于或等于最大值。";
                return false;
            }
            if (alarm is RateOfChangeAlarmRule rateOfChange
                && (rateOfChange.MaxDelta <= 0))
            {
                error = "变化率必须大于零。";
                return false;
            }
            return true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (AlarmInstance == null)
            {
                AlarmInstance = new RangeAlarmRule();
            }
            ReloadPropertyInfo();
            RebuildVariableButton();
            AlarmType_SelectionChanged(null, null);

            AlarmItemType = Enum.TryParse(AlarmInstance.ClassName, out ItemType type) ? type : ItemType.Unknown;
            AlarmPropertyName = BindingProperties.FirstOrDefault(x => x.Value == AlarmInstance.PropertyName) ?? new DisplayKeyValuePair
            {
                Key = "请选择属性",
                Value = ""
            };

            OnPropertyChanged(nameof(AlarmInstance));
            OnPropertyChanged(nameof(AlarmItemType));
            OnPropertyChanged(nameof(AlarmPropertyName));

            FormLoaded = true;
        }

        private void AlarmType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AlarmRuleBase alarm = AlarmType switch
            {
                AlarmType.RateOfChangeAlarm => new RateOfChangeAlarmRule(),
                AlarmType.ThresholdAlarm => new ThresholdAlarmRule(),
                _ => new RangeAlarmRule(),
            };
            if (FormLoaded && AlarmInstance != null)
            {
                AlarmInstance.CloneBase(alarm);
                AlarmInstance = alarm;
            }

            RangeAlarmPropertyContainer.Visibility = AlarmInstance is RangeAlarmRule ? Visibility.Visible : Visibility.Collapsed;
            RateOfChangeAlarmPropertyContainer.Visibility = AlarmInstance is RateOfChangeAlarmRule ? Visibility.Visible : Visibility.Collapsed;
            ThresholdAlarmPropertyContainer.Visibility = AlarmInstance is ThresholdAlarmRule ? Visibility.Visible : Visibility.Collapsed;

            RebuildVariableButton();
        }

        private void RebuildVariableButton()
        {
            if (AlarmInstance == null)
            {
                return;
            }
            AlarmPostFormatContainer.Children.Clear();
            AlarmRecoveryFormatContainer.Children.Clear();

            List<(string display, string value)> alarmPostButtons = [];
            List<(string display, string value)> alarmRecoveryButtons = [];
            foreach (var variable in AlarmInstance.GetVariableList(0))
            {
                alarmPostButtons.Add(variable.Key);
                alarmRecoveryButtons.Add(variable.Key);
            }

            foreach (var (display, value) in alarmPostButtons)
            {
                Button button = new()
                {
                    Content = display,
                    Tag = value
                };
                button.Click += (sender, _)=>
                {
                    if (AlarmInstance == null || sender is not Button b)
                    {
                        return;
                    }
                    if (AlarmInstance.AlarmNotifyFormat == null)
                    {
                        AlarmInstance.AlarmNotifyFormat = "";
                    }
                    AlarmInstance.AlarmNotifyFormat += $"%{b.Tag}%";
                    OnPropertyChanged(nameof(AlarmInstance));
                };
                AlarmPostFormatContainer.Children.Add(button);
            }
            foreach (var (display, value) in alarmRecoveryButtons)
            {
                Button button = new()
                {
                    Content = display,
                    Tag = value
                };
                button.Click += (sender, _) =>
                {
                    if (AlarmInstance == null || sender is not Button b)
                    {
                        return;
                    }
                    if (AlarmInstance.RecoverNotifyFormat == null)
                    {
                        AlarmInstance.RecoverNotifyFormat = "";
                    }
                    AlarmInstance.RecoverNotifyFormat += $"%{b.Tag}%";
                    OnPropertyChanged(nameof(AlarmInstance));
                };
                AlarmRecoveryFormatContainer.Children.Add(button);
            }
        }

        private void ReloadPropertyInfo()
        {
            BindingProperties.Clear();

            var monitorItemType = AlarmItemType.ToString() ?? "";
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "me.cqp.luohuaming.UnraidMonitor.PublicInfos");
            var modelTypes = assembly?.GetType($"me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.{monitorItemType}");
            if (modelTypes == null)
            {
                return;
            }
            foreach (PropertyInfo propertyInfo in modelTypes.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                string displayName = Attribute.GetCustomAttribute(propertyInfo, typeof(DescriptionAttribute)) is DescriptionAttribute attr ? attr.Description : propertyInfo.Name;
                var item = new DisplayKeyValuePair()
                {
                    Key = displayName,
                    Value = propertyInfo.Name
                };
                BindingProperties.Add(item);
            }
        }

        private void ItemType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReloadPropertyInfo();
        }
    }
}
