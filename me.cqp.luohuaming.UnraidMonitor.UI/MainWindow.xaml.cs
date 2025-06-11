using HandyControl.Controls;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Handler;
using me.cqp.luohuaming.UnraidMonitor.UI.Models;
using me.cqp.luohuaming.UnraidMonitor.UI.Windows;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
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

        }

        private void CreateFileButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

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
                StyleHistoryListBox.SelectedItem = null;
            }
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
    }
}