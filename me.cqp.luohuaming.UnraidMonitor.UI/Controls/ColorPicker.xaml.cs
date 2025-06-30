using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Controls
{
    /// <summary>
    /// ColorPicker.xaml 的交互逻辑
    /// </summary>
    public partial class ColorPicker : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(string), typeof(ColorPicker), new PropertyMetadata("#FFFFFF", OnColorChanged));

        public static readonly DependencyProperty ShowDropperProperty =
            DependencyProperty.Register("ShowDropper", typeof(bool), typeof(ColorPicker), new PropertyMetadata(true, OnDropperVisibilityChanged));

        public ColorPicker()
        {
            InitializeComponent();
        }

        public string Color
        {
            get { return (string)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public bool ShowDropper
        {
            get { return (bool)GetValue(ShowDropperProperty); }
            set { SetValue(ShowDropperProperty, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event Action<Color, string> OnColorOutputChanged;

        public Brush ColorPreview { get; set; }

        public Visibility DropperVisibility { get; set; }

        public static bool TryParseColorFromHexString(string input, out Color color)
        {
            color = Colors.White;
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            try
            {
                if (input.StartsWith("#"))
                {
                    input = input.Substring(1);
                }
                if (input.Length == 6)
                {
                    input = "FF" + input; // 添加透明度
                }
                color = (Color)ColorConverter.ConvertFromString("#" + input);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ColorPicker colorPicker)
            {
                return;
            }
            if (!TryParseColorFromHexString((string)e.NewValue, out Color c))
            {
                c = Colors.Transparent;
            }
            colorPicker.ColorPreview = new SolidColorBrush(c);
            colorPicker.ColorText.Text = c.ToString().Replace("#FF", "#");
            colorPicker.OnPropertyChanged(nameof(ColorPreview));
        }

        private static void OnDropperVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPicker colorPicker)
            {
                colorPicker.DropperVisibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
                colorPicker.OnPropertyChanged(nameof(DropperVisibility));
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenColorPickerButton_Click(sender, e);
        }

        private void ColorPickerDialog_Canceled(object sender, EventArgs e)
        {
            ColorPickerPopup.IsOpen = false;
        }

        private void ColorPickerDialog_Confirmed(object sender, HandyControl.Data.FunctionEventArgs<Color> e)
        {
            ColorPickerPopup.IsOpen = false;
            Color = e.Info.ToString().Replace("#FF", "#");
            OnColorOutputChanged?.Invoke(e.Info, ColorText.Text);
        }

        private void OpenColorPickerButton_Click(object sender, RoutedEventArgs e)
        {
            if (ColorPickerPopup.IsOpen)
            {
                ColorPickerPopup.IsOpen = false;
                return;
            }
            if (!TryParseColorFromHexString(ColorText.Text, out Color c))
            {
                MainWindow.ShowError($"颜色无效：{ColorText.Text}，已替换为默认颜色");
                c = Colors.Transparent;
                ColorText.Text = c.ToString();
            }
            ColorPickerDialog.SelectedBrush = new SolidColorBrush(c);
            ColorPickerPopup.IsOpen = true;
        }
    }
}