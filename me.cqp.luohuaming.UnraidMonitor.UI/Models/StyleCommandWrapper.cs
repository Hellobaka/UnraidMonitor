using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using PropertyChanged;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Models
{
    [AddINotifyPropertyChangedInterface]
    public class StyleCommandWrapper
    {
        public string StylePath { get; set; }

        public string Command { get; set; }

        public Commands Raw { get; set; }
    }
}
