using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.UI.Controls;
using me.cqp.luohuaming.UnraidMonitor.UI.Converters;
using me.cqp.luohuaming.UnraidMonitor.UI.ViewModels;
using System;
using System.Diagnostics;
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
            UpdateKeyBinding();
        }

        public Workbench(string? path)
        {
            InitializeComponent();
            ViewModel = new WorkbenchViewModel();
            DataContext = ViewModel;
            ViewModel.CurrentStylePath = path;
            UpdateKeyBinding();
        }

        private Point ScrollStartPoint { get; set; }

        private Point ScrollStartOffset { get; set; }

        private bool IsDragging { get; set; } = false;

        private int DebounceRedrawTime { get; set; } = 1500;

        private CancellationTokenSource DebounceCancel { get; set; }

        private WorkbenchViewModel ViewModel { get; set; }

        private async Task<DrawingStyle?> LoadStyleFromFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return null;
            }
            try
            {
                DrawingStyle style = await Task.Run(() => DrawingStyle.LoadFromFile(path, false));
                Title = $"{style.Name} - 样式编辑器";
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
            (NonClientAreaContent as NonClientAreaContent).DataContext = ViewModel;

            if (!string.IsNullOrEmpty(ViewModel.CurrentStylePath))
            {
                var style = await LoadStyleFromFile(ViewModel.CurrentStylePath);
                if (style == null)
                {
                    MainWindow.ShowError($"无法加载样式文件: {ViewModel.CurrentStylePath}");
                }
                else
                {
                    ViewModel.CurrentStyle?.UnsubscribePropertyChangedEvents();

                    ViewModel.CurrentStyle = style;
                    ViewModel.CurrentStyle.SubscribePropertyChangedEvents();

                    StyleEditor.DataContext = ViewModel;
                    MainWindow.ShowInfo($"{ViewModel.CurrentStyle.Name} 样式加载成功");
                    await CallStyleRedraw();
                }
                ViewModel.ApplyMonitor();
                ViewModel.OnPropertyChangedDetail += ViewModel_OnPropertyChangedDetail;
            }
        }

        private void UpdateKeyBinding()
        {
            InputBindings.Add(new KeyBinding(ViewModel.SaveCommand, Key.S, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(ViewModel.NewCommand, Key.N, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(ViewModel.OpenCommand, Key.O, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(ViewModel.UndoCommand, Key.Z, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(ViewModel.RedoCommand, Key.Y, ModifierKeys.Control));

            CommandBindings.Add(new CommandBinding(ViewModel.SaveCommand, (_, _) => ViewModel.SaveCommand.Execute(null)));
            CommandBindings.Add(new CommandBinding(ViewModel.NewCommand, (_, _) => ViewModel.NewCommand.Execute(null)));
            CommandBindings.Add(new CommandBinding(ViewModel.OpenCommand, (_, _) => ViewModel.OpenCommand.Execute(null)));
            CommandBindings.Add(new CommandBinding(ViewModel.UndoCommand, (_, _) => ViewModel.UndoCommand.Execute(null)));
            CommandBindings.Add(new CommandBinding(ViewModel.RedoCommand, (_, _) => ViewModel.RedoCommand.Execute(null)));
        }

        private void ViewModel_OnPropertyChangedDetail(System.Reflection.PropertyInfo propertyInfo, System.Reflection.PropertyInfo parentPropertyType, object newValue, object oldValue)
        {
            if (ViewModel.AutoRedraw)
            {
                DebounceStyleRedraw_Click(null, null);
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
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                ViewModel.Debouncing = true;
                using var img = await Task.Run(async () =>
                {
                    try
                    {
                        return ViewModel.CurrentStyle?.Draw(ViewModel.CurrentStyle.Width);
                    }
                    catch (Exception e)
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            MainWindow.ShowError($"绘制时发生异常：{e}");
                        });
                        return null;
                    }
                });
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
            catch (Exception e)
            {
                MainWindow.ShowError($"绘制时发生异常：{e}");
            }
            finally
            {
                ViewModel.Debouncing = false;
                DrawTimeText.Text = $"{stopwatch.ElapsedMilliseconds} ms";
            }
        }
    }
}
