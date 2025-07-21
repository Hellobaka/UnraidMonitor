using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.UI.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Windows
{
    /// <summary>
    /// BindingEditor.xaml 的交互逻辑
    /// </summary>
    public partial class BindingEditor : Window, INotifyPropertyChanged
    {
        public BindingEditor(DrawingItemBase itemBase)
        {
            InitializeComponent();
            DataContext = this;
            ItemBase = itemBase;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Should be Cloned from DrawingItemBase
        /// </summary>
        public Binding CustomBinding { get; set; }

        public ObservableCollection<FlatMultiBindingItem> FlatMultiBindingItems { get; set; }

        public ObservableCollection<DisplayKeyValuePair> Conditions { get; set; }

        public Array BindingItemValues { get; set; } = Enum.GetValues(typeof(ItemType));

        public Array BindingTimeRangeValues { get; set; } = Enum.GetValues(typeof(TimeRange));

        public DrawingItemBase ItemBase { get; set; }

        private void CreateMultiBinding_Click(object sender, RoutedEventArgs e)
        {
            MultiBindingEditor editor = new()
            {
                DrawingItemBase = ItemBase,
                MonitorItemType = CustomBinding.ItemType,
                Owner = this
            };
            if (editor.ShowDialog() ?? false)
            {
                if (string.IsNullOrWhiteSpace(editor.Path) 
                    || string.IsNullOrWhiteSpace(editor.MultipleBinding.Path))
                {
                    BindingEditor.ShowError("绑定路径不能为空，请修改后再添加");
                    return;
                }
                FlatMultiBindingItems.Add(new FlatMultiBindingItem
                {
                    Key = editor.Path,
                    MultipleBinding = editor.MultipleBinding
                });
                UpdateMultiBinding();
                OnPropertyChanged(nameof(FlatMultiBindingItems));
            }
        }

        private void CreateCondition_Click(object sender, RoutedEventArgs e)
        {
            BindingConditionEditor editor = new()
            {
                MonitorItemType = CustomBinding.ItemType,
                Owner = this
            };
            if (editor.ShowDialog() ?? false)
            {
                if (Conditions.Any(x => x.Key == editor.SelectedPath))
                {
                    BindingEditor.ShowError($"条件 {editor.SelectedPath} 已存在，请修改后再添加");
                    return;
                }
                if (string.IsNullOrWhiteSpace(editor.SelectedPath)
                    || string.IsNullOrWhiteSpace(editor.TargetValue))
                {
                    BindingEditor.ShowError("条件路径和值不能为空，请修改后再添加");
                    return;
                }
                Conditions.Add(new DisplayKeyValuePair
                {
                    Key = editor.SelectedPath,
                    Value = editor.TargetValue
                });
                UpdateConditions();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (CustomBinding.BindingPath?.Count == 0)
            {
                if (BindingEditor.ShowConfirmAsync("未设置绑定值，此绑定无法生效，确定要退出？"))
                {
                    DialogResult = false;
                    Close();
                }
                else
                {
                    return;
                }
            }
            DialogResult = true;
            Close();
        }

        private void EditMultiBinding_Click(object sender, RoutedEventArgs e)
        {
            var bindingItem = (FlatMultiBindingItem)((Button)sender).DataContext;
            MultiBindingEditor editor = new()
            {
                DrawingItemBase = ItemBase,
                MonitorItemType = CustomBinding.ItemType,
                MultipleBinding = bindingItem.MultipleBinding,
                Owner = this
            };
            if (editor.ShowDialog() ?? false)
            {
                if (string.IsNullOrWhiteSpace(editor.Path)
                    || string.IsNullOrWhiteSpace(editor.MultipleBinding.Path))
                {
                    BindingEditor.ShowError("绑定路径不能为空，请修改后再添加");
                    return;
                }
                UpdateMultiBinding();
            }
        }

        private void DeleteMultiBinding_Click(object sender, RoutedEventArgs e)
        {
            var bindingItem = (FlatMultiBindingItem)((Button)sender).DataContext;
            if (BindingEditor.ShowConfirmAsync($"确定删除绑定 {bindingItem.Key}？") != true)
            {
                return;
            }
            FlatMultiBindingItems.Remove(bindingItem);
            UpdateMultiBinding();
        }

        private void EditCondition_Click(object sender, RoutedEventArgs e)
        {
            var conditionItem = (DisplayKeyValuePair)((Button)sender).DataContext;
            BindingConditionEditor editor = new()
            {
                MonitorItemType = CustomBinding.ItemType,
                SelectedPath = conditionItem.Key,
                TargetValue = conditionItem.Value,
                Owner = this
            };
            if (editor.ShowDialog() ?? false)
            {
                if (Conditions.Any(x => x.Key == editor.SelectedPath))
                {
                    BindingEditor.ShowError($"条件 {editor.SelectedPath} 已存在，请修改后再添加");
                    return;
                }
                if (string.IsNullOrWhiteSpace(editor.SelectedPath)
                     || string.IsNullOrWhiteSpace(editor.TargetValue))
                {
                    BindingEditor.ShowError("条件路径和值不能为空，请修改后再添加");
                    return;
                }
                Conditions.Remove(conditionItem);
                Conditions.Add(new DisplayKeyValuePair
                {
                    Key = editor.SelectedPath,
                    Value = editor.TargetValue
                });
                UpdateConditions();
            }
        }

        private void DeleteCondition_Click(object sender, RoutedEventArgs e)
        {
            var conditionItem = (DisplayKeyValuePair)((Button)sender).DataContext;
            if (BindingEditor.ShowConfirmAsync($"确定删除条件 {conditionItem.Key}？") != true)
            {
                return;
            }
            Conditions.Remove(conditionItem);
            UpdateConditions();
        }

        private void UpdateConditions()
        {
            CustomBinding.Conditions = [];
            foreach (var item in Conditions)
            {
                CustomBinding.Conditions.Add(item.Key, item.Value);
            }
        }

        private void UpdateMultiBinding()
        {
            CustomBinding.BindingPath = [];
            foreach (var item in FlatMultiBindingItems)
            {
                if (CustomBinding.BindingPath.TryGetValue(item.Key, out var bindings))
                {
                    bindings.Add(item.MultipleBinding);
                }
                else
                {
                    bindings = [item.MultipleBinding];
                    CustomBinding.BindingPath.Add(item.Key, bindings);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CustomBinding ??= new();
            FlatMultiBindingItems = [];
            Conditions = [];
            if (CustomBinding.BindingPath != null)
            {
                foreach (var item in CustomBinding.BindingPath)
                {
                    foreach (var binding in item.Value)
                    {
                        FlatMultiBindingItems.Add(new FlatMultiBindingItem
                        {
                            Key = item.Key,
                            MultipleBinding = binding
                        });
                    }
                }
            }

            if (CustomBinding.Conditions != null)
            {
                foreach (var item in CustomBinding.Conditions)
                {
                    Conditions.Add(new()
                    {
                        Key = item.Key,
                        Value = item.Value
                    });
                }
            }
        }

        public static void ShowError(string content)
        {
            System.Windows.MessageBox.Show(content, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static bool ShowConfirmAsync(string content)
        {
            return System.Windows.MessageBox.Show(content, "询问", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
