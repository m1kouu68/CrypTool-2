using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace CrypTool.Plugins.Anonymity
{
    public class EquivalenceClassColor : IValueConverter
    {





        private readonly List<Brush> _colors = new List<Brush>
    {
   
            Brushes.LightBlue,
        Brushes.LightGreen,
 
    };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null || (int)value == -1)  // Check if value is null or 0
                return Brushes.Transparent;


                // get the index of the color in the listth
                int groupID = (int)value;
                return _colors[groupID % _colors.Count];

            
            }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}