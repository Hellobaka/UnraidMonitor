using PropertyChanged;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Models
{
    [AddINotifyPropertyChangedInterface]
    public class PictureItem
    {
        public string ImagePath { get; set; } = "";

        public bool IsAddButton { get; set; }
    }
}
