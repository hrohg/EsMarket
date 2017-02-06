using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UserControls.ControlPanel.Controls
{
    /// <summary>
    /// Interaction logic for SelectItems.xaml
    /// </summary>
    public partial class SelectItems : Window
    {
        private readonly List<ItemsToSelect> _items;
        public List<ItemsToSelect> SelectedItems = new List<ItemsToSelect>();
        public SelectItems(List<ItemsToSelect> list, bool allowMultipleChoise)
        {
            InitializeComponent();
            if (list == null || list.Count == 0) { return;}
            LvItems.SelectionMode = allowMultipleChoise ? SelectionMode.Multiple: SelectionMode.Single;
            LvItems.DisplayMemberPath = "DisplayName";
            LvItems.SelectedValuePath = "SelectedValue";
            LvItems.ItemsSource = _items = list;
        }
        #region SelectItems Events

        private void WinSelectItems_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
            }
        }
        private void WinSelectItems_Loaded(object sender, EventArgs e)
        {
            TxtSearchText.Focus();
        }
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            LvItems.ItemsSource =_items.Where(s=>s.DisplayName.ToLower().Contains(TxtSearchText.Text.ToLower())).ToList();
        }
        private void BtnAccept_Click(object sender, EventArgs e)
        {
            foreach (ItemsToSelect item in LvItems.SelectedItems)
            {
                SelectedItems.Add(item);
            }
            DialogResult = true;
            Close();
        }
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            SelectedItems = null;
            DialogResult = false;
            Close();
        }
        private void TxtSearchText_Changed(object sender, EventArgs e)
        {
            BtnSearch_Click(null,null);
        }

        private void TxtSearchText_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                case Key.Down:
                    LvItems.Focus();
                   LvItems.SelectedIndex = 0;
                    break;
                case Key.Up:
                    LvItems.Focus();
                    LvItems.SelectedIndex = LvItems.Items.Count;
                    break;
            }
        }

        private void LvItems_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    BtnAccept_Click(null, null);
                    break;
                case Key.Up:
                    if (LvItems.SelectedIndex == LvItems.Items.Count) LvItems.SelectedIndex = 0;
                    break;
                case Key.Down:
                    if (LvItems.SelectedIndex == 0) LvItems.SelectedIndex = LvItems.Items.Count;
                    break;
                default:
                    TxtSearchText.Focus();
                    break;
            }
        }
        private void LvItems_MouseDoubleClick(object sender, EventArgs e)
        {
            BtnAccept_Click(null, null);
        }
        #endregion
    }

    public class ItemsToSelect
    {
        public ItemsToSelect() { }
        public ItemsToSelect(string displayName, object selectedItem)
        {
            DisplayName = displayName;
            SelectedValue = selectedItem;
        }
        public string DisplayName { get; set; }
        public object SelectedValue { get; set; }
    }
    
}
