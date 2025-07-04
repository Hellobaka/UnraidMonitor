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
    /// DrawingItem.xaml 的交互逻辑
    /// </summary>
    public partial class DrawingItem : UserControl
    {
        public DrawingItem()
        {
            InitializeComponent();
        }

        private void OpenContextMenu_Click(object sender, RoutedEventArgs e)
        {
            ItemContextMenu.PlacementTarget = sender as Button;
            ItemContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            ItemContextMenu.IsOpen = true;
        }

        private void DuplicateItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearBinding_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
