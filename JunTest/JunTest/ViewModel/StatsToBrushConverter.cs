using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace JunTest.ViewModel
{
    internal class StatsToBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            if (values[0] is int avgStep && values[1] is int minStep && values[2] is int maxStep)
            {
                var minStat = avgStep - minStep;
                var maxStat = maxStep - avgStep;

                var checkedProcent = (avgStep / 100) * 20;


                return (checkedProcent < minStat || maxStat > checkedProcent) ? Brushes.Red : Brushes.White;
            }
            else return Brushes.White;



        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
