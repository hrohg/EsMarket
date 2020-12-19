using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using UserControls.Controls;

namespace UserControls.ControlPanel.Controls.ExtendedControls
{
    public class ExtendedDataGrid : DataGrid
    {
        #region Dependency properties
        public ObservableCollection<DataGridColumnMetedata> ColumnMetadatas
        {
            get { return GetValue(ColumnMetadatasProperty) as ObservableCollection<DataGridColumnMetedata>; }
            set { SetValue(ColumnMetadatasProperty, value); }
        }

        public static readonly DependencyProperty ColumnMetadatasProperty = DependencyProperty.Register("ColumnMetadatas", typeof(ObservableCollection<DataGridColumnMetedata>), typeof(ExtendedDataGrid), new PropertyMetadata(OnColumnsChanged));

        static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            var dataGrid = d as ExtendedDataGrid;
            if (dataGrid == null) return;

            dataGrid.ColumnMetadatas.CollectionChanged -= OnColumnHeadersChanged;
            dataGrid.ColumnMetadatas.CollectionChanged += OnColumnHeadersChanged;
            dataGrid.Columns.Clear();

            //Add Grid Columns
            
            foreach (var value in dataGrid.ColumnMetadatas)
            {
                if (dataGrid.Columns.All(c => c.Header.ToString() != value.Header))
                {
                    var column =  new DataGridTextColumn();
                    column.IsReadOnly = !value.IsEditable;
                    column.Header = value.Header;
                    column.Binding = new Binding(value.Property);
                    column.SortMemberPath = value.Property;
                    column.CanUserSort = true;
                    dataGrid.Columns.Add(column);
                }
            }
            //dataGrid.Columns.Add(new DataGridTextColumn { Header = "Կոդ", Binding = new Binding("Product.Code") });
            //dataGrid.Columns.Add(new DataGridTextColumn { Header = "Անվանում", Binding = new Binding("Product.Description") });
            //dataGrid.Columns.Add(new DataGridTextColumn { Header = "Չմ", Binding = new Binding("Product.Mu") });
            //dataGrid.Columns.Add(new DataGridTextColumn { Header = "Գին", Binding = new Binding("Product.Price") });
            //dataGrid.Columns.Add(new DataGridTextColumn { Header = "Առկա է", Binding = new Binding("ExistingQuantity") });
            ////Add Stocks Columns data
            //if (dataGrid.ColumnHeaders != null)
            //    foreach (var value in dataGrid.ColumnHeaders)
            //    {
            //        var column = new SortableDataGridTextColumn()
            //        {
            //            Header = value.Name,
            //            Binding = new Binding("StockProducts") { ConverterParameter = value.Name, Converter = new StockProductsConverter() },
            //            CustomSort = new StockProductsCustomSort() { Key = value.Name },
            //            CanUserSort = true
            //        };
            //        dataGrid.Columns.Add(column);
            //    }
        }

        private static void OnColumnHeadersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var dataGrid = sender as ExtendedDataGrid;
            if (dataGrid == null) return;
            foreach (var value in e.NewItems.Cast<DataGridColumnMetedata>())
            {
                if (dataGrid.Columns.All(c => c.Header.ToString() != value.Header))
                {
                    var column = new SortableDataGridTextColumn()
                            {
                                Header = value.Header,
                                Binding = new Binding(value.Property),
                                SortMemberPath = value.Property,
                                CanUserSort = true
                            };
                    dataGrid.Columns.Add(column);
                }
            }
        }

        #endregion Dependency properties
        public ExtendedDataGrid()
        {
            Loaded += OnLoaded;
            SelectionChanged += OnSelectionChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //this.ScrollIntoView(Items[Items.Count-1]);
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItem == null)
            {
                return;
            }
            this.ScrollIntoView(SelectedItem);
        }
    }

    public class DataGridColumnMetedata
    {
        public string Header { get; set; }
        public string Property { get; set; }
        public bool IsEditable { get; set; }
    }
}
