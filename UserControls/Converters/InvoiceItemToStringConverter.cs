using System;
using System.Globalization;
using System.Windows.Data;
using ES.Data.Models;

namespace UserControls.Converters
{
    public class InvoiceItemToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var invoiceItem = value as InvoiceItemsModel;
            if (invoiceItem == null) return null;
            return string.Format("{0} {1} {2} {3}x{4}={5}դր.",
                invoiceItem.Code,
                invoiceItem.Description,
                invoiceItem.Mu,
                invoiceItem.Quantity.HasValue ? invoiceItem.Product.IsWeight ? invoiceItem.Quantity.Value.ToString("N3") : invoiceItem.Quantity.Value.ToString("N0") : "0", invoiceItem.Price != null ? invoiceItem.Price.Value.ToString("N0") : "0", invoiceItem.Amount.ToString("N0"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
