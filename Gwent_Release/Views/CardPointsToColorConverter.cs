using Gwent_Release.Models.CardsNS;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Gwent_Release.Views
{
    public class CardPointsToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is HeroCard)
            {
                return Brushes.White;
            }
            else if (value is UnitCard unitCard)
            {
                if (unitCard.ActualCardScore > unitCard.DefaultCardScore)
                    return Brushes.Green;
                else if (unitCard.ActualCardScore < unitCard.DefaultCardScore)
                    return Brushes.Red;
                else
                    return Brushes.Black;
            }

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}