using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.UI.Models;
using me.cqp.luohuaming.UnraidMonitor.UI.Windows;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Converters
{
    public class EnumToItemsSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Type enumType = value is Array array && array.Length > 0 ? array.GetValue(0)?.GetType() : null;
            return enumType != null && enumType.IsEnum ? Enum.GetValues(enumType) : (object)null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ObjectToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value != null) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var type = value.GetType();
            if (!type.IsEnum)
            {
                return value.ToString();
            }

            var name = Enum.GetName(type, value);
            if (name == null)
            {
                return value.ToString();
            }

            var field = type.GetField(name);
            if (field == null)
            {
                return value.ToString();
            }

            return Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr ? attr.Description : name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !targetType.IsEnum)
            {
                return null;
            }

            foreach (var field in targetType.GetFields())
            {
                var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if ((attr != null && attr.Description == value.ToString()) || field.Name == value.ToString())
                {
                    return Enum.Parse(targetType, field.Name);
                }
            }
            return null;
        }

    }

    public class EnumToStringConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? DependencyProperty.UnsetValue;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !targetType.IsEnum)
            {
                return null;
            }

            return Enum.Parse(targetType, value.ToString());
        }

    }

    public class GetDisplayKeyValuePairKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DisplayKeyValuePair pair)
            {
                return pair.Key;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DisplayKeyValuePair pair)
            {
                return pair.Value;
            }
            return DependencyProperty.UnsetValue;
        }
    }

    public class GetDisplayKeyValuePairValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DisplayKeyValuePair pair)
            {
                return pair.Value;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DisplayKeyValuePair pair)
            {
                return pair.Key;
            }
            return DependencyProperty.UnsetValue;
        }
    }

    public class SingleToArraySourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return new ObservableCollection<string>() { str };
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<string> arr)
            {
                return arr.FirstOrDefault() ?? DependencyProperty.UnsetValue;
            }
            return DependencyProperty.UnsetValue;
        }
    }

    public class StringToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrEmpty(path))
            {
                try
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    return image;
                }
                catch
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AlarmToDetailConverter : IValueConverter
    {
        private static Array ItemTypes => Enum.GetValues(typeof(ItemType));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AlarmRuleBase alarm)
            {
                var d = ItemTypes.Cast<ItemType>().FirstOrDefault(x => x.ToString() == alarm.ClassName);
                if (d != ItemType.Unknown)
                {
                    var description = new EnumDescriptionConverter().Convert(d, targetType, parameter, culture) as string;
                    
                    var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "me.cqp.luohuaming.UnraidMonitor.PublicInfos");
                    var modelTypes = assembly?.GetType($"me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.{alarm.ClassName}");
                    if (modelTypes == null)
                    {
                        return DependencyProperty.UnsetValue;
                    }

                    foreach (PropertyInfo propertyInfo in modelTypes.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        string displayName = Attribute.GetCustomAttribute(propertyInfo, typeof(DescriptionAttribute)) is DescriptionAttribute attr ? attr.Description : propertyInfo.Name;
                        return $"{description} {displayName}";
                    }
                }
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AlarmToTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AlarmRuleBase alarm)
            {
                if(alarm is RangeAlarmRule) 
                {
                    return "区间报警";
                }
                else if (alarm is ThresholdAlarmRule)
                {
                    return "持续时间阈值报警";
                }
                else if (alarm is RateOfChangeAlarmRule)
                {
                    return "变化率报警";
                }
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TimeSpanToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return DateTime.Today;
            }
            if (value is TimeSpan time)
            {
                return DateTime.Today.Add(time);
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new TimeSpan();
            }
            if (value is DateTime time)
            {
                return time.TimeOfDay;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
