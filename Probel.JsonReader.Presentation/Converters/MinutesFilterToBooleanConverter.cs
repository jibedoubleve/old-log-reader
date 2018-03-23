using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace Probel.JsonReader.Presentation.Converters
{
    public class MinutesFilterToBooleanConverter : IValueConverter
    {
        #region Methods
        private readonly IFormatProvider _formatProvider = new CultureInfo("en-US");
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal valueMin && parameter is string param)
            {
                if (decimal.TryParse(param, NumberStyles.AllowDecimalPoint, _formatProvider, out var paramMin))
                {
                    var result = valueMin == paramMin;
                    Debug.WriteLine($"Check for '{paramMin}' when minutes is '{valueMin}': {result}");
                    return result;
                }
                else { return false; }
            }
            else { return null; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        #endregion Methods
    }
}