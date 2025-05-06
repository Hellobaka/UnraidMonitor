using me.cqp.luohuaming.UnraidMonitor.UI.ViewModels;
using System.Windows.Controls;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Views.Controls
{
    public partial class CommandDetailControl : UserControl
    {
        public CommandDetailControl()
        {
            InitializeComponent();
            this.DataContext = new CommandDetailViewModel();
        }
    }
}