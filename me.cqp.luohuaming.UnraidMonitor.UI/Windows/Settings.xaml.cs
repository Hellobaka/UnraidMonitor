using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Windows
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

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
                    && VerifyInput(properties, AuthContainer, out err)
                    && VerifyInput(properties, CommandContainer, out err))
                {
                    AppConfig.Instance.DisableAutoReload();
                    GetAndSetConfigFromStackPanel(properties, AppConfigContainer, AppConfig.Instance);
                    GetAndSetConfigFromStackPanel(properties, CommandContainer, AppConfig.Instance);
                    GetAndSetConfigFromStackPanel(properties, AuthContainer, AppConfig.Instance);
                    AppConfig.Instance.EnableAutoReload();

                    CommandIntervalConfig.Instance.DisableAutoReload();
                    GetAndSetConfigFromStackPanel(properties, APIContainer, CommandIntervalConfig.Instance);
                    CommandIntervalConfig.Instance.EnableAutoReload();
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
            SetConfigToStackPanel(properties, CommandContainer);
            SetConfigToStackPanel(properties, AppConfigContainer);
            SetConfigToStackPanel(properties, AuthContainer);
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

        private void ListAddButtonHandler(TextBox textBox, ListBox listBox, bool longcheck = false)
        {
            if (!string.IsNullOrEmpty(textBox.Text)
                && (!longcheck || long.TryParse(textBox.Text, out _)))
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
    }
}
