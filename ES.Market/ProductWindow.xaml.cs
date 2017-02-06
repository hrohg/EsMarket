using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ESL.DataAccess.Mangers;
using ESL.Business.FileManager;
using ESL.MsOffice.ExcelManager;
using ESL.DataAccess.Models;

namespace ESL.Shop
{
    /// <summary>
    /// Interaction logic for ProductWindow.xaml
    /// </summary>
    public partial class ProductWindow : Window
    {
        public ProductWindow()
        {
            InitializeComponent();
        }

        private void WinProducts_Loaded(object sender, RoutedEventArgs e)
        {
            DgProducts.DataContext = ProductsManager.GetProductsByMember(1);
        }

        private void ShowHideDetails(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    row.DetailsVisibility =
                      row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
        }

        private void MiImportFromExcel_Click(object sender, RoutedEventArgs e)
        {
            var filePath=FileManager.OpenExcelFile();
            if (File.Exists(filePath))
            {
                var productObjects = ExcelImportManager.ImportProducts(filePath);
                var product = (List<Product>)productObjects.Item1;
                DgProducts.DataContext = product;
            }
        }

        private void MiExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            FileManager.OpenFiles();
        }
    }
}
