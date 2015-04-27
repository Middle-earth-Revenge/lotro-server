using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PacketBrowser
{
    public class PacketFilterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Here we receive the packet as first parameter and the filter string as the second one and return the visibility of the element

            PacketData packet = (PacketData)values[0];
            string filterQuery = (string)values[1];

            if (string.IsNullOrEmpty(filterQuery))
            {
                // Everything is visible
                return Visibility.Visible;
            }

            // Filter the packet

            // By ID
            string toFilter = packet.Index.ToString();
            if (toFilter.Contains(filterQuery))
            {
                return Visibility.Visible;
            }

            // By type
            toFilter = packet.Type.ToString("X8");
            if (toFilter.Contains(filterQuery))
            {
                return Visibility.Visible;
            }

            // By summary
            toFilter = packet.Summary;
            if (toFilter.Contains(filterQuery))
            {
                return Visibility.Visible;
            }

            // Didn't pass, collapse
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
