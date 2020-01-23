using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Չմ", Binding = new Binding("Product.Mu") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Գին", Binding = new Binding("Product.Price") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Առկա է", Binding = new Binding("ExistingQuantity") });
            //Add Stocks Columns data
            if (dataGrid.ColumnHeaders != null)
                foreach (var value in dataGrid.ColumnHeaders)
                {
                    var column = new SortableDataGridTextColumn()
                    {
                        Header = value.Name,
                        Binding = new Binding("StockProducts") { ConverterParameter = value.Name, Converter = new StockProductsConverter() },
                        CustomSort = new StockProductsCustomSort() { Key = value.Name },
                        CanUserSort = true
                    };
                    dataGrid.Columns.Add(column);
                }
        }

        protected override void OnSorting(DataGridSortingEventArgs eventArgs)
        {
            if (eventArgs.Column == null || eventArgs.Column.CanUserSort != true || !(eventArgs.Column is SortableDataGridTextColumn))
            {
                base.OnSorting(eventArgs);
                return;
            }

            ListCollectionView view = (ListCollectionView)CollectionViewSource.GetDefaultView(this.ItemsSource);

            var customSort = ((SortableDataGridTextColumn)eventArgs.Column).CustomSort;
            customSort.SortDirection = eventArgs.Column.SortDirection ?? ListSortDirection.Ascending;

            var sortDirection = customSort.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            view.CustomSort = customSort;
            eventArgs.Column.SortDirection = sortDirection;
            view.SortDescriptions.Add(new SortDescription(((SortableDataGridTextColumn)eventArgs.Column).Header.ToString(), sortDirection));
            //eventArgs.Handled = true;
            base.OnSorting(eventArgs);
        }


    }

    public class SortableDataGridTextColumn : DataGridTextColumn
    {
        public static readonly DependencyProperty CustomSortProperty = DependencyProperty.Register("CustomSort", typeof(CustomSort), typeof(SortableDataGridTextColumn));

        public CustomSort CustomSort
        {
            get { return GetValue(CustomSortProperty) as CustomSort; }
            set { SetValue(CustomSortProperty, value); }
        }
    }
    public class StockProductsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var items = value as IEnumerable<StockProducts>;
            if (items != null && parameter != null)
            {
                var item = items.FirstOrDefault(s => s.Stock.Name == parameter.ToString());
                if (item != null)
                    return item.Quantity;
                return 0;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CustomSort : IComparer
    {
        public ListSortDirection? SortDirection { get; set; }
        protected int Direction
        {
            get { return SortDirection == ListSortDirection.Ascending ? 1 : -1; }
        }
        public string Key { get; set; }
        public virtual int Compare(object x, object y)
        {
            return 0;
        }
    }

    public class StockProductsCustomSort : CustomSort
    {
        public override int Compare(object x, object y)
        {
            var xVal = x as ProductOrderModel;
            var yVal = y as ProductOrderModel;
            if (xVal == null) return 0;
            if (yVal == null) return 0;
            var xValQ = xVal.StockProducts.Where(s => s.Stock.Name == Key).Select(s => s.Quantity).FirstOrDefault();
            var yValQ = yVal.StockProducts.Where(s => s.Stock.Name == Key).Select(s => s.Quantity).FirstOrDefault();
            return xValQ == yValQ ? 0 : Direction * (xValQ > yValQ ? 1 : -1);
        }
    }
}
