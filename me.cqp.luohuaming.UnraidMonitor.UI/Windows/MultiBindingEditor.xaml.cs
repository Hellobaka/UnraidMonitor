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

        public MultiBindingEditor(string selectedItem, string selectedModel)
        {
            InitializeComponent();
            DataContext = this;
            this.selectedItem = selectedItem;
            this.selectedModel = selectedModel;
        }

        private string selectedItem;
        private string selectedModel;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<DisplayKeyValuePair> ModelProperties { get; set; } = [];

        public ObservableCollection<DisplayKeyValuePair> ItemProperties { get; set; } = [];

        public DrawingItemBase DrawingItemBase { get; set; }

        public ItemType MonitorItemType { get; set; } = ItemType.Unknown;

        public Array BindingNumberConverterValues { get; set; } = Enum.GetValues(typeof(NumberConverter));

        public Array BindingValueTypeValues { get; set; } = Enum.GetValues(typeof(PublicInfos.Drawing.ValueType));

        public DisplayKeyValuePair SelectedModel { get; set; }

        public DisplayKeyValuePair SelectedItem { get; set; }

        public NumberConverter SelectedNumberConverter { get; set; }

        public PublicInfos.Drawing.ValueType SelectedValueType { get; set; }

        public double SelectedDiffUnit { get; set; }

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

            foreach (PropertyInfo propertyInfo in DrawingItemBase.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                bool canBind = Attribute.GetCustomAttribute(propertyInfo, typeof(BindableAttribute)) is BindableAttribute bindable && bindable.Bindable;
                if (!canBind)
                {
                    continue;
                }
                string displayName = Attribute.GetCustomAttribute(propertyInfo, typeof(DescriptionAttribute)) is DescriptionAttribute attr ? attr.Description : propertyInfo.Name;
                var item = new DisplayKeyValuePair()
                {
                    Key = displayName,
                    Value = propertyInfo.Name
                };
                ItemProperties.Add(item);
                if (item.Value == selectedItem)
                {
                    SelectedItem = item;
                }
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
                var item = new DisplayKeyValuePair()
                {
                    Key = displayName,
                    Value = propertyInfo.Name
                };
                ModelProperties.Add(item);
                if (item.Value == selectedModel)
                {
                    SelectedModel = item;
                }
            }
        }

        private void BindingValueType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            DiffUnitContainer.IsEnabled = SelectedValueType.ToString().StartsWith("Diff");
        }
    }
}
