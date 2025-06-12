using HandyControl.Tools.Extension;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using me.cqp.luohuaming.UnraidMonitor.UI.Controls;
using me.cqp.luohuaming.UnraidMonitor.UI.Models;
using me.cqp.luohuaming.UnraidMonitor.UI.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Windows
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : HandyControl.Controls.Window, INotifyPropertyChanged
    {
        public Settings()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<StyleCommandWrapper> StyleCommands { get; set; } = [];

        private static bool TryParse(string input, Type type, out object value)
        {
            value = input;
            if (type.Name == "Int32")
            {
                if (int.TryParse(input, out int v))
                {
                    value = v;
                }
                else
                {
                    return false;
                }
            }
            else if (type.Name == "UInt16")
            {
                if (ushort.TryParse(input, out ushort v))
                {
                    value = v;
                }
                else
                {
                    return false;
                }
            }
            else if (type.Name == "Int64")
            {
                if (long.TryParse(input, out long v))
                {
                    value = v;
                }
                else
                {
                    return false;
                }
            }
            else if (type.Name == "Single")
            {
                if (float.TryParse(input, out float v))
                {
                    value = v;
                }
                else
                {
                    return false;
                }
            }
            else if (type.Name == "Double")
            {
                if (double.TryParse(input, out double v))
                {
                    value = v;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private void BlackListAddButton_Click(object sender, RoutedEventArgs e)
        {
            ListAddButtonHandler(BlackListAdd, BlackList, true);
        }

        private void BlackListRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ListRemoveButtonHandler(BlackList);
        }

        private void GetAndSetConfigFromStackPanel(PropertyInfo[] properties, StackPanel container, ConfigBase config)
        {
            foreach (UIElement item in container.Children)
            {
                if (item is TextBox textBox)
                {
                    var property = properties.FirstOrDefault(x => x.Name == textBox.Name);
                    if (property != null && TryParse(textBox.Text, property.PropertyType, out object value))
                    {
                        property.SetValue(null, value);
                        config.SetConfig(textBox.Name, value);
                    }
                }
                else if (item is StackPanel stackPanel)
                {
                    foreach (UIElement child in stackPanel.Children)
                    {
                        if (child is ToggleButton checkBox)
                        {
                            var property = properties.FirstOrDefault(x => x.Name == checkBox.Name);
                            property?.SetValue(null, checkBox.IsChecked);
                            config.SetConfig(checkBox.Name, checkBox.IsChecked);
                        }
                    }
                }
                else if (item is ListBox listbox)
                {
                    var property = properties.FirstOrDefault(x => x.Name == listbox.Name);
                    if (property == null)
                    {
                        Debugger.Break();
                        continue;
                    }
                    var list = property?.GetValue(null, null);
                    if (list is List<long> l)
                    {
                        l.Clear();
                        foreach (var i in listbox.Items)
                        {
                            l.Add(long.Parse(i.ToString()));
                        }
                        config.SetConfig(listbox.Name, l);
                    }
                    else if (list is List<string> l2)
                    {
                        l2.Clear();
                        foreach (var i in listbox.Items)
                        {
                            l2.Add(i.ToString());
                        }
                        config.SetConfig(listbox.Name, l2);
                    }
                }
                else if (item is ComboBox comboBox)
                {
                    var property = properties.FirstOrDefault(x => x.Name == comboBox.Name);
                    if (property == null)
                    {
                        Debugger.Break();
                        continue;
                    }
                    config.SetConfig(comboBox.Name, (comboBox.SelectedItem as ComboBoxItem).Tag.ToString());
                }
            }
        }

        private void GroupListAddButton_Click(object sender, RoutedEventArgs e)
        {
            ListAddButtonHandler(GroupListAdd, GroupList, true);
        }

        private void GroupListRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ListRemoveButtonHandler(GroupList);
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var uri = e.Uri;
            Process.Start(uri.ToString());
        }

        private void PersonListAddButton_Click(object sender, RoutedEventArgs e)
        {
            ListAddButtonHandler(PersonListAdd, PersonList, true);
        }

        private void PersonListRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ListRemoveButtonHandler(PersonList);
        }

        private void SaveConfigButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var properties = typeof(AppConfig).GetProperties(BindingFlags.Static | BindingFlags.Public);
                if (VerifyInput(properties, AppConfigContainer, out string err)
                    && VerifyInput(properties, APIContainer, out err)
                    && VerifyInput(properties, AuthContainer, out err))
                {
                    AppConfig.Instance.DisableAutoReload();
                    GetAndSetConfigFromStackPanel(properties, AppConfigContainer, AppConfig.Instance);
                    GetAndSetConfigFromStackPanel(properties, AuthContainer, AppConfig.Instance);
                    AppConfig.Instance.EnableAutoReload();

                    CommandIntervalConfig.Instance.DisableAutoReload();
                    GetAndSetConfigFromStackPanel(properties, APIContainer, CommandIntervalConfig.Instance);
                    CommandIntervalConfig.Instance.EnableAutoReload();

                    MainSave.Commands = StyleCommands.Select(x => x.Raw).ToList();
                    MainWindow.ShowInfo("配置保存成功");
                }
                else
                {
                    MainWindow.ShowError(err);
                }
            }
            catch (Exception ex)
            {
                MainSave.CQLog?.Info("配置保存", $"{ex.Message}\n{ex.StackTrace}");
                MainWindow.ShowError("配置保存失败，查看日志排查问题");
            }
            finally
            {
                CommandIntervalConfig.Instance.EnableAutoReload();
                AppConfig.Instance.EnableAutoReload();
            }
        }

        private void SetConfigToStackPanel(PropertyInfo[] properties, StackPanel container)
        {
            try
            {
                foreach (UIElement item in container.Children)
                {
                    if (item is TextBox textBox)
                    {
                        var property = properties.FirstOrDefault(x => x.Name == textBox.Name);
                        if (property != null)
                        {
                            textBox.Text = property.GetValue(null)?.ToString() ?? "";
                        }
                    }
                    else if (item is StackPanel stackPanel)
                    {
                        foreach (UIElement child in stackPanel.Children)
                        {
                            if (child is ToggleButton checkBox)
                            {
                                var property = properties.FirstOrDefault(x => x.Name == checkBox.Name);
                                if (property != null)
                                {
                                    checkBox.IsChecked = (bool)property.GetValue(null);
                                }
                            }
                        }
                    }
                    else if (item is ListBox listbox)
                    {
                        var property = properties.FirstOrDefault(x => x.Name == listbox.Name);
                        if (property == null)
                        {
                            Debugger.Break();
                            continue;
                        }
                        listbox.Items.Clear();
                        var list = property?.GetValue(null, null);
                        if (list is List<long> l)
                        {
                            foreach (var i in l)
                            {
                                listbox.Items.Add(i);
                            }
                        }
                        else if (list is List<string> l2)
                        {
                            foreach (var i in l2)
                            {
                                listbox.Items.Add(i);
                            }
                        }
                    }
                    else if (item is ComboBox combobox)
                    {
                        var property = properties.FirstOrDefault(x => x.Name == combobox.Name);
                        if (property == null)
                        {
                            Debugger.Break();
                            continue;
                        }
                        var v = property?.GetValue(null, null);
                        combobox.SelectedIndex = (int)v;
                    }
                }
            }
            catch
            { }
        }

        private void TextTemplate_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                Clipboard.SetText((sender as TextBlock).Tag.ToString());
            }
            catch
            {
                MainWindow.ShowError("复制文本失败");
            }
        }

        private bool VerifyInput(PropertyInfo[] properties, StackPanel container, out string err)
        {
            err = "";
            foreach (UIElement item in container.Children)
            {
                if (item is TextBox textBox)
                {
                    var property = properties.FirstOrDefault(x => x.Name == textBox.Name);
                    if (property != null && !TryParse(textBox.Text, property.PropertyType, out _))
                    {
                        err = $"{textBox.Name} 的 {textBox.Text} 输入无法转换为有效配置";
                        return false;
                    }
                }
            }
            return true;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var properties = typeof(CommandIntervalConfig).GetProperties(BindingFlags.Static | BindingFlags.Public);
            SetConfigToStackPanel(properties, APIContainer);
            properties = typeof(AppConfig).GetProperties(BindingFlags.Static | BindingFlags.Public);
            SetConfigToStackPanel(properties, AppConfigContainer);
            SetConfigToStackPanel(properties, AuthContainer);

            string commandPath = System.IO.Path.Combine(MainSave.AppDirectory, "Commands.json");
            StyleCommands = [];
            if (File.Exists(commandPath))
            {
                try
                {
                    var commands = JsonConvert.DeserializeObject<List<Commands>>(File.ReadAllText(commandPath));
                    if (commands != null)
                    {
                        foreach(var item in commands)
                        {
                            StyleCommands.Add(new()
                            {
                                Command = item.Command,
                                Raw = item,
                                StylePath = item.StylePath
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MainSave.CQLog?.Error("加载样式指令", $"加载指令时发生异常：{ex}");
                    MainWindow.ShowError("加载样式指令失败，请查看日志");
                }
            }
        }

        private void ListRemoveButtonHandler(ListBox listBox)
        {
            if (listBox.SelectedIndex < 0)
            {
                MainWindow.ShowError("请选择一项");
                return;
            }
            listBox.Items.RemoveAt(listBox.SelectedIndex);
        }

        private void ListAddButtonHandler(TextBox textBox, ListBox listBox, bool longCheck = false)
        {
            if (!string.IsNullOrEmpty(textBox.Text)
                && (!longCheck || long.TryParse(textBox.Text, out _)))
            {
                bool duplicate = false;
                foreach (var item in listBox.Items)
                {
                    if (item.ToString() == textBox.Text)
                    {
                        duplicate = true;
                        break;
                    }
                }
                if (duplicate)
                {
                    MainWindow.ShowError("已存在相同项");
                    return;
                }
                listBox.Items.Add(textBox.Text);
                textBox.Clear();
            }
            else
            {
                MainWindow.ShowError("输入内容格式错误");
            }
        }

        private async void StyleCommandNewButton_Click(object sender, RoutedEventArgs e)
        {
            var (success, command) = await HandyControl.Controls.Dialog.Show<AddOrUpdateStyleCommand>()
                .Initialize<AddOrUpdateStyleCommandViewModel>(vm => vm.Result = (false, new()))
                .GetResultAsync<(bool success, StyleCommandWrapper command)>();
            if (!success || !CheckStyleCanAdd(command))
            {
                return;
            }
            StyleCommands.Add(command);
            UpdateStyleCommand();
        }

        private bool CheckStyleCanAdd(StyleCommandWrapper command)
        {
            command.Command = command.Command.Trim();
            command.StylePath = command.StylePath.Trim();
            if (StyleCommands.Any(x => x != command && x.StylePath == command.StylePath))
            {
                MainWindow.ShowError("已存在同路径指令，无法添加");
                return false;
            }
            if (StyleCommands.Any(x => x != command && x.Command == command.Command))
            {
                MainWindow.ShowError("已存在相同指令，无法添加");
                return false;
            }
            if (!File.Exists(command.StylePath))
            {
                MainWindow.ShowError("样式文件不存在，请检查路径");
                return false;
            }
            if (!string.IsNullOrEmpty(command.Command))
            {
                if (command.Command.Length > 100)
                {
                    MainWindow.ShowError("指令长度超过限制，请检查");
                    return false;
                }
                if (command.Command.Contains('\n') || command.Command.Contains('\r'))
                {
                    MainWindow.ShowError("指令中不能包含换行符");
                    return false;
                }
            }
            else
            {
                MainWindow.ShowError("指令不能为空，请检查");
                return false;
            }
            return true;
        }

        private async void StyleCommandEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (StyleCommandList.SelectedItem == null || StyleCommandList.SelectedItem is not StyleCommandWrapper c)
            {
                MainWindow.ShowError("请选择一项");
                return;
            }
            var (success, command) = await HandyControl.Controls.Dialog.Show<AddOrUpdateStyleCommand>()
                .Initialize<AddOrUpdateStyleCommandViewModel>(vm => vm.Result = (false, c))
                .GetResultAsync<(bool success, StyleCommandWrapper command)>();
            if (!success || !CheckStyleCanAdd(command))
            {
                return;
            }
            UpdateStyleCommand();

            OnPropertyChanged(nameof(StyleCommands));
        }

        private async void StyleCommandDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (StyleCommandList.SelectedItem == null || StyleCommandList.SelectedItem is not StyleCommandWrapper command)
            {
                MainWindow.ShowError("请选择一项");
                return;
            }
            if (!await MainWindow.ShowConfirmAsync("确定要删除所选的样式指令吗？"))
            {
                return;
            }
            StyleCommands.Remove(command);
            UpdateStyleCommand();
        }

        private void UpdateStyleCommand()
        {
            var list = StyleCommands.Select(x => x.Raw);
            File.WriteAllText(System.IO.Path.Combine(MainSave.AppDirectory, "Commands.json"), JsonConvert.SerializeObject(list));
            //MainWindow.ShowInfo("保存成功");
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
