using System.Windows;
using System.Windows.Controls;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Controls.StyleControls
{
    /// <summary>
    /// Margin.xaml 的交互逻辑
    /// </summary>
    public partial class Margin : UserControl
    {
        public Margin()
        {
            InitializeComponent();
        }

        public PublicInfos.Drawing.Thickness Thickness
        {
            get { return (PublicInfos.Drawing.Thickness)GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }

        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register("Thickness", typeof(PublicInfos.Drawing.Thickness), typeof(Margin), new PropertyMetadata(new PublicInfos.Drawing.Thickness(0)));
    }
}
