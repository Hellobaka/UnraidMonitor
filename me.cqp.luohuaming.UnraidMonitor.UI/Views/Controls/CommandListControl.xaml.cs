using me.cqp.luohuaming.UnraidMonitor.UI.ViewModels;
using System.Windows.Controls;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Views.Controls
{
    public partial class CommandListControl : UserControl
    {
        public CommandListControl()
        {
            InitializeComponent();
            this.DataContext = new CommandListViewModel();
        }
    }
}