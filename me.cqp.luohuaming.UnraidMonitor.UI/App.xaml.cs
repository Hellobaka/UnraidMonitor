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
            base.OnStartup(e);
            Debug = e.Args.Length > 0 && e.Args[0] == "Debug";
        }
    }
}