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
            //if (targetType != typeof(UserRoleEnum))
            //{ return false;}
            var type = (UserRoleEnum)parameter;
            
            switch (type)
            {
                case UserRoleEnum.Admin:
                    return ApplicationManager.Instance.UserRoles.Any(r => r.Id == 1);
                    break;
                case UserRoleEnum.Director:
                    return ApplicationManager.Instance.UserRoles.Any(r => r.Id == 2);

                    break;
                case UserRoleEnum.Manager:
                    return ApplicationManager.Instance.UserRoles.Any(r => r.Id == 3);

                    break;
                case UserRoleEnum.StockKeeper:
                    return ApplicationManager.Instance.UserRoles.Any(r => r.Id == 4);

                    break;
                case UserRoleEnum.SaleManager:
                    return ApplicationManager.Instance.UserRoles.Any(r => r.Id == 5);
                    
                    break;
                case UserRoleEnum.SeniorSaler:
                    return ApplicationManager.Instance.UserRoles.Any(r => r.Id == 6);

                    break;
                case UserRoleEnum.Saller:
                    return ApplicationManager.Instance.UserRoles.Any(r => r.Id == 7);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            throw new InvalidOperationException("Converter can only convert to value of type Visibility.");
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new Exception("Invalid call - one way only");
        }
    }
}
