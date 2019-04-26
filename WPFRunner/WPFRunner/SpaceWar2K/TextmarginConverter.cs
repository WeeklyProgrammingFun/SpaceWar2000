using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WPFRunner.SpaceWar2K
{
    class TextMarginConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var width = (double)values[0];
            var height = (double)values[1];
            var radius = (double)values[2];
            //Debug.WriteLine(String.Format("{0},{1} : {2}",width,height,radius));
            var a = new Thickness(radius / 2 - width / 2, radius / 2 - height / 2, 0, 0);
            return a;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
