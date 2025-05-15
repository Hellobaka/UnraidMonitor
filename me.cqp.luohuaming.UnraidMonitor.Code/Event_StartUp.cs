using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.Interface;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using System;
using System.IO;
using System.Reflection;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Handler;

namespace me.cqp.luohuaming.UnraidMonitor.Code
{
    public class Event_StartUp : ICQStartup
    {
        public void CQStartup(object sender, CQStartupEventArgs e)
        {
            MainSave.AppDirectory = e.CQApi.AppDirectory;
            MainSave.CQApi = e.CQApi;
            MainSave.CQLog = e.CQLog;
            MainSave.ImageDirectory = CommonHelper.GetAppImageDirectory();
            foreach (var item in Assembly.GetAssembly(typeof(Event_GroupMessage)).GetTypes())
            {
                if (item.IsInterface)
                    continue;
                foreach (var instance in item.GetInterfaces())
                {
                    if (instance == typeof(IOrderModel))
                    {
                        IOrderModel obj = (IOrderModel)Activator.CreateInstance(item);
                        if (obj.ImplementFlag == false)
                            continue;
                        MainSave.Instances.Add(obj);
                    }
                }
            }

            e.CQLog.Info("初始化", "加载配置");
            AppConfig appConfig = new(Path.Combine(MainSave.AppDirectory, "Config.json"));
            appConfig.LoadConfig();
            appConfig.EnableAutoReload();
            CommandIntervalConfig intervalConfig = new(Path.Combine(MainSave.AppDirectory, "Interval.json"));
            intervalConfig.LoadConfig();
            intervalConfig.EnableAutoReload();

            e.CQLog.Info("初始化", "启动监控线程");
            if (AppConfig.MonitorOSType.Equals("Linux", StringComparison.OrdinalIgnoreCase))
            {
                MainSave.MonitorAPI = new Linux(); 
            }
            else if (AppConfig.MonitorOSType.Equals("Windows", StringComparison.OrdinalIgnoreCase))
            {
                MainSave.MonitorAPI = new Windows();
            }
            else
            {
                e.CQLog.Error("初始化", $"不支持的操作系统类型 {AppConfig.MonitorOSType}，插件不能使用");
            }
        }
    }
}
