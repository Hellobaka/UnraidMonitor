using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.UI.Controls.StyleControls;
using me.cqp.luohuaming.UnraidMonitor.UI.Models;
using me.cqp.luohuaming.UnraidMonitor.UI.Windows;
using Microsoft.Win32;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace me.cqp.luohuaming.UnraidMonitor.UI.ViewModels
{
    public class WorkbenchViewModel : INotifyPropertyChanged
    {
        public WorkbenchViewModel(Window workbench)
        {
            Workbench = workbench;

            NewCommand = new RelayCommand(_ => NewStyle(), _ => true);
            SaveCommand = new RelayCommand(_ => SaveStyle(), _ => true);
            ExitCommand = new RelayCommand(_ => Exit(), _ => true);
            OpenCommand = new RelayCommand(_ => OpenStyle(), _ => true);
            UndoCommand = new RelayCommand(_ => Undo(), _ => true);
            RedoCommand = new RelayCommand(_ => Redo(), _ => true);
            ToggleThemeCommand = new RelayCommand(_ => ToggleTheme(), _ => true);

            if (MainSave.CQApi != null && MainSave.CQApi.AppInfo != null)
            {
                VersionInfo = $"插件版本 {MainSave.CQApi.AppInfo.Version}";
            }

            ThemeIcon = (Geometry)Application.Current.FindResource("LightModeGeometry");
            UndoRedoManager = new UndoRedoManager(50);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event MainSave.PropertyChangeEventArg OnPropertyChangedDetail;
        public event MainSave.CollectionChangeEventArg OnCollectionChangedDetail;

        public bool AutoRedraw { get; set; } = true;

        public bool Debouncing { get; set; }
        
        public bool OperationPending { get; set; }

        public double DebounceValue { get; set; }

        public string? CurrentStylePath { get; set; }

        public DrawingStyle? CurrentStyle { get; set; }

        public string VersionInfo { get; set; } = "插件版本 v1.0.0";

        public Array ThemeValues { get; set; } = Enum.GetValues(typeof(DrawingStyle.Theme));

        public Array DrawBackgroundImageScaleTypeValues { get; set; } = Enum.GetValues(typeof(DrawingStyle.BackgroundImageScaleType));

        public Array DrawBackgroundTypeValues { get; set; } = Enum.GetValues(typeof(DrawingStyle.BackgroundType));

        public Array LayoutTypeValues { get; set; } = Enum.GetValues(typeof(DrawingCanvas.Layout));

        public Array ItemTypeValues { get; set; } = Enum.GetValues(typeof(DrawingItemBase.ItemType));

        public Array PositionValues { get; set; } = Enum.GetValues(typeof(DrawingCanvas.Position));

        public Array AlertTypeValues { get; set; } = Enum.GetValues(typeof(DrawingCanvas.AlertType));

        public Array BindingBoolConditionValues { get; set; } = Enum.GetValues(typeof(BoolCondition));

        public Array BindingNumberConverterValues { get; set; } = Enum.GetValues(typeof(NumberConverter));

        public Array BindingItemValues { get; set; } = Enum.GetValues(typeof(ItemType));

        public Array BindingTimeRangeValues { get; set; } = Enum.GetValues(typeof(TimeRange));
       
        public Window Workbench { get; set; }
      
        public ICommand NewCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        public ICommand OpenCommand { get; set; }

        public ICommand ExitCommand { get; set; }

        public ICommand UndoCommand { get; set; }

        public ICommand RedoCommand { get; set; }

        public ICommand ToggleThemeCommand { get; set; }

        public bool CanUndo => !UndoRedoManager.Processing && UndoRedoManager.UndoStack.Count > 0;

        public bool CanRedo => !UndoRedoManager.Processing && UndoRedoManager.RedoStack.Count > 0;

        public bool IsDarkMode { get; set; }

        public Geometry ThemeIcon { get; set; }

        public UndoRedoManager UndoRedoManager { get; set; }

        public void UnsubscribePropertyChangedEvents()
        {
            MainSave.OnPropertyChangedDetail -= MainSave_OnPropertyChangedDetail;
            MainSave.OnCollectionChangedDetail -= MainSave_OnCollectionChangedDetail;
        }

        public void SubscribePropertyChangedEvents()
        {
            MainSave.OnPropertyChangedDetail += MainSave_OnPropertyChangedDetail;
            MainSave.OnCollectionChangedDetail += MainSave_OnCollectionChangedDetail;
        }

        private void MainSave_OnCollectionChangedDetail(NotifyCollectionChangedEventArgs e, object instance)
        {
            UndoRedoManager.AddCommand(new CollectionChangeCommand(instance, e));
            OnCollectionChangedDetail?.Invoke(e, instance);
            OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(CanUndo));
            OperationPending = true;
        }

        private void MainSave_OnPropertyChangedDetail(PropertyInfo propertyInfo, object instance, object newValue, object oldValue)
        {
            if (propertyInfo.Name == "Boundary")
            {
                return;
            }
            UndoRedoManager.AddCommand(new PropertyChangeCommand(instance, propertyInfo, oldValue, newValue));
            OnPropertyChangedDetail?.Invoke(propertyInfo, instance, newValue, oldValue);
            OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(CanUndo));
            OperationPending = true;
        }

        public async Task NewStyle()
        {
            if (OperationPending && !await MainWindow.ShowConfirmAsync("还有操作未保存，确定要抛弃这些更改吗？"))
            {
                return;
            }

            CreateStyle dialog = new CreateStyle();
            dialog.Owner = Workbench;
            dialog.ShowDialog();
            if (dialog.DialogResult ?? false)
            {
                CurrentStylePath = dialog.SavedStylePath;
                try
                {
                    MainWindow.SaveActiveHistory(new StyleHistoryItem
                    {
                        DateTime = DateTime.Now,
                        FileName = Path.GetFileName(CurrentStylePath),
                        FullPath = CurrentStylePath
                    });

                    OperationPending = false;
                    UndoRedoManager.Clear();

                    CurrentStyle = DrawingStyle.LoadFromFile(CurrentStylePath);
                    OnPropertyChanged(nameof(CurrentStyle));
                    Workbench.Title = $"{CurrentStyle.Name} - 样式编辑器";
                    OnPropertyChangedDetail?.Invoke(null, this, CurrentStyle, null);
                }
                catch (Exception e)
                {
                    MainWindow.ShowError($"加载样式文件失败：{e.Message}");
                }
            }
        }

        public void SaveStyle()
        {
            if (CurrentStyle == null)
            {
                MainWindow.ShowError("当前样式未定义，无法保存。");
                return;
            }
            if (string.IsNullOrEmpty(CurrentStylePath))
            {
                SaveFileDialog dialog = new();
                dialog.Title = "保存样式文件";
                dialog.Filter = "样式文件|*.style|所有文件|*.*";
                if (dialog.ShowDialog() ?? false)
                {
                    CurrentStylePath = dialog.FileName;
                }
                else
                {
                    return;
                }
            }
            File.WriteAllText(CurrentStylePath, CurrentStyle.Serialize());
            MainWindow.ShowInfo("保存成功");
            OperationPending = false;
        }

        public async Task OpenStyle()
        {
            if (OperationPending && !await MainWindow.ShowConfirmAsync("还有操作未保存，确定要抛弃这些更改吗？"))
            {
                return;
            }
            OpenFileDialog dialog = new();
            dialog.Title = "打开样式文件";
            dialog.Filter = "样式文件|*.style|所有文件|*.*";
            if (dialog.ShowDialog() ?? false)
            {
                string path = dialog.FileName;
                MainWindow.SaveActiveHistory(new StyleHistoryItem
                {
                    DateTime = DateTime.Now,
                    FileName = Path.GetFileName(path),
                    FullPath = path
                });
                CurrentStylePath = path;
                try
                {
                    OperationPending = false;
                    UndoRedoManager.Clear();

                    CurrentStyle = DrawingStyle.LoadFromFile(path);
                    OnPropertyChanged(nameof(CurrentStyle));
                    Workbench.Title = $"{CurrentStyle.Name} - 样式编辑器";
                    OnPropertyChangedDetail?.Invoke(null, this, CurrentStyle, null);
                }
                catch (Exception e)
                {
                    MainWindow.ShowError($"加载样式文件失败：{e.Message}");
                }
            }
        }

        public async Task Exit()
        {
            if (OperationPending && !await MainWindow.ShowConfirmAsync("还有操作未保存，确定要抛弃这些更改吗？"))
            {
                return;
            }
            Workbench.Close();
        }

        public void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
            if (IsDarkMode)
            {
                ThemeIcon = (Geometry)Application.Current.FindResource("LightModeGeometry");
            }
            else
            {
                ThemeIcon = (Geometry)Application.Current.FindResource("DarkGeometry");
            }
        }

        public void Undo()
        {
            try
            {
                UndoRedoManager.Undo();
                OnPropertyChanged(nameof(CanRedo));
                OnPropertyChanged(nameof(CanUndo));
            }
            catch (Exception e)
            {
                MainWindow.ShowError($"撤销操作过程异常：{e}");
            }
        }

        public void Redo()
        {
            try
            {
                UndoRedoManager.Redo();
                OnPropertyChanged(nameof(CanRedo));
                OnPropertyChanged(nameof(CanUndo));
            }
            catch (Exception e)
            {
                MainWindow.ShowError($"重做操作过程异常：{e}");
            }
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void NoticeCanUndo()
        {
            OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(CanUndo));
        }
    }
}
