using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace me.cqp.luohuaming.UnraidMonitor.UI
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static bool Debug { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            base.OnStartup(e);
            Debug = e.Args.Length > 0 && e.Args[0] == "Debug";
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            MainSave.CQLog?.Error("异步UI异常", e.Exception.ToString());
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MainSave.CQLog?.Error("UI异常", ex.ToString());
            }
            else
            {
                MainSave.CQLog?.Error("UI异常", e.ExceptionObject?.ToString() ?? "未知错误");
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            MainSave.CQLog?.Error("UI线程异常", e.Exception.ToString());
        }
    }
}