using System;
using System.Globalization;
using System.Windows.Data;
using ES.Business.Managers;
using ES.Common.Enumerations;

namespace UserControls.Converters
{
    public class UserRoleToBoolConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            var type = (UserRoleEnum?)parameter;
            return ApplicationManager.IsInRole(type);
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new Exception("Invalid call - one way only");
        }
    }
}
