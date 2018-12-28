using System;
using System.Collections.Generic;
using ES.Common.Managers;

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
                MessageManager.ShowMessage("Գործողությունն ընդհատված է։", "Գործողության ընդհատում");
                return new List<ItemsToSelect>();
            }
            return window.SelectedItems;
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
