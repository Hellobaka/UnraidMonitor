using SkiaSharp;
using System.Windows;
using System.Windows.Controls;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Controls.StyleControls
{
    /// <summary>
    /// Margin.xaml 的交互逻辑
    /// </summary>
    public partial class Size : UserControl
    {
        public Size()
        {
            InitializeComponent();
        }

        public SKSize SKSize
        {
            get { return (SKSize)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("SKSize", typeof(SKSize), typeof(Size), new PropertyMetadata(new SKSize()));
    }
}
