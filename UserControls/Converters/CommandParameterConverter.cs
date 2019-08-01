using System;
using System.Windows.Data;
using ES.Business.Managers;

namespace UserControls.Converters
{
    public class GetInvoiceParameterConverter:IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new Tuple<InvoiceTypeEnum, InvoiceState, MaxInvocieCount>((InvoiceTypeEnum)values[0], (InvoiceState)values[1], (MaxInvocieCount)values[2]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GetInvocieByConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new Tuple<InvoiceTypeEnum, InvoiceState, MaxInvocieCount, OrderInvoiceBy>((InvoiceTypeEnum)values[0], (InvoiceState)values[1], (MaxInvocieCount)values[2], (OrderInvoiceBy)values[3]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
