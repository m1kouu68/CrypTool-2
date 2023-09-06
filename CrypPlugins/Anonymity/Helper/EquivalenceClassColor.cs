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
        private readonly List<Brush> colors = new List<Brush>
    {
   
        Brushes.LightBlue,
        Brushes.LightCoral,
 
    };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null || (int)value == -1)  
                return Brushes.Transparent;


                // get the index of the color in the listth
                int groupID = (int)value;
                return colors[groupID % colors.Count];

            
            }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}