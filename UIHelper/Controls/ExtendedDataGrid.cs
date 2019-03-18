using System.Windows;
using System.Windows.Controls;

namespace UIHelper.Controls
{
    public class ExtendedDataGrid : DataGrid
    {
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
            ScrollIntoView(SelectedItem);
        }
    }
}
