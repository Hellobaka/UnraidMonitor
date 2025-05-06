using me.cqp.luohuaming.UnraidMonitor.UI.ViewModels;
using System.Windows.Controls;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Views
{
    public partial class CommandConfigPage : UserControl
    {
        private CommandListViewModel _listVM;

        private CommandDetailViewModel _detailVM;

        public CommandConfigPage()
        {
            InitializeComponent();
            _listVM = new CommandListViewModel();
            _detailVM = new CommandDetailViewModel(_listVM.CommandList);
            // 选中项联动
            _listVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_listVM.SelectedCommand))
                {
                    _detailVM.CurrentConfig = _listVM.SelectedCommand ?? new me.cqp.luohuaming.UnraidMonitor.PublicInfos.UnraidCommandConfig();
                }
            };
            // 初始化时同步一次
            _detailVM.CurrentConfig = _listVM.SelectedCommand;
            // 绑定到子控件
            this.DataContext = new { ListVM = _listVM, DetailVM = _detailVM };
        }

        public CommandListViewModel ListVM => _listVM;

        public CommandDetailViewModel DetailVM => _detailVM;
    }
}