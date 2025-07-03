using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        private void PaletteTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            var darkMode = PaletteTemplateDarkMode.IsChecked ?? false;
            DrawingStyle.Colors color = null;
            switch (PaletteTemplateSeletor.SelectedIndex)
            {
                case 0:
                    color = DrawingStyle.GetThemeDefaultColor(DrawingStyle.Theme.WinUI3, darkMode);
                    break;

                case 1:
                    color = DrawingStyle.GetThemeDefaultColor(DrawingStyle.Theme.MaterialDesign2, darkMode);
                    break;

                case 2:
                    color = DrawingStyle.GetThemeDefaultColor(DrawingStyle.Theme.MaterialDesign3, darkMode);
                    break;

                case 3:
                    color = DrawingStyle.GetThemeDefaultColor(DrawingStyle.Theme.Unraid, darkMode);
                    break;

                default:
                    MainWindow.ShowError("颜色模板无效");
                    break;
            }
            if (color != null)
            {
                Colors.AccentColor = color.AccentColor;
                Colors.Accent2Color = color.Accent2Color;
                Colors.BackgroundColor = color.BackgroundColor;
                Colors.TextColor = color.TextColor;
                Colors.SuccessColor = color.SuccessColor;
                Colors.SuccessIconColor = color.SuccessIconColor;
                Colors.FatalColor = color.FatalColor;
                Colors.FatalIconColor = color.FatalIconColor;
                Colors.InfoColor = color.InfoColor;
                Colors.InfoIconColor = color.InfoIconColor;
                Colors.WarningColor = color.WarningColor;
                Colors.WarningIconColor = color.WarningIconColor;
            }
        }
    }
}
