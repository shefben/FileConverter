using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FileConverter;
using FileConverter.CustomConverters;

namespace FileConverter.ValueConverters
{
    public class OptionVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return Visibility.Visible;
            }

            var option = values[0] as CustomConverterOptionDefinition;
            var preset = values[1] as ConversionPreset;
            if (option == null || preset == null)
            {
                return Visibility.Visible;
            }

            return option.EvaluateCondition(preset) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
