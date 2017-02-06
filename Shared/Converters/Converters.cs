using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Shared.Helpers;
using System.Windows.Media;
using ES.Common.Cultures;
using ES.Common.Enumerations;


namespace Shared.Converters
{

	[ValueConversion(typeof(Boolean), typeof(Visibility))]
	public class BoolToVisibilityInvertConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value != null && (bool)value)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	[ValueConversion(typeof(Boolean), typeof(Visibility))]
	public class BoolToVisibilityWithHiddenInvertConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value != null && (bool)value)
			{
				return Visibility.Hidden;
			}
			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	[ValueConversion(typeof(Boolean), typeof(Visibility))]
	public class BoolToVisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null || !(bool)value)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	[ValueConversion(typeof(int), typeof(Visibility))]
	public class NullToBoolConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null) return false;
			return (bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value != null && (bool)value)
			{
				return value;
			}
			return null;
		}

		#endregion
	}


	public class HasItemsToEnabilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				var list = value as IList;
				return list != null && list.Count > 0;
			}
			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}


	


	public class ErrorsArrayToErrorString : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is IEnumerable<ValidationError>)
			{
				string result = "";
				IEnumerable<ValidationError> enumerable = (IEnumerable<ValidationError>)value;
				foreach (ValidationError error in enumerable)
				{
					result += error.ErrorContent + "\n";
				}
				return result.Trim();
			}
			return string.Format("{0} - {1}", CultureResources.Inst["{0}"], value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new InvalidOperationException();
		}

		#endregion
	}

	public class EstateTypeIDToEarthPlaceTypesVisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				return (int)value == 3 ? Visibility.Visible : Visibility.Collapsed;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class EstateTypeIDToOfficePlaceTypesVisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				return (int)value == 4 ? Visibility.Visible : Visibility.Collapsed;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class OrderTypeIDToPricePerDayVisibilityConverter : IMultiValueConverter
	{
		#region IMultiValueConverter Members

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values != null && values.Count() == 2)
			{
				if (values[1] != null && (int)values[1] == 3) return Visibility.Collapsed;
				return (values[0] != null && (int)values[0] == 2) ? Visibility.Visible : Visibility.Collapsed;
			}

			return Visibility.Collapsed;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}


	public class DateTimeToFormattedDateTimeConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				return ((DateTime)value).ToString("dd.MM.yyyy");
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class StringEmptyToNullConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return string.Empty;
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || string.IsNullOrEmpty(value.ToString()))
			{
				return null;
			}
			return value;
		}

		#endregion
	}







	public class NullToVisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				decimal d;
				if (decimal.TryParse(value.ToString(), out d))
				{
					return d > 0 ? Visibility.Visible : Visibility.Collapsed;
				}
				int i;
				if (int.TryParse(value.ToString(), out i))
				{
					return i > 0 ? Visibility.Visible : Visibility.Collapsed;
				}
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}


	public class DaylyArendVisibilityConverter : IMultiValueConverter
	{
		#region IMultiValueConverter Members

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values != null && values.Count() == 2)
			{
				if ((Visibility)values[1] == Visibility.Collapsed)
				{
					return Visibility.Collapsed;
				}
				return new BoolToVisibilityConverter().Convert(values[0], targetType, parameter, culture);
			}
			return Visibility.Collapsed;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class IntToVisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null && (int)value > 0)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class FilePathToFileNameConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				return Path.GetFileName(value.ToString());
			}
			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class EstateTypeToEarchPlaseAndHouseVisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null && ((int)value == 2 || (int)value == 3))
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}


	public class IsRentToStringConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				bool isForRent;
				if (bool.TryParse(value.ToString(), out isForRent))
				{
					return isForRent ? CultureResources.Inst["Rent"] : CultureResources.Inst["Buy"];
				}

				return value;
			}
			return CultureResources.Inst["Buy"];
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class StringToOneRowStringConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				string valStr = value.ToString().Replace(Environment.NewLine, " ");
				if (valStr.Length > 50)
				{
					valStr = valStr.Substring(0, 50) + "...";
				}
				return valStr;
			}
			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class UploadVisibilityConverter : IMultiValueConverter
	{
		#region IMultiValueConverter Members

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values != null && values.Length == 2)
			{
				if ((bool)values[0] && (bool)values[1])
				{
					return Visibility.Visible;
				}
			}
			return Visibility.Collapsed;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}



	public class EstateToActionButtonTextConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				return (int)value > 0 ? CultureResources.Inst["Save"] : CultureResources.Inst["Add"];
			}
			return CultureResources.Inst["Add"];
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
    public class OrderTypeToExchangeVisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				return (int)value == 2 ? Visibility.Collapsed : Visibility.Visible;
			}
			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class VisibilityMultiConverter : IMultiValueConverter
	{
		#region IMultiValueConverter Members

		public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
		{
			int? index = parameter != null ? (int?)int.Parse(parameter.ToString()) : null;
			for (int i = 0; i < value.Length; i++)
			{
				if (value[i] is Visibility)
				{
					if (index.HasValue && i != index && value[index.Value] is Visibility && ((Visibility)value[index.Value] == Visibility.Visible) && ((Visibility)value[i] == Visibility.Visible)) return Visibility.Visible;
					if (!index.HasValue && (Visibility)value[i] == Visibility.Collapsed) return Visibility.Collapsed;
				}
			}

			return !index.HasValue ? Visibility.Visible : Visibility.Collapsed;
		}

		public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class ShowingColumnsToVisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null && parameter != null)
			{
				var showableColumns = value as IEnumerable<ShowableColumn>;
				if (showableColumns == null) return Visibility.Collapsed;
				var columnID = System.Convert.ToInt32(parameter);

				var column = showableColumns.FirstOrDefault(s => s.ID == columnID);
				if (column == null) return Visibility.Visible;
				return column.Show ? Visibility.Visible : Visibility.Collapsed;
			}
			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class BoolToClientModeCheckBoxToolTipTextConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null && (bool)value)
			{
				return string.Format("{0} {1}", CultureResources.Inst["Off"], CultureResources.Inst["ClientShowMode"]);
			}
			return string.Format("{0} {1}", CultureResources.Inst["On"], CultureResources.Inst["ClientShowMode"]);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class FullAddressVisibilityConverter : IMultiValueConverter
	{
		#region IMultiValueConverter Members

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values != null && values.Length == 2)
			{
				var showAddress = values[0] != DependencyProperty.UnsetValue && (bool)values[0];
				bool isClientShowingMode = (bool)values[1];
				if (isClientShowingMode)
				{
					return Visibility.Collapsed;
				}
				return showAddress ? Visibility.Visible : Visibility.Collapsed;
			}
			return Visibility.Visible;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class BoolInvertConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return false;
			}
			return !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return false;
			}
			return !(bool)value;
		}

		#endregion
	}

	public class VideoObjToVideoFilePathConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				return new Uri(value.ToString());
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class BoolToVideoPlayerButtonImageSourceConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null && (bool)value)
			{
				return new BitmapImage(new Uri("pack://application:,,,/Shared;component/Images/pause.png"));
			}
			return new BitmapImage(new Uri("pack://application:,,,/Shared;component/Images/play.png"));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class SeekToSliderPositionConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return 0;
			TimeSpan ts = (TimeSpan)value;
			return ts.TotalMilliseconds;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return new TimeSpan();
			int SliderValue = System.Convert.ToInt32(value);
			return new TimeSpan(0, 0, 0, 0, SliderValue);

		}

		#endregion
	}

	public class SelectedItemsCountToContextMenuItemVisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				return (int)value != 1 ? Visibility.Collapsed : Visibility.Visible;
			}
			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}


	public class StrignEmptyToVisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null && !string.IsNullOrEmpty(value.ToString()) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class HasValueToVisibilityConverter : ConverterMarkupExtension<HasValueToVisibilityConverter>
	{
		#region IValueConverter Members

		public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool notreverse = true;
			if (parameter != null && parameter.ToString() == "!") notreverse = false;

			if (value != null)
			{
				bool result;
				if (value is long || value is int || value is long?)
				{
					result = notreverse ? (long.Parse(value.ToString()) > 0) : !(long.Parse(value.ToString()) > 0);
				}
				else if (value is double)
				{
					result = notreverse ? (double.Parse(value.ToString()) > 0d) : !(double.Parse(value.ToString()) > 0d);
				}
				else if (value is string)
				{
					result = notreverse ? !string.IsNullOrEmpty((string)value) : string.IsNullOrEmpty((string)value);
				}
				else if (value is DateTime || value is DateTime?)
				{
					result = notreverse ? !((DateTime)value).IsEmpty() : ((DateTime)value).IsEmpty();
				}
				else return notreverse ? Visibility.Collapsed : Visibility.Visible;

				return !result ? Visibility.Collapsed : Visibility.Visible;
			}

			return notreverse ? Visibility.Collapsed : Visibility.Visible;
		}

		public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Convert(value, targetType, parameter, culture);
		}

		#endregion
	}

	public class IsNullToEnabledConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}





	public class RoleToObjectVisibilityConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values != null && values.Length > 1)
			{
				if ((bool)values[2]) return Visibility.Collapsed; //offline mode

				var isAdmin = (bool)values[0];
				if (isAdmin)
				{
					return Visibility.Visible;
				}
				bool allowBrokerAddData;
				if (!bool.TryParse(values[1].ToString(), out allowBrokerAddData))
				{
					return Visibility.Collapsed;
				}

				return allowBrokerAddData ? Visibility.Visible : Visibility.Collapsed;
			}
			return Visibility.Collapsed;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class ElementsCountToTextColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				return ((int)value) > 11 ? "Red" : "Green";
			}
			return "Red";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class ElementsCountToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				return ((int)value) > 11 ? Visibility.Collapsed : Visibility.Visible;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class ReportTypeToBrokerVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return Visibility.Collapsed;
			ReportTypeEntity reportType = (ReportTypeEntity)value;
			return reportType.ID == (int)ReportTypes.ByAgency ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class ReportTypeToAgencyVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return Visibility.Collapsed;
			ReportTypeEntity reportType = (ReportTypeEntity)value;
			return reportType.ID == (int)ReportTypes.ByAgency ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}


	public class IsAdminOrDirectorToObjectVisibilityConverter : IMultiValueConverter
	{

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values != null && values.Length > 1)
			{
				if ((bool)values[1]) return Visibility.Collapsed; //offline mode

				if ((bool)values[0])
				{
					return Visibility.Visible;
				}
			}
			return Visibility.Collapsed;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class DataUpdateButtonVisibilityConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values != null && values.Length > 1)
			{
				var isOfflineMode = (bool)values[0];
				var serverIP = values[1].ToString();
				if (serverIP == "127.0.0.1" || serverIP.ToUpper() == "LOCALHOST")
				{
					return Visibility.Collapsed;
				}
				return isOfflineMode ? Visibility.Collapsed : Visibility.Visible;
			}
			return Visibility.Collapsed;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class ServerIPToItemVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return Visibility.Collapsed;
			var serverIP = value.ToString();
			if (serverIP == "127.0.0.1" || serverIP.ToUpper() == "LOCALHOST")
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class SiteDataUpdateButtonVisibilityConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values != null && values.Length > 1)
			{
				var isWebEnabled = (bool?)values[0];
				if (isWebEnabled ?? false)
				{
					var serverIP = values[1].ToString();
					if (serverIP == "127.0.0.1" || serverIP.ToUpper() == "LOCALHOST")
					{
						return Visibility.Visible;
					}
				}
				return Visibility.Collapsed;
			}
			return Visibility.Collapsed;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}


	public class IsMainToBorderThiknessConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return new Thickness(1);
			return (bool) value ? new Thickness(3) : new Thickness(1);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
	
	public class IsMainToToolTipConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return null;
			return (bool)value ? "Գլխավոր" : null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class LastModifiedDateToTextColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var date = value as DateTime?;
			if (date > DateTime.Now.AddDays(-1))
			{
				return Brushes.Red;
			}
			return Brushes.Black;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

    public class CollectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;
            var collection = value as IList;
            if (collection == null) return Visibility.Collapsed;
            return collection.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

  
}
