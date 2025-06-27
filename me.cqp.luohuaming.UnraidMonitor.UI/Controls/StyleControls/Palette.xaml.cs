using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Palette.xaml 的交互逻辑
    /// </summary>
    public partial class Palette : UserControl, INotifyPropertyChanged
    {
        public Palette()
        {
            InitializeComponent();
        }

        public PublicInfos.Drawing.DrawingStyle.Colors Colors
        {
            get { return (PublicInfos.Drawing.DrawingStyle.Colors)GetValue(ColorsProperty); }
            set { SetValue(ColorsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Colors.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorsProperty =
            DependencyProperty.Register("Colors", typeof(PublicInfos.Drawing.DrawingStyle.Colors), typeof(Palette), new PropertyMetadata(null, OnColorsChanged));
       
        public event PropertyChangedEventHandler PropertyChanged;
      
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private static void OnColorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Palette palette && e.NewValue is PublicInfos.Drawing.DrawingStyle.Colors colors)
            {
                palette.OnPropertyChanged(nameof(Colors));
            }
        }
    }
}
