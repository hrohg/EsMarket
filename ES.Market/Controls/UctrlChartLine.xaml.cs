using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ES.Business.Helpers;
using ES.Data.Models;
using UserControl = System.Windows.Controls.UserControl;

namespace ES.Shop.Controls
{
    /// <summary>
    /// Interaction logic for UctrlChartLine.xaml
    /// </summary>
    public partial class UctrlChartLine : UserControl
    {
        private List<InvoiceModel> Invoices { get; set; }
        public UctrlChartLine(List<InvoiceModel> invoices)
        {
            InitializeComponent();
            Invoices = invoices.OrderBy(s=>s.CreateDate).ToList();
            CmbBy.ItemsSource = new List<ItemProperty> { 
                new ItemProperty { Value = "Ժամվա", Key = 0 }, 
                new ItemProperty { Value = "Օրվա", Key = 1 }, 
                new ItemProperty { Value = "Շաբաթվա", Key = 2 }, 
                new ItemProperty { Value = "Ամսվա", Key = 3 }, 
                new ItemProperty { Value = "Տարվա", Key = 4 } };
            //CmbBy.SelectedValue = 1;
        }

        protected void SetChart()
        {
            if (CmbBy.SelectedValue == null) { return; }
            switch ((int)CmbBy.SelectedValue)
            {
                    //ByHour
                case 0:
                    var invociesGroupByHour = Invoices.OrderBy(s=>s.CreateDate.Hour).GroupBy(s => s.CreateDate.Hour);
                    var listByHour = new ObservableCollection<KeyValuePair<int, decimal>>();
                    foreach (var invoice in invociesGroupByHour)
                    {
                        if (!invoice.Any()) { continue; }
                        listByHour.Add(new KeyValuePair<int, decimal>(invoice.First().CreateDate.Hour, invoice.Sum(s => s.Total) / invoice.Select(s => s.CreateDate.Date).Distinct().Count()));
                        LineChart.DataContext = null;
                    } 
                    //LineChart.DataContext = listByHour;
                    LineChart.ItemsSource = listByHour;
                    break;
                    //ByDay
                case 1:
                    var invociesGroupByDay = Invoices.OrderBy(s => s.CreateDate.Day).GroupBy(s => s.CreateDate.Day);
                    var listByDay = new ObservableCollection<KeyValuePair<int, decimal>>();
                    foreach (var invoice in invociesGroupByDay)
                    {
                        if (!invoice.Any()) { continue; }
                        listByDay.Add(new KeyValuePair<int, decimal>(invoice.First().CreateDate.Day, invoice.Sum(s => s.Total) / invoice.Select(s=>s.CreateDate.Date).Distinct().Count()));
                        LineChart.DataContext = null;
                    } 
                    //LineChart.DataContext = listByDay;
                    LineChart.ItemsSource = listByDay;
                    break;
                    //ByWeek
                case 2:
                    var invociesGroupByWeek = Invoices.OrderBy(s => s.CreateDate.DayOfWeek).GroupBy(s => s.CreateDate.DayOfWeek);
                    var listByWeek = new ObservableCollection<KeyValuePair<DayOfWeek, decimal>>();
                    foreach (var invoice in invociesGroupByWeek)
                    {
                        if (!invoice.Any()) { continue; }
                        listByWeek.Add(new KeyValuePair<DayOfWeek, decimal>(invoice.First().CreateDate.DayOfWeek, invoice.Sum(s => s.Total) / invoice.Select(s => s.CreateDate.Date).Distinct().Count()));
                    }
                    LineChart.ItemsSource = listByWeek;
                    break;
                    //ByMonth
                case 3:
                    var invociesGroupByMonth = Invoices.OrderBy(s => s.CreateDate.Month).GroupBy(s => s.CreateDate.Month);
                    var listByMonth = new ObservableCollection<KeyValuePair<int, decimal>>();
                    foreach (var invoice in invociesGroupByMonth)
                    {
                        if (!invoice.Any()) { continue; }
                        listByMonth.Add(new KeyValuePair<int, decimal>(invoice.First().CreateDate.Month, invoice.Sum(s => s.Total) / invoice.Select(s => s.CreateDate.Date).Distinct().Count()));
                    } LineChart.ItemsSource = listByMonth;
                    break;
                    //ByYear
                case 4:
                    var invociesGroupByYear = Invoices.OrderBy(s => s.CreateDate.Year).GroupBy(s => s.CreateDate.Year);
                    var listByYear = new ObservableCollection<KeyValuePair<int, decimal>>();
                    foreach (var invoice in invociesGroupByYear)
                    {
                        if (!invoice.Any()) { continue;}
                        listByYear.Add(new KeyValuePair<int, decimal>(invoice.First().CreateDate.Month, invoice.Sum(s => s.Total) / invoice.Select(s => s.CreateDate.Date).Distinct().Count()));
                    } LineChart.ItemsSource = listByYear;
                    break;
                default:
                    break;
            }


        }

        protected void CmbBy_SelectionChanged(object sender, EventArgs e)
        {
            SetChart();
        }
    }
}
