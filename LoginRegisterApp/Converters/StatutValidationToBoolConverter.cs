using System;
using System.Globalization;
using System.Windows.Data;

namespace LoginRegisterApp.Converters
{
    // "Valide" -> true (switch ON), "NonValide" -> false (switch OFF).
    public class StatutValidationToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value as string) == "Valide";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
