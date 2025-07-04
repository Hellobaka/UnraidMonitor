using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.UI.Models;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace me.cqp.luohuaming.UnraidMonitor.UI.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class WorkbenchViewModel
    {
        public WorkbenchViewModel()
        {
            NewCommand = new RelayCommand(_ => NewStyle(), _ => true);
            SaveCommand = new RelayCommand(_ => SaveStyle(), _ => true);
            ExitCommand = new RelayCommand(_ => Exit(), _ => true);
            OpenCommand = new RelayCommand(_ => OpenStyle(), _ => true);
            UndoCommand = new RelayCommand(_ => Undo(), _ => true);
            RedoCommand = new RelayCommand(_ => Redo(), _ => true);
            UpdateActionStackCommand = new RelayCommand(_ => UpdateActionStack(), _ => true);
            ToggleThemeCommand = new RelayCommand(_ => ToggleTheme(), _ => true);

            if (MainSave.CQApi != null && MainSave.CQApi.AppInfo != null)
            {
                VersionInfo = $"插件版本 {MainSave.CQApi.AppInfo.Version}";
            }

            ThemeIcon = (Geometry)Application.Current.FindResource("LightModeGeometry");
            Instance = this;
        }

        public event MainSave.PropertyChangeEventArg OnPropertyChangedDetail;

        /// <summary>
        /// 记录样式各个字段的值，包括嵌套对象中属性的值
        /// </summary>
        public Dictionary<string, object> StylePropertyValues { get; set; } = [];

        public bool AutoRedraw { get; set; } = true;

        public bool Debouncing { get; set; }

        public double DebounceValue { get; set; }

        public string? CurrentStylePath { get; set; }

        public DrawingStyle? CurrentStyle { get; set; }

        public Array ThemeValues => Enum.GetValues(typeof(DrawingStyle.Theme));

        public string VersionInfo { get; set; } = "插件版本 v1.0.0";

        public Array DrawBackgroundImageScaleTypeValues => Enum.GetValues(typeof(DrawingStyle.BackgroundImageScaleType));

        public Array DrawBackgroundTypeValues => Enum.GetValues(typeof(DrawingStyle.BackgroundType));

        public Array LayoutTypeValues => Enum.GetValues(typeof(DrawingCanvas.Layout));

        public Array ItemTypeValues => Enum.GetValues(typeof(DrawingItemBase.ItemType));

        public Array PositionValues => Enum.GetValues(typeof(DrawingCanvas.Position));

        public Array AlertTypeValues => Enum.GetValues(typeof(DrawingCanvas.AlertType));

        private Stack<(PropertyInfo property, object parent, object oldValue, object newValue)> ActionHistories { get; set; } = [];

        private int ActionCurrentIndex { get; set; }

        public ICommand NewCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        public ICommand OpenCommand { get; set; }

        public ICommand ExitCommand { get; set; }

        public ICommand UndoCommand { get; set; }

        public ICommand RedoCommand { get; set; }

        public ICommand ToggleThemeCommand { get; set; }

        public ICommand UpdateActionStackCommand { get; set; }

        public bool CanUndo { get; set; }

        public bool CanRedo { get; set; }

        public bool IsDarkMode { get; set; }

        public Geometry ThemeIcon { get; set; }
      
        public static WorkbenchViewModel Instance { get; private set; }

        public void ApplyMonitor()
        {
            if (CurrentStyle == null)
            {
                return;
            }
            var styleType = CurrentStyle.GetType();
            foreach (var property in styleType.GetProperties())
            {
                if (property.CanRead && property.CanWrite)
                {
                    var value = property.GetValue(CurrentStyle);
                    var valueType = value?.GetType();
                    if (value != null && !valueType.IsArray && (valueType.IsClass || valueType.IsLayoutSequential) && !(value is string || value is float || value is bool || value is int))
                    {
                        foreach (var nestedProperty in value.GetType().GetProperties())
                        {
                            if (nestedProperty.CanRead && nestedProperty.CanWrite)
                            {
                                //var nestedValue = nestedProperty.GetValue(value);
                                //StylePropertyValues[$"{property.Name}.{nestedProperty.Name}"] = nestedValue;
                            }
                        }
                    }
                    else
                    {
                        StylePropertyValues[property.Name] = value;
                    }
                }
            }
            CurrentStyle.OnPropertyChangedDetail += CurrentStyle_OnPropertyChangedDetail;
        }

        public void NewStyle()
        {

        }

        public void SaveStyle()
        {

        }

        public void OpenStyle()
        {

        }

        public void Exit()
        {

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

        public void UpdateActionStack()
        {

        }

        public void Undo()
        {
            if (ActionHistories.Count == 0)
            {
                return;
            }
            var lastAction = ActionHistories.Pop();
            var property = lastAction.property;
            var parent = lastAction.parent;
            var oldValue = lastAction.oldValue;
            property.SetValue(parent, oldValue);
            CurrentStyle_OnPropertyChangedDetail(property, parent.GetType().GetProperty(property.Name), oldValue, lastAction.newValue);
        }

        public void Redo()
        {
            if (ActionHistories.Count == 0)
            {
                return;
            }
            var lastAction = ActionHistories.Pop();
            var property = lastAction.property;
            var parent = lastAction.parent;
            var newValue = lastAction.newValue;
            property.SetValue(parent, newValue);
            CurrentStyle_OnPropertyChangedDetail(property, parent.GetType().GetProperty(property.Name), newValue, lastAction.oldValue);
        }

        private void CurrentStyle_OnPropertyChangedDetail(PropertyInfo propertyInfo, PropertyInfo parentType, object newValue, object oldValue)
        {
            string propertyName = parentType == null ? propertyInfo.Name : $"{parentType.Name}.{propertyInfo.Name}";
            if (StylePropertyValues.TryGetValue(propertyName, out object value))
            {
                // Get OldValue
                Debug.WriteLine($"{propertyName} From {value} Changed To {newValue}");
                StylePropertyValues[propertyName] = newValue;
            }
            else
            {
                value = null;
                // Debugger.Break();
                Debug.WriteLine($"{propertyName} Changed To {newValue}, but not found in StylePropertyValues");
            }
            OnPropertyChangedDetail?.Invoke(propertyInfo, parentType, newValue, value);
        }
    }
}
