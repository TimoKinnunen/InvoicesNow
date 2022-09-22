using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace InvoicesNow.Converters
{
    public sealed class CurrentCultureConverter : IValueConverter
    {
        // This converts the value object to the string to display.
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            // Retrieve the format string and use it to format the value.
            string formatString = parameter as string;
            if (!string.IsNullOrEmpty(formatString))
            {
                try
                {
                    return string.Format(CultureInfo.CurrentCulture, formatString, value);
                }
                catch
                {
                }
            }
            // If the format string is null or empty, simply call ToString()
            // on the value.
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
