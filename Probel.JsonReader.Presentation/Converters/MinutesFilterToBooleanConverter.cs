using System;
using System.Globalization;
using System.Windows.Data;

namespace Probel.JsonReader.Presentation.Converters
{
    public class MinutesFilterToBooleanConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int minutes && parameter is string param)
            {
                if (int.TryParse(param, out int p)) { return minutes == p; }
                else { return false; }
            }
            else { return null; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        #endregion Methods
    }
}