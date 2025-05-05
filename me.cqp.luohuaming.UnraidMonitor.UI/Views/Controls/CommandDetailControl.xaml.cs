using System.Windows.Controls;
using me.cqp.luohuaming.UnraidMonitor.UI.ViewModels;

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