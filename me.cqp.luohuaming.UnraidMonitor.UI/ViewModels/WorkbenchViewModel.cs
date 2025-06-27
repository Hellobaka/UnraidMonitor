using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using static me.cqp.luohuaming.UnraidMonitor.PublicInfos.MainSave;

namespace me.cqp.luohuaming.UnraidMonitor.UI.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class WorkbenchViewModel
    {
        public event PropertyChangeEventArg OnPropertyChangedDetail;

        /// <summary>
        /// 记录样式各个字段的值，包括嵌套对象中属性的值
        /// </summary>
        public Dictionary<string, object> StylePropertyValues { get; set; } = [];

        public bool Debouncing { get; set; }

        public double DebounceValue { get; set; }

        public string? CurrentStylePath { get; set; }

        public DrawingStyle? CurrentStyle { get; set; }

        public Array ThemeValues => Enum.GetValues(typeof(DrawingStyle.Theme));

        public Array DrawBackgroundImageScaleTypeValues => Enum.GetValues(typeof(DrawingStyle.BackgroundImageScaleType));

        public Array DrawBackgroundTypeValues => Enum.GetValues(typeof(DrawingStyle.BackgroundType));

        private Stack<(PropertyInfo property, object parent, object oldValue, object newValue)> ActionHistories { get; set; } = [];

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
                                var nestedValue = nestedProperty.GetValue(value);
                                StylePropertyValues[$"{property.Name}.{nestedProperty.Name}"] = nestedValue;
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
                Debugger.Break();
                Debug.WriteLine($"{propertyName} Changed To {newValue}, but not found in StylePropertyValues");
            }
            OnPropertyChangedDetail?.Invoke(propertyInfo, parentType, newValue, value);
        }
    }
}
