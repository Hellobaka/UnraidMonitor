using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.UI.Controls;
using me.cqp.luohuaming.UnraidMonitor.UI.Converters;
using me.cqp.luohuaming.UnraidMonitor.UI.ViewModels;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Windows
{
    /// <summary>
    /// Workbench.xaml 的交互逻辑
    /// </summary>
    public partial class Workbench : HandyControl.Controls.Window
    {
        public Workbench()
        {
            InitializeComponent();
            ViewModel = new WorkbenchViewModel();
            DataContext = ViewModel;
        }
     
        private Point ScrollStartPoint { get; set; }
     
        private Point ScrollStartOffset { get; set; }
     
        private bool IsDragging { get; set; } = false;

        private int DebounceRedrawTime { get; set; } = 3000;

        private CancellationTokenSource DebounceCancel { get; set; }

        private WorkbenchViewModel ViewModel { get; set; }

        public Workbench(string? path)
        {
            InitializeComponent();
            ViewModel = new WorkbenchViewModel();
            DataContext = ViewModel;
            ViewModel.CurrentStylePath = path;
        }

        private async Task<DrawingStyle?> LoadStyleFromFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return null;
            }
            try
            {
                DrawingStyle style = await Task.Run(() => DrawingStyle.LoadFromFile(path));
                return style;
            }
            catch (Exception ex)
            {
                MainWindow.ShowError($"加载样式文件时发生错误: {ex.Message}");
                return null;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NonClientAreaContent = new NonClientAreaContent();

            if (!string.IsNullOrEmpty(ViewModel.CurrentStylePath))
            {
                ViewModel.CurrentStyle = await LoadStyleFromFile(ViewModel.CurrentStylePath);
                if (ViewModel.CurrentStyle == null)
                {
                    MainWindow.ShowError($"无法加载样式文件: {ViewModel.CurrentStylePath}");
                }
                else
                {
                    MainWindow.ShowInfo($"{ViewModel.CurrentStyle.Name} 样式加载成功");
                }
            }
        }
        private void MainImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MainImage.Source == null)
            {
                return;
            }

            IsDragging = true;
            ScrollStartPoint = e.GetPosition(ImageScrollViewer);
            ScrollStartOffset = new Point(ImageScrollViewer.HorizontalOffset, ImageScrollViewer.VerticalOffset);
            MainImage.CaptureMouse();
        }

        private void MainImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsDragging = false;
            MainImage.ReleaseMouseCapture();
        }

        private void MainImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsDragging && MainImage.Source != null)
            {
                var pos = e.GetPosition(ImageScrollViewer);
                var dX = pos.X - ScrollStartPoint.X;
                var dY = pos.Y - ScrollStartPoint.Y;
                ImageScrollViewer.ScrollToHorizontalOffset(ScrollStartOffset.X - dX);
                ImageScrollViewer.ScrollToVerticalOffset(ScrollStartOffset.Y - dY);
            }
        }

        private void MainImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (MainImage.Source == null)
                return;

            double zoom = e.Delta > 0 ? 1.1 : 0.9;
            double oldScale = ImageScale.ScaleX;
            double newScale = oldScale * zoom;

            // 限制缩放范围
            if (newScale < 0.1) newScale = 0.1;
            if (newScale > 10) newScale = 10;

            // 获取鼠标在Image上的位置
            var position = e.GetPosition(MainImage);

            // 计算缩放前后ScrollViewer的偏移
            var sv = ImageScrollViewer;
            double relativeX = position.X / MainImage.ActualWidth;
            double relativeY = position.Y / MainImage.ActualHeight;
            double offsetX = (sv.HorizontalOffset + position.X) * zoom - position.X;
            double offsetY = (sv.VerticalOffset + position.Y) * zoom - position.Y;

            ImageScale.ScaleX = newScale;
            ImageScale.ScaleY = newScale;

            sv.Dispatcher.BeginInvoke(new Action(() =>
            {
                sv.ScrollToHorizontalOffset(offsetX);
                sv.ScrollToVerticalOffset(offsetY);
            }));

            e.Handled = true;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ImageScale.ScaleX = 1;
            ImageScale.ScaleY = 1;
        }

        private async void DebounceStyleRedraw_Click(object sender, RoutedEventArgs e)
        {
            DebounceCancel?.Cancel();
            if (ViewModel.Debouncing)
            {
                return;
            }
            DebounceCancel = new CancellationTokenSource();
            var token = DebounceCancel.Token;

            ViewModel.DebounceValue = 0;

            try
            {
                int interval = 10; // 进度条刷新间隔(ms)
                int elapsed = 0;

                while (elapsed < DebounceRedrawTime)
                {
                    await Task.Delay(interval, token);
                    elapsed += interval;
                    ViewModel.DebounceValue = (double)elapsed / DebounceRedrawTime * 100;
                }

                await CallStyleRedraw();
                ViewModel.DebounceValue = 0;
            }
            catch (TaskCanceledException)
            {
                ViewModel.DebounceValue = 0;
            }
        }

        private async Task CallStyleRedraw()
        {
            try
            {
                ViewModel.Debouncing = true;
                using var img = await Task.Run(() => ViewModel.CurrentStyle?.Draw(ViewModel.CurrentStyle.Width));
                if (img != null)
                {
                    var bitmapSource = (BitmapSource)SKImageToImageSource.Convert(img);
                    if (MainImage.Source == null)
                    {
                        double imageWidth = bitmapSource.PixelWidth;
                        double imageHeight = bitmapSource.PixelHeight;

                        // 获取ScrollViewer可视区域尺寸
                        double viewportWidth = ImageScrollViewer.ViewportWidth;
                        double viewportHeight = ImageScrollViewer.ViewportHeight;

                        if (viewportWidth <= 0 || viewportHeight <= 0) return;

                        double scaleX = viewportWidth / imageWidth;
                        double scaleY = viewportHeight / imageHeight;
                        double scale = Math.Min(scaleX, scaleY);

                        // 自适应放大
                        ImageScale.ScaleX = scale;
                        ImageScale.ScaleY = scale;

                        double offsetX = (imageWidth - viewportWidth) / 2;
                        double offsetY = (imageHeight - viewportHeight) / 2;
                        
                        // 居中
                        ImageScrollViewer.ScrollToHorizontalOffset(offsetX);
                        ImageScrollViewer.ScrollToVerticalOffset(offsetY);
                    }
                    MainImage.Source = bitmapSource;
                }
            }
            catch { }
            finally
            {
                ViewModel.Debouncing = false;
            }
        }
    }
}
