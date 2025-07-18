using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models;
using me.cqp.luohuaming.UnraidMonitor.UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Windows
{
    /// <summary>
    /// MultiBindingEditor.xaml 的交互逻辑
    /// </summary>
    public partial class MultiBindingEditor : Window, INotifyPropertyChanged
    {
        public MultiBindingEditor()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<DisplayKeyValuePair> ModelProperties { get; set; } = [];

        public ObservableCollection<DisplayKeyValuePair> ItemProperties { get; set; } = [];

        public DrawingItemBase DrawingItemBase { get; set; }

        public ItemType MonitorItemType { get; set; } = ItemType.Unknown;

        public MultipleBinding MultipleBinding { get; set; }

        public Array BindingNumberConverterValues { get; set; } = Enum.GetValues(typeof(NumberConverter));

        public Array BindingItemValues { get; set; } = Enum.GetValues(typeof(ItemType));

        public Array BindingTimeRangeValues { get; set; } = Enum.GetValues(typeof(TimeRange));

        public Array BindingValueTypeValues { get; set; } = Enum.GetValues(typeof(PublicInfos.Drawing.ValueType));

        public string Path { get; set; }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DrawingItemBase == null)
            {
                BindingEditor.ShowError("元素无效，检查参数");
                DialogResult = false;
                Close();
            }
            if (MonitorItemType == ItemType.Unknown)
            {
                BindingEditor.ShowError("模型无效，检查参数");
                DialogResult = false;
                Close();
            }
            MultipleBinding ??= new();

            foreach (PropertyInfo propertyInfo in DrawingItemBase.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                string displayName = Attribute.GetCustomAttribute(propertyInfo, typeof(DescriptionAttribute)) is DescriptionAttribute attr ? attr.Description : propertyInfo.Name;

                ItemProperties.Add(new()
                {
                    Key = displayName,
                    Value = propertyInfo.Name
                });
            }
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "me.cqp.luohuaming.UnraidMonitor.PublicInfos");
            var modelTypes = assembly?.GetType($"me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.{MonitorItemType}");
            if (modelTypes == null)
            {
                BindingEditor.ShowError($"未找到模型类型 {MonitorItemType}，请检查插件版本");
                DialogResult = false;
                Close();
                return;
            }

            foreach (PropertyInfo propertyInfo in modelTypes.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                string displayName = Attribute.GetCustomAttribute(propertyInfo, typeof(DescriptionAttribute)) is DescriptionAttribute attr ? attr.Description : propertyInfo.Name;

                ModelProperties.Add(new()
                {
                    Key = displayName,
                    Value = propertyInfo.Name
                });
            }
        }
    }
}
