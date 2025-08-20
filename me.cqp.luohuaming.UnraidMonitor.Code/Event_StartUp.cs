using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.Interface;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using System;
using System.IO;
using System.Reflection;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Handler;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;

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
            Directory.CreateDirectory(MainSave.UnraidMonitorImageSavePath);

            e.CQLog.Info("初始化", "加载配置");
            AppConfig appConfig = new(Path.Combine(MainSave.AppDirectory, "Config.json"));
            appConfig.LoadConfig();
            appConfig.EnableAutoReload();
            CommandIntervalConfig intervalConfig = new(Path.Combine(MainSave.AppDirectory, "Interval.json"));
            intervalConfig.LoadConfig();
            intervalConfig.EnableAutoReload();
            if (!Directory.Exists(Path.Combine(MainSave.AppDirectory, "images")))
            {
                e.CQLog.Warning("初始化", $"图片文件夹缺失，可能没有放置数据包");
            }
            e.CQLog.Info("初始化", "加载指令");
            string path = Path.Combine(MainSave.AppDirectory, "Commands.json");
            if (File.Exists(path))
            {
                try
                {
                    MainSave.Commands = JsonConvert.DeserializeObject<List<Commands>>(File.ReadAllText(path));
                }
                catch (Exception ex)
                {
                    e.CQLog.Error("初始化", $"加载指令时发生异常：{ex}");
                }
            }

            e.CQLog.Info("初始化", $"加载了 {MainSave.Commands?.Count ?? 0} 条指令");

            e.CQLog.Info("初始化", "启动数据采集线程");
            try
            {
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
                MainSave.MonitorAPI?.StartMonitor();
            }
            catch (Exception ex)
            {
                e.CQLog.Error("初始化", $"加载数据采集时发生异常：{ex}");
            }
            e.CQLog.Info("初始化", "加载Alarm规则");
            AlarmManager.LoadRules(Path.Combine(MainSave.AppDirectory, "AlarmRules.json"));
            AlarmManager.OnAlarmPost += AlarmManager_OnAlarmPost;
            AlarmManager.OnAlarmRecover += AlarmManager_OnAlarmRecover;
            e.CQLog.Info("初始化", $"加载了 {AlarmManager.Instance.Rules.Count} 条Alarm规则");
            e.CQLog.InfoSuccess("初始化", "初始化完成");
        }

        private void AlarmManager_OnAlarmRecover(AlarmRuleBase alarm, string message)
        {
            MainSave.CQLog.Info("Alarm恢复", $"{alarm.Name} 恢复正常");
            if (!AppConfig.EnableAlarmRecoveryNotice)
            {
                return;
            }

            foreach (var group in AppConfig.AlarmNoticeGroupList)
            {
                try
                {
                    MainSave.CQApi.SendGroupMessage(group, message);

                    Thread.Sleep(AppConfig.AlarmNoticeDelay * 1000);
                }
                catch (Exception ex)
                {
                    MainSave.CQLog.Error("Alarm恢复通知", $"发送报警信息到群 {group} 时发生异常：{ex}");
                }
            }
        }

        private void AlarmManager_OnAlarmPost(AlarmRuleBase alarm, string message)
        {
            MainSave.CQLog.Info("Alarm抛出", $"{alarm.Name} 发生异常");
            if (!AppConfig.EnableAlarmPostNotice)
            {
                return;
            }
            foreach (var group in AppConfig.AlarmNoticeGroupList)
            {
                try
                {
                    MainSave.CQApi.SendGroupMessage(group, message);

                    Thread.Sleep(AppConfig.AlarmNoticeDelay * 1000);
                }
                catch (Exception ex)
                {
                    MainSave.CQLog.Error("Alarm通知", $"发送报警信息到群 {group} 时发生异常：{ex}");
                }
            }
        }
    }
}
