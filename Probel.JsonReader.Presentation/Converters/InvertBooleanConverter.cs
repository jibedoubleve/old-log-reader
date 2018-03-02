using System;
using System.Globalization;
using System.Windows.Data;

namespace Probel.JsonReader.Presentation.Converters
{
    public class InvertBooleanConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool vBool) { return !vBool; }
            else { return false; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool vBool) { return !vBool; }
            else { return false; }
        }
        #endregion Methods
    }
}