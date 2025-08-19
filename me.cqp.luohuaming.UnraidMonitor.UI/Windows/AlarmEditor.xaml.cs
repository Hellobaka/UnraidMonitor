using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.UI.Models;
using System;
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

        public AlarmType AlarmType { get; set; } = AlarmType.RangeAlarm;

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadPropertyInfo();
        }

        private void AlarmType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RangeAlarmPropertyContainer.Visibility = AlarmInstance is RangeAlarmRule ? Visibility.Visible : Visibility.Collapsed;
            RateOfChangeAlarmPropertyContainer.Visibility = AlarmInstance is RateOfChangeAlarmRule ? Visibility.Visible : Visibility.Collapsed;
            ThresholdAlarmPropertyContainer.Visibility = AlarmInstance is ThresholdAlarmRule ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ReloadPropertyInfo()
        {
            var monitorItemType = AlarmInstance?.ClassName ?? "";
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "me.cqp.luohuaming.UnraidMonitor.PublicInfos");
            var modelTypes = assembly?.GetType($"me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.{monitorItemType}");
            if (modelTypes == null)
            {
                BindingEditor.ShowError($"未找到模型类型 {monitorItemType}，请检查插件版本");
                DialogResult = false;
                Close();
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
