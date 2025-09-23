using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Helpers
{
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => Microsoft.Maui.Graphics.Color.FromArgb(value?.ToString() ?? "#FFFFFF");

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }

    public class NotNullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value != null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }


    public class SelectedColorBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var current = value?.ToString();
            var selected = parameter?.ToString();
            return current == selected ? Colors.Black : Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // Multi-binding: determina el color del borde según si el item actual coincide con el seleccionado
    public class SelectedColorBorderMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var current = values.Length > 0 ? values[0]?.ToString() : null;
            var selected = values.Length > 1 ? values[1]?.ToString() : null;
            return current == selected ? Colors.Black : Colors.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // Multi-binding: devuelve 1.0 (visible) si el item actual está seleccionado, 0.0 en caso contrario
    public class SelectedColorToOpacityMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var current = values.Length > 0 ? values[0]?.ToString() : null;
            var selected = values.Length > 1 ? values[1]?.ToString() : null;
            return current == selected ? 1.0 : 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }


}
