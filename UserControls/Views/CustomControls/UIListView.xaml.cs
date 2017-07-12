using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ES.Business.ExcelManager;
using UserControls.Helpers;

namespace UserControls.Views.CustomControls
{
    /// <summary>
    /// Interaction logic for UIListView.xaml
    /// </summary>
    public partial class UIListView : Window
    {
        private readonly IEnumerable<object> _list; 
        public UIListView(IEnumerable<object> list, string title = "Վերծանում", double total = 0)
        {
            InitializeComponent();
            _list = list;
            var itemsSource = list as IList<object> ?? list.ToList();
            if(itemsSource.Count==0) return;
            var columns = itemsSource.ToList().First() != null ? itemsSource.ToList().First().GetType().GetProperties() : new PropertyInfo[0];
            foreach (var column in columns.Select(item => new DataGridTextColumn
            {
                Header = item.Name,
                Binding = new Binding(item.Name) { Mode = BindingMode.OneTime}
            }))
            {
                DgView.Columns.Add(column);
            }
            DgView.ItemsSource = itemsSource;
            DgView.ItemsSource = itemsSource.ToList();
            txtRowCount.Text = _list.Count().ToString();
            txtTotal.Text = total.ToString("N");
            Title = title;
        }

        public void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void BtnExportToExcel_Click(object sender, EventArgs e)
        {
            ExcelExportManager.ExportList(_list);
        }
        public void BtnPrint_Click(object sender, EventArgs e)
        {
            var itemsSource = _list as IList<object> ?? _list.ToList();
            var columns = itemsSource.ToList().First() != null ? itemsSource.ToList().First().GetType().GetProperties() : new PropertyInfo[0];
            var dgList = new DataGrid();
            foreach (var column in columns.Select(item => new DataGridTextColumn
            {
                Header = item.Name,
                Binding = new Binding(item.Name)
            }))
            {
                dgList.Columns.Add(column);
            }
            dgList.ItemsSource = itemsSource.ToList();
            PrintManager.Print(dgList, true);
        }
    }
}
