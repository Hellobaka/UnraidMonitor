using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using me.cqp.luohuaming.UnraidMonitor.UI.ViewModels;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Controls
{
    /// <summary>
    /// AddOrUpdateStyleCommand.xaml 的交互逻辑
    /// </summary>
    public partial class AddOrUpdateStyleCommand : Border
    {
        public AddOrUpdateStyleCommand()
        {
            InitializeComponent();
            DataContext = new AddOrUpdateStyleCommandViewModel();
        }

        private void BrowserPathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                InitialDirectory = MainSave.AppDirectory,
                Filter = "样式文件|*.style|所有文件|*.*",
                Multiselect = false,
                Title = "选择一个样式文件"
            };
            if (dialog.ShowDialog() ?? false)
            {
                if (DataContext is AddOrUpdateStyleCommandViewModel viewModel)
                {
                    viewModel.StylePath = dialog.FileName;
                }
            }
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddOrUpdateStyleCommandViewModel viewModel)
            {
                viewModel.StylePath = viewModel.Result.Item2.StylePath;
                viewModel.Command = viewModel.Result.Item2.Command;
            }
        }
    }
}
