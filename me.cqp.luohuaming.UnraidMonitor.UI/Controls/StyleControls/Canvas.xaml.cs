using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.UI.ViewModels;
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
    /// Canvas.xaml 的交互逻辑
    /// </summary>
    public partial class Canvas : UserControl
    {
        public Canvas()
        {
            InitializeComponent();
        }

        private DrawingStyle CurrentStyle => WorkbenchViewModel.Instance.CurrentStyle;

        private DrawingCanvas CurrentCanvas => DataContext as DrawingCanvas;

        private void OpenContextMenu_Click(object sender, RoutedEventArgs e)
        {
            CanvasContextMenu.PlacementTarget = sender as Button;
            CanvasContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            CanvasContextMenu.IsOpen = true;
        }

        private void CreateItem_Text_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CreateItem_Image_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CreateItem_Chart_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CreateItem_ProgressBar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CreateItem_ProgressRing_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CreateItem_RunningStatus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CreateItem_Alert_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CollapseAllItems_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExpandAllItems_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DuplicateCanvasWithItems_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DuplicateCanvas_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteCanvas_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
