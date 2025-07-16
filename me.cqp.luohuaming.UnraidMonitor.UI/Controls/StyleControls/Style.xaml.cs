using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.UI.ViewModels;
using me.cqp.luohuaming.UnraidMonitor.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Controls.StyleControls
{
    /// <summary>
    /// Style.xaml 的交互逻辑
    /// </summary>
    public partial class Style : UserControl
    {
        public Style()
        {
            InitializeComponent();
        }

        private DrawingStyle CurrentStyle => (DataContext as WorkbenchViewModel).CurrentStyle;

        private void OpenContextMenu_Click(object sender, RoutedEventArgs e)
        {
            StyleContextMenu.PlacementTarget = sender as Button;
            StyleContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            StyleContextMenu.IsOpen = true;
        }

        private void CreateNewCanvas_Click(object sender, RoutedEventArgs e)
        {
            DrawingCanvas canvas = new();
            canvas.Name = $"新画布";
            CurrentStyle.Content.Add(canvas);

        }

        private void CollapseAllCanvas_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in CanvasContainer.Items)
            {
                if (CanvasContainer.ItemContainerGenerator.ContainerFromItem(item) is ContentPresenter contentPresenter)
                {
                    var canvas = MainWindow.FindVisualChild<Canvas>(contentPresenter);
                    if (canvas != null)
                    {
                        canvas.CanvasExpander.IsExpanded = false;
                    }
                }
            }
        }

        private void ExpandAllCanvas_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in CanvasContainer.Items)
            {
                if (CanvasContainer.ItemContainerGenerator.ContainerFromItem(item) is ContentPresenter contentPresenter)
                {
                    var canvas = MainWindow.FindVisualChild<Canvas>(contentPresenter);
                    if (canvas != null)
                    {
                        canvas.CanvasExpander.IsExpanded = true;
                    }
                }
            }
        }
    }
}
