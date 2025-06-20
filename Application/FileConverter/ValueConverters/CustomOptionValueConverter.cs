using System;
using System.Globalization;
using System.Windows.Data;
using FileConverter.CustomConverters;
using FileConverter;

namespace FileConverter.ValueConverters
{
    public class CustomOptionValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IConversionSettings settings) || !(parameter is CustomConverterOptionDefinition opt))
            {
                return null;
            }

            if (!settings.TryGetValue(opt.Name, out string str))
            {
                str = opt.DefaultValue;
            }

            switch (opt.ControlType?.ToLowerInvariant())
            {
                case "checkbox":
                    return string.Equals(str, opt.CheckedValue, StringComparison.OrdinalIgnoreCase);
                case "slider":
                case "textbox":
                case "dropdown":
                case "radio":
                default:
                    {
                        if (string.IsNullOrEmpty(opt.ValueType))
                        {
                            return str;
                        }

                        Type type = Type.GetType(opt.ValueType) ?? typeof(string);
                        return System.Convert.ChangeType(str, type, culture);
                    }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is CustomConverterOptionDefinition opt))
            {
                return null;
            }

            string result;
            if (opt.ControlType?.ToLowerInvariant() == "checkbox")
            {
                bool b = value is bool bl && bl;
                result = b ? opt.CheckedValue : opt.UncheckedValue;
            }
            else
            {
                result = System.Convert.ToString(value, culture);
            }

            return new ConversionSettingsOverride(opt.Name, result);
        }
    }
}
