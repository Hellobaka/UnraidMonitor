using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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

}
