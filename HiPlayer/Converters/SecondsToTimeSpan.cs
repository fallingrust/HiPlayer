using System;
using System.Globalization;
using System.Windows.Data;

namespace HiPlayer.Converters
{
    public class SecondsToTimeSpan : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(0);
            if (value is double sec)
            {
                timeSpan = TimeSpan.FromSeconds((int)sec);
            }
            return timeSpan;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
