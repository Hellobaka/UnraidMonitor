using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items;
using me.cqp.luohuaming.UnraidMonitor.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Controls.StyleControls
{
    /// <summary>
    /// Canvas.xaml 的交互逻辑
    /// </summary>
    public partial class Canvas : UserControl
    {
        public Canvas()
        {
            InitializeComponent();
        }

        private DrawingCanvas CurrentCanvas => DataContext as DrawingCanvas;

        private void OpenContextMenu_Click(object sender, RoutedEventArgs e)
        {
            CanvasContextMenu.PlacementTarget = sender as Button;
            CanvasContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            CanvasContextMenu.IsOpen = true;
        }

        private WorkbenchViewModel? GetWorkbenchViewModel()
        {
            if (Window.GetWindow(this)?.DataContext is not WorkbenchViewModel vm)
            {
                MainWindow.ShowError("无法获取主 ViewModel");
                return null;
            }
            return vm;
        }

        private void CreateItem_Text_Click(object sender, RoutedEventArgs e)
        {
            DrawingItem_Text textItem = new();
            textItem.Text = $"新文本";
            CurrentCanvas.Content = [.. CurrentCanvas.Content, textItem];
            DrawingItemExpander.IsExpanded = true;
        }

        private void CreateItem_Image_Click(object sender, RoutedEventArgs e)
        {
            DrawingItem_Image imageItem = new();
            CurrentCanvas.Content = [.. CurrentCanvas.Content, imageItem];
            DrawingItemExpander.IsExpanded = true;
        }

        private void CreateItem_Chart_Click(object sender, RoutedEventArgs e)
        {
            DrawingItem_Chart item = new();
            CurrentCanvas.Content = [.. CurrentCanvas.Content, item];
            DrawingItemExpander.IsExpanded = true;
        }

        private void CreateItem_ProgressBar_Click(object sender, RoutedEventArgs e)
        {
            DrawingItem_ProgressBar item = new();
            CurrentCanvas.Content = [.. CurrentCanvas.Content, item];
            DrawingItemExpander.IsExpanded = true;
        }

        private void CreateItem_ProgressRing_Click(object sender, RoutedEventArgs e)
        {
            DrawingItem_ProgressRing item = new();
            CurrentCanvas.Content = [.. CurrentCanvas.Content, item];
            DrawingItemExpander.IsExpanded = true;
        }

        private void CreateItem_RunningStatus_Click(object sender, RoutedEventArgs e)
        {
            DrawingItem_RunningStatus item = new();
            CurrentCanvas.Content = [.. CurrentCanvas.Content, item];
            DrawingItemExpander.IsExpanded = true;
        }

        private void CreateItem_Alert_Click(object sender, RoutedEventArgs e)
        {
            DrawingItem_Alert item = new();
            CurrentCanvas.Content = [.. CurrentCanvas.Content, item];
            DrawingItemExpander.IsExpanded = true;
        }

        private void CollapseAllItems_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in CurrentCanvas.Content)
            {
                item.IsExpanded = false;
            }
        }

        private void ExpandAllItems_Click(object sender, RoutedEventArgs e)
        {
            DrawingItemExpander.IsExpanded = true;
            foreach (var item in CurrentCanvas.Content)
            {
                item.IsExpanded = true;
            }
        }

        private void DuplicateCanvasWithItems_Click(object sender, RoutedEventArgs e)
        {
            var vm = GetWorkbenchViewModel();
            if (vm == null)
            {
                return;
            }
            var cloneCanvas = CurrentCanvas.Clone();
            cloneCanvas.Name = $"{CurrentCanvas.Name}_复制";
            vm.CurrentStyle.Content.Add(cloneCanvas);
        }

        private void DuplicateCanvas_Click(object sender, RoutedEventArgs e)
        {
            var vm = GetWorkbenchViewModel();
            if (vm == null)
            {
                return;
            }
            var cloneCanvas = CurrentCanvas.Clone();
            cloneCanvas.Content = [];
            cloneCanvas.Name = $"{CurrentCanvas.Name}_复制";
            vm.CurrentStyle.Content.Add(cloneCanvas);
        }

        private void DeleteCanvas_Click(object sender, RoutedEventArgs e)
        {
            var vm = GetWorkbenchViewModel();
            if (vm == null)
            {
                return;
            }
            if (vm.CurrentStyle.Content.Contains(CurrentCanvas))
            {
                vm.CurrentStyle.Content.Remove(CurrentCanvas);
            }
            else
            {
                MainWindow.ShowError("当前画布不在样式中，无法删除");
            }
        }
    }
}
