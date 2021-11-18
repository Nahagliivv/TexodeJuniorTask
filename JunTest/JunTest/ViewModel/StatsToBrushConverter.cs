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

            var avgStep = (int)values[0];
            var minStep = (int)values[1];
            var maxStep = (int)values[2];


            var minStat = avgStep - minStep;
            var maxStat = maxStep - avgStep;

            var checkedProcent = (avgStep / 100) * 20;


            return (checkedProcent < minStat || maxStat > checkedProcent) ? Brushes.Red : Brushes.White;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
