using HandyControl.Controls;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Handler;
using me.cqp.luohuaming.UnraidMonitor.UI.Models;
using me.cqp.luohuaming.UnraidMonitor.UI.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            if (App.Debug)
            {
                LoadDebug();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LoadDebug()
        {
            MainSave.AppDirectory = Path.GetFullPath(".");
            MainSave.ImageDirectory = CommonHelper.GetAppImageDirectory();

            AppConfig appConfig = new(Path.Combine(MainSave.AppDirectory, "Config.json"));
            appConfig.LoadConfig();
            appConfig.EnableAutoReload();

            CommandIntervalConfig intervalConfig = new(Path.Combine(MainSave.AppDirectory, "Interval.json"));
            intervalConfig.LoadConfig();
            intervalConfig.EnableAutoReload();

            try
            {
                if (AppConfig.MonitorOSType.Equals("Linux", StringComparison.OrdinalIgnoreCase))
                {
                    MainSave.MonitorAPI = new Linux();
                }
                else if (AppConfig.MonitorOSType.Equals("Windows", StringComparison.OrdinalIgnoreCase))
                {
                    MainSave.MonitorAPI = new PublicInfos.Handler.Windows();
                }
                else
                {
                    ShowError($"不支持的操作系统类型 {AppConfig.MonitorOSType}，插件不能使用");
                }
                //MainSave.MonitorAPI?.StartMonitor();
            }
            catch (Exception ex)
            {
                ShowError($"加载数据采集时发生异常：{ex}");
            }
        }

        public ObservableCollection<StyleHistoryItem> StyleHistories { get; set; } = [
            new(){
                DateTime = DateTime.Now,
                FileName = "default.style",
                FullPath = @"D:\Code\UnraidMonitor\me.cqp.luohuaming.UnraidMonitor.PublicInfos\bin\x86\Debug\default.style"
            },
            new(){
                DateTime = DateTime.Now.AddDays(-1),
                FileName = "layout.style",
                FullPath = @"D:\Code\UnraidMonitor\me.cqp.luohuaming.UnraidMonitor.PublicInfos\bin\x86\Debug\layout.style"
            },
        ];

        private void OpenFileButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenFileDialog dialog = new();
            dialog.Title = "打开样式文件";
            dialog.Filter = "样式文件|*.style|所有文件|*.*";
            if (dialog.ShowDialog() ?? false)
            {
                string path = dialog.FileName;
                SaveActiveHistory(new StyleHistoryItem
                {
                    DateTime = DateTime.Now,
                    FileName = Path.GetFileName(path),
                    FullPath = path
                });
                LoadWorkbench(path);
            }
        }

        private void CreateFileButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CreateStyle createStyle = new();
            createStyle.Owner = this;
            Hide();
            createStyle.ShowDialog();
            if (createStyle.DialogResult ?? false)
            {
                string path = createStyle.SavedStylePath;
                LoadWorkbench(path);
            }
            else
            {
                Show();
            }
        }

        private void SettingButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Settings settings = new();
            settings.Owner = this;
            settings.Show();
        }

        private void StyleHistoryListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (StyleHistoryListBox.SelectedItem is StyleHistoryItem item)
            {
                SaveActiveHistory(item);
                LoadWorkbench(item.FullPath);
                StyleHistoryListBox.SelectedItem = null;
            }
        }

        private async Task LoadActiveHistory()
        {
            StyleHistories = [];

            string historyFilePath = Path.Combine(MainSave.AppDirectory, "history.json");
            if (File.Exists(historyFilePath))
            {
                List<StyleHistoryItem> historyItems = await Task.Run(() =>
                {
                    try
                    {
                        string json = File.ReadAllText(historyFilePath);
                        return JsonConvert.DeserializeObject<List<StyleHistoryItem>>(json) ?? [];
                    }
                    catch { }
                    return [];
                });
                foreach (var item in historyItems.OrderByDescending(x => x.DateTime))
                {
                    StyleHistories.Add(item);
                }
            }
        }

        private void SaveActiveHistory(StyleHistoryItem item)
        {
            item.DateTime = DateTime.Now;
            if (StyleHistories.Contains(item))
            {
                OnPropertyChanged(nameof(StyleHistories));
            }
            else
            {
                StyleHistories.Add(item);
            }

            try
            {
                File.WriteAllText(Path.Combine(MainSave.AppDirectory, "history.json"), JsonConvert.SerializeObject(StyleHistories));
            }
            catch (Exception e)
            {
                ShowError($"保存历史记录时发生错误: {e.Message}");
            }
        }

        private void LoadWorkbench(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                ShowError("样式文件不存在或路径无效");
                return;
            }
            Workbench workbench = new(path);
            Hide();
            workbench.Show();
        }

        public static void ShowInfo(string content)
        {
            Growl.Info(content);
        }

        public static void ShowError(string content)
        {
            Growl.Error(content);
        }

        public static Task<bool> ShowConfirmAsync(string content)
        {
            var tcs = new TaskCompletionSource<bool>();

            Growl.Ask(content, (isConfirmed) =>
            {
                tcs.SetResult(isConfirmed);
                return true;
            });

            return tcs.Task;
        }

        private async void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await LoadActiveHistory();
            EmptyHint.Visibility = StyleHistories.Count > 0 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }
    }
}