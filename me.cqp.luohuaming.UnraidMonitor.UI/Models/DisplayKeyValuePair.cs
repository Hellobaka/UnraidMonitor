using PropertyChanged;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Models
{
    [AddINotifyPropertyChangedInterface]
    public class DisplayKeyValuePair
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is DisplayKeyValuePair pair)
            {
                return Key == pair.Key && Value == pair.Value;
            }
            return false;
        }
    }
}
