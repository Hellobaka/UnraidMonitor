using me.cqp.luohuaming.UnraidMonitor.UI.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Controls
{
    /// <summary>
    /// PicturePicker.xaml 的交互逻辑
    /// </summary>
    public partial class PicturePicker : UserControl
    {
        public PicturePicker()
        {
            InitializeComponent();
        }

        private bool ControlLoaded { get; set; } = false;

        public bool MultiSelect
        {
            get => (bool)GetValue(MultiSelectProperty);
            set => SetValue(MultiSelectProperty, value);
        }

        public static readonly DependencyProperty MultiSelectProperty =
            DependencyProperty.Register("MultiSelect", typeof(bool), typeof(PicturePicker), new PropertyMetadata(false));

        public ObservableCollection<PictureItem> PictureItems { get; set; } = [];

        public ObservableCollection<string> PicturePaths
        {
            get => (ObservableCollection<string>)GetValue(PicturePathsProperty);
            set => SetValue(PicturePathsProperty, value);
        }

        public static readonly DependencyProperty PicturePathsProperty =
            DependencyProperty.Register("PicturePaths", typeof(ObservableCollection<string>), typeof(PicturePicker), new PropertyMetadata(new ObservableCollection<string>(), OnPicturePathsChanged));

        private static void OnPicturePathsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PicturePicker picker)
            {
                if (e.OldValue is ObservableCollection<string> oldCollection)
                {
                    oldCollection.CollectionChanged -= picker.ImagePaths_CollectionChanged;
                }

                if (e.NewValue is ObservableCollection<string> newCollection)
                {
                    newCollection.CollectionChanged += picker.ImagePaths_CollectionChanged;
                }
                picker.SyncImagePathsToPictureItems();
            }
        }

        private void ImagePaths_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SyncImagePathsToPictureItems();
        }

        private void SyncImagePathsToPictureItems()
        {
            PictureItems.Clear();

            if (PicturePaths != null)
            {
                foreach (var path in PicturePaths.Where(x => !string.IsNullOrEmpty(x)))
                {
                    if (File.Exists(path))
                    {
                        PictureItems.Add(new PictureItem
                        {
                            ImagePath = path,
                            IsAddButton = false
                        });
                    }
                    else
                    {
                        MainWindow.ShowError($"图片 {path} 路径文件不存在");
                    }
                }
            }
            if (PictureItems.Count == 0 || MultiSelect)
            {
                PictureItems.Add(new PictureItem
                {
                    IsAddButton = true
                });
            }
        }

        private void SyncPictureItemsToImagePaths()
        {
            if (PicturePaths == null)
            {
                return;
            }

            var newPaths = PictureItems
                .Where(x => !x.IsAddButton)
                .Select(x => x.ImagePath)
                .ToList();

            ObservableCollection<string> arr = [];
            foreach (var p in newPaths)
            {
                arr.Add(p);
            }
            PicturePaths = arr;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Grid).Tag is PictureItem item
                && !string.IsNullOrEmpty(item.ImagePath))
            {
                // 阻止事件冒泡
                return;
            }
            string path = ShowPictureSelectDialog();
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            if (MultiSelect)
            {
                PictureItems.Insert(PictureItems.Count - 1, new PictureItem { ImagePath = path });
            }
            else if (!MultiSelect)
            {
                PictureItems[0].ImagePath = path;
                PictureItems[0].IsAddButton = false;
            }
            SyncPictureItemsToImagePaths();
        }

        private void PictureImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string path = ShowPictureSelectDialog();
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            if ((sender as Image).DataContext is not PictureItem item)
            {
                return;
            }
            item.ImagePath = path;
            SyncPictureItemsToImagePaths();
        }

        private string? ShowPictureSelectDialog()
        {
            var dialog = new OpenFileDialog()
            {
                Multiselect = MultiSelect,
                Filter = "图片|*.jpg;*.jpeg;*.png;*.bmp;|所有文件|*.*",
                CheckFileExists = true,
            };
            return dialog.ShowDialog() ?? false ? dialog.FileName : null;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).DataContext is not PictureItem item)
            {
                return;
            }
            PictureItems.Remove(item);
            SyncPictureItemsToImagePaths();
        }
    }
}
