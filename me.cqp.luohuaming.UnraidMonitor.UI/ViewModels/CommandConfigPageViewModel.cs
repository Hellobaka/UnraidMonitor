using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using PropertyChanged;
using System.Collections.ObjectModel;

namespace me.cqp.luohuaming.UnraidMonitor.UI.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class CommandConfigPageViewModel
    {
        public ObservableCollection<UnraidCommandConfig> CommandConfigs { get; set; } = new ObservableCollection<UnraidCommandConfig>();

        public CommandConfigPageViewModel()
        {
            // 可在此处加载默认配置或从文件导入
        }
    }
}