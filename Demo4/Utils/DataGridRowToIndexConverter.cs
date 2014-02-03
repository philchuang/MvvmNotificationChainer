using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Demo4.Utils
{
    public class DataGridRowToIndexConverter : IValueConverter
    {
        public int Offset { get; set; }

        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dgr = (DataGridRow) value;
            return dgr.GetIndex () + Offset;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException ();
        }
    }
}
