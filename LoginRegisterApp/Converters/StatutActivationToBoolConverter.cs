using System;
using System.Globalization;
using System.Windows.Data;

namespace LoginRegisterApp.Converters
{
    // "Actif" -> true (switch ON), "Desactive" -> false (switch OFF).
    // OneWay only - the switch's Click handler in MainWindow.xaml.cs decides
    // the new value and writes it through UserRepository, it doesn't rely on
    // ConvertBack.
    public class StatutActivationToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value as string) == "Actif";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
