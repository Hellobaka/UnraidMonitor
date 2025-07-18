using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.UI.Windows;
using System.Windows;
using System.Windows.Controls;

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

        public DrawingCanvas CurrentCanvas
        {
            get { return (DrawingCanvas)GetValue(CurrentCanvasProperty); }
            set { SetValue(CurrentCanvasProperty, value); }
        }

        public static readonly DependencyProperty CurrentCanvasProperty =
            DependencyProperty.Register("CurrentCanvas", typeof(DrawingCanvas), typeof(DrawingItem), new PropertyMetadata(null));

        public DrawingItemBase CurrentItem => DataContext as DrawingItemBase;

        private void OpenContextMenu_Click(object sender, RoutedEventArgs e)
        {
            ItemContextMenu.PlacementTarget = sender as Button;
            ItemContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            ItemContextMenu.IsOpen = true;
        }

        private void DuplicateItem_Click(object sender, RoutedEventArgs e)
        {
            var newItem = CurrentItem.Clone();
            CurrentCanvas.Content.Add(newItem);
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentCanvas.Content.Contains(CurrentItem))
            {
                CurrentCanvas.Content.Remove(CurrentItem);
            }
            else
            {
                MainWindow.ShowError("当前项不在画布中，无法删除");
            }
        }

        private void ClearBinding_Click(object sender, RoutedEventArgs e)
        {
            CurrentItem.Binding = null;
        }

        private void SetBinding_Click(object sender, RoutedEventArgs e)
        {
            BindingEditor editor = new(CurrentItem)
            {
                Owner = Window.GetWindow(this),
                CustomBinding = CurrentItem.Binding?.Clone(),
            };
            if (editor.ShowDialog() ?? false)
            {
                CurrentItem.Binding = editor.CustomBinding;
            }
        }
    }
}
