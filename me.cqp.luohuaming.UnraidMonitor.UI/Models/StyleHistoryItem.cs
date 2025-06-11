using PropertyChanged;
using System;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Models
{
    [AddINotifyPropertyChangedInterface]
    public class StyleHistoryItem
    {
        public string FileName { get; set; }

        public string FullPath { get; set; }

        public DateTime DateTime { get; set; }
    }
}
