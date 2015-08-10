using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace MBMEKiosk.Infrastructure.Utils
{
    public class LocalisedNumberConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string convertedValue = string.Empty;
            if (values.Length == 2 && values[0] is string && ((string)values[0]).Length > 0)
            {
                if (values[1] is string)
                {
                    if (string.Compare("arabic", System.Convert.ToString(values[1]), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        convertedValue = ConvertToArabicNumerals(values[0] as string);
                    }
                    else
                    {
                        convertedValue = values[0] as string;
                    }
                }
            }

            return convertedValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        private string ConvertToArabicNumerals(string input)
        {
            UTF8Encoding utf8Encoder = new UTF8Encoding();
            Decoder utf8Decoder = utf8Encoder.GetDecoder();
            StringBuilder convertedChars = new System.Text.StringBuilder();
            char[] convertedChar = new char[1];
            byte[] bytes = new byte[] { 217, 160 };
            char[] inputCharArray = input.ToCharArray();

            foreach (char c in inputCharArray)
            {
                if (char.IsDigit(c))
                {
                    bytes[1] = System.Convert.ToByte(160 + char.GetNumericValue(c));
                    utf8Decoder.GetChars(bytes, 0, 2, convertedChar, 0);
                    convertedChars.Append(convertedChar[0]);
                }
                else
                {
                    convertedChars.Append(c);
                }
            }

            return convertedChars.ToString();
        }
    }
}
