using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace MBMEKiosk.Infrastructure.Utils
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Boolean)
            {
                if (parameter is string)
                {
                    if (string.Compare("VisibleCollapsed", System.Convert.ToString(parameter), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
                    }
                    else if (string.Compare("CollapsedVisible", System.Convert.ToString(parameter), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return ((bool)value) ? Visibility.Collapsed : Visibility.Visible;
                    }
                }
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
