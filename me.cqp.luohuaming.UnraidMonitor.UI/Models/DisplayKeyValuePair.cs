using PropertyChanged;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Models
{
    [AddINotifyPropertyChangedInterface]
    public class DisplayKeyValuePair
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
