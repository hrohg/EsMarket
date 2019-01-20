using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ES.Data.Models;

namespace UserControls.Controls
{
    public class ProductItemDataGrid : DataGrid
    {
        public ObservableCollection<StockModel> ColumnHeaders
        {
            get { return GetValue(ColumnHeadersProperty) as ObservableCollection<StockModel>; }
            set { SetValue(ColumnHeadersProperty, value); }
        }

        public static readonly DependencyProperty ColumnHeadersProperty = DependencyProperty.Register("ColumnHeaders", typeof(ObservableCollection<StockModel>), typeof(ProductItemDataGrid), new PropertyMetadata(OnColumnsChanged));

        static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = d as ProductItemDataGrid;
            if (dataGrid == null) return;
            dataGrid.Columns.Clear();
            //Add Product Columns
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Կոդ", Binding = new Binding("Product.Code") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Անվանում", Binding = new Binding("Product.Description") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Չմ", Binding = new Binding("Product.Mu")});
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Գին", Binding = new Binding("Product.Price") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Առկա է", Binding = new Binding("ExistingQuantity") });
            //Add Stocks Columns data
            foreach (var value in dataGrid.ColumnHeaders)
            {
                var column = new DataGridTextColumn
                {
                    Header = value.Name,
                    Binding = new Binding("StockProducts") { ConverterParameter = value.Name, Converter = new StockProductsConverter() },
                    SortMemberPath = "Name",
                    CanUserSort = true
                };
                dataGrid.Columns.Add(column);
            }
        }
    }

    public class StockProductsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var items = value as IEnumerable<StockProducts>;
            if (items != null && parameter != null)
            {
                var phone = items.FirstOrDefault(s => s.Stock.Name == parameter.ToString());
                if (phone != null)
                    return phone.Quantity;
                return 0;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
