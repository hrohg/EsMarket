﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ES.Business.Managers;
using ES.Common.Enumerations;

namespace UserControls.Converters
{
    public class UserRoleToBoolConverter : IValueConverter
    {
        public object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            var type = (UserRoleEnum?)parameter;
            return ApplicationManager.IsInRole(type);
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new Exception("Invalid call - one way only");
        }
    }

    public class UserRoleToBoolMultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Any(value => ApplicationManager.IsInRole((UserRoleEnum) value));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new Exception("Invalid call - one way only");
        }
    }

    public class UserRoleToVisibilityConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            var type = (UserRoleEnum?)parameter;
            return ApplicationManager.IsInRole(type)?Visibility.Visible:Visibility.Collapsed;
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new Exception("Invalid call - one way only");
        }
    }
}
