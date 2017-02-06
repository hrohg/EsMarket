using System;
using System.Collections.Generic;
using System.Windows;

namespace UserControls.ControlPanel.Controls
{
    public static class SelectManager
    {
        public static List<ItemsToSelect> GetSelectedItem(List<ItemsToSelect> items, bool allowMultipleChoise)
        {
            var window = new SelectItems(items, allowMultipleChoise);
            window.ShowDialog();
            if (window.DialogResult == null || !(bool)window.DialogResult || window.SelectedItems==null || window.SelectedItems.Count == 0)
            {
                MessageBox.Show("Գործողությունն ընդհատված է։", "Գործողության ընդհատում", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return new List<ItemsToSelect>();
            }
            return window.SelectedItems;
        }
        public static Tuple<DateTime, DateTime> GetDateIntermediate()
        {
            var window = new SelectDateIntermediate(DateTime.Today, DateTime.Now);
            window.ShowDialog();
            if (window.DialogResult == null || !(bool)window.DialogResult)
            {
                return null;
                return new Tuple<DateTime, DateTime>(window.StartDate??DateTime.Today, window.EndDate!=null?window.EndDate.Value.AddDays(1):DateTime.Now);
            }
            return new Tuple<DateTime, DateTime>(window.StartDate ?? DateTime.Today, window.EndDate != null ? window.EndDate.Value.AddDays(1) : DateTime.Now);
        }
        public static Tuple<DateTime, DateTime> GetDateIntermediate(DateTime defaultStartDate , DateTime defaultEndDate)
        {
            var window = new SelectDateIntermediate(defaultStartDate, defaultEndDate);
            window.ShowDialog();
            if (window.DialogResult == null || !(bool)window.DialogResult)
            {
                return new Tuple<DateTime, DateTime>(window.StartDate ?? DateTime.Today, window.EndDate != null ? window.EndDate.Value.AddDays(1) : DateTime.Now);
            }
            return new Tuple<DateTime, DateTime>(window.StartDate ?? DateTime.Today, window.EndDate != null ? window.EndDate.Value.AddDays(1) : DateTime.Now);
        }
        public static string ReadText(string oldText, string description)
        {
            var ui = new InputBox(description, oldText);
            ui.ShowDialog();
            if (ui.DialogResult==true)
            {
                return ui.InputValue;
            }
            return null;
        }

        
    }
}
