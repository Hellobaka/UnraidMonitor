using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using PropertyChanged;

namespace me.cqp.luohuaming.UnraidMonitor.UI.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class WorkbenchViewModel
    {
        public bool Debouncing { get; set; }

        public double DebounceValue { get; set; }

        public string? CurrentStylePath { get; set; }

        public DrawingStyle? CurrentStyle { get; set; }

    }
}
