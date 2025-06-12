using HandyControl.Tools.Extension;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using me.cqp.luohuaming.UnraidMonitor.UI.Models;
using PropertyChanged;
using System;

namespace me.cqp.luohuaming.UnraidMonitor.UI.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class AddOrUpdateStyleCommandViewModel : IDialogResultable<(bool, StyleCommandWrapper)>
    {
        [DoNotNotify]
        public Action CloseAction { get; set; }

        public string StylePath { get; set; }

        public string Command { get; set; }

        public (bool, StyleCommandWrapper) Result { get; set; }

        public RelayCommand SaveCmd => new((_) => 
        { 
            Result.Item2.Command = Command;
            Result.Item2.StylePath = StylePath; 
            Result.Item2.Raw = new()
            {
                Command = Command,
                StylePath = StylePath
            }; 
            Result = (true, Result.Item2); 
            CloseAction?.Invoke(); 
        });

        public RelayCommand CancelCmd => new((_) => 
        { 
            Result = (false, Result.Item2);
            CloseAction?.Invoke(); 
        });
    }
}
