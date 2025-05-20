using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Helpers
{
    public class CalificacionColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double calificacion)
            {
                if (calificacion >= 9)
                    return Color.FromArgb("#2ecc71"); // Verde
                else if (calificacion >= 7)
                    return Color.FromArgb("#f1c40f"); // Amarillo
                else
                    return Color.FromArgb("#e74c3c"); // Rojo
            }

            return Color.FromArgb("#bdc3c7"); // Gris para nulo
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
