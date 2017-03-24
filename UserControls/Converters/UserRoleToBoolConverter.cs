using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common.Enumerations;

namespace UserControls.Converters
{
    class UserRoleToBoolConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            var type = (UserRoleEnum)parameter;
            return ApplicationManager.Instance.IsInRole(type);
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new Exception("Invalid call - one way only");
        }
    }
}
