using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.UI.Converters;
using me.cqp.luohuaming.UnraidMonitor.UI.ViewModels;
using Microsoft.Win32;
using PropertyChanged;
using SkiaSharp;
using System;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Controls
{
    /// <summary>
    /// FontPicker.xaml 的交互逻辑
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class FontPicker : UserControl
    {
        public FontPicker()
        {
            InitializeComponent();
        }

        public bool Loading { get; set; } = false;

        private string FontPreviewText { get; set; } = "AaBb114514 喵喵喵～(￣▽￣～)";

        public string CustomFont
        {
            get => (string)GetValue(CustomFontProperty);
            set => SetValue(CustomFontProperty, value);
        }

        public static readonly DependencyProperty CustomFontProperty =
            DependencyProperty.Register("CustomFont", typeof(string), typeof(FontPicker), new PropertyMetadata("微软雅黑"));

        private void BrowserFont_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Multiselect = false,
                Filter = "字体文件|*.ttf;*.otf;*.woff;*.woff2|所有文件|*.*",
                CheckFileExists = true,
            };
            if (dialog.ShowDialog() ?? false)
            {
                string fontPath = dialog.FileName;
                if (File.Exists(fontPath))
                {
                    CustomFont = CommonHelper.GetRelativePath(fontPath, MainSave.AppDirectory);
                }
                else
                {
                    MainWindow.ShowError("所选文件不存在或不是有效的字体文件");
                }
            }
        }

        private void Popup_Closed(object sender, EventArgs e)
        {

        }

        private async void FontPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (Loading || FontPreviewPopup.IsOpen)
            {
                return;
            }
            Loading = true;
            if (Window.GetWindow(this)?.DataContext is not WorkbenchViewModel vm)
            {
                MainWindow.ShowError("无法获取主 ViewModel");
                Loading = false;
                return;
            }
            FontPreviewPopup.IsOpen = true;
            string font = string.IsNullOrEmpty(CustomFont)
                ? DrawingStyle.GetThemeDefaultFont(vm.CurrentStyle.ItemTheme)
                : Path.Combine(MainSave.AppDirectory, CustomFont);
            using Painting painting = new(500, 100);
            await Task.Run(() =>
            {
                painting.Clear(SKColors.Transparent);
                var p = painting.DrawText(FontPreviewText, Painting.Anywhere, new(), SKColors.Black, customFont: Painting.CreateCustomFont(font));
                painting.Resize((int)(p.X + 10), (int)(p.Y + 5));
            });
            var bitmapSource = (BitmapSource)SKImageToImageSource.Convert(painting);
            Loading = false;
            FontPreview.Source = bitmapSource;
        }
    }
}
