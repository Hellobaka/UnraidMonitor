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
    /// BindingConditionEditor.xaml 的交互逻辑
    /// </summary>
    public partial class BindingConditionEditor : Window, INotifyPropertyChanged
    {
        public BindingConditionEditor()
        {
            InitializeComponent();
            DataContext = this;
        }

        public BindingConditionEditor(string path)
        {
            InitializeComponent();
            DataContext = this;
            this.path = path;
        }

        private string path;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ItemType MonitorItemType { get; set; } = ItemType.Unknown;

        public ObservableCollection<DisplayKeyValuePair> AvailablePath { get; set; } = [];

        public DisplayKeyValuePair SelectedPath { get; set; }

        public string TargetValue { get; set; } = "";

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (MonitorItemType == ItemType.Unknown)
            {
                BindingEditor.ShowError("模型无效，检查参数");
                DialogResult = false;
                Close();
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
                AvailablePath.Add(item);
                if(item.Value == path)
                {
                    SelectedPath = item;
                }
            }
        }

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
    }
}
