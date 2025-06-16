using System.Globalization;

namespace EscolarAppPadres.Helpers
{
    public class TipoDocumentoToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int tipoDocumento)
            {
                return tipoDocumento switch
                {
                    1 => Color.FromArgb("#2196F3"), // Azul para Inscripciones
                    2 => Color.FromArgb("#4CAF50"), // Verde para Colegiaturas
                    3 => Color.FromArgb("#FF9800"), // Naranja para Otros
                    _ => Color.FromArgb("#9E9E9E")  // Gris por defecto
                };
            }
            return Color.FromArgb("#9E9E9E");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}