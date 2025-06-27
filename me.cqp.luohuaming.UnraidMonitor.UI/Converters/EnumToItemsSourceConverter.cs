using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

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
}
