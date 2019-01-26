using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace ES.Business.Helpers
{
    /// <summary>
    /// Interaction logic for SelectItems.xaml
    /// </summary>
    public partial class SelectItemsByCheck : Window
    {
        private readonly List<ProductToSelectByCheck> _productItemsByCkeck;
        private readonly List<ItemsToSelectByCheck> _itemsByCkeck;
        private readonly bool _allowMultipleChoise;
        Timer _timer = null;
        public List<ProductToSelectByCheck> SelectedProductItems = new List<ProductToSelectByCheck>();
        public List<ItemsToSelectByCheck> SelectedItems = new List<ItemsToSelectByCheck>();
        public SelectItemsByCheck(List<ProductToSelectByCheck> list, bool allowMultipleChoise, string title = "Ընտրել")
        {
            InitializeComponent();
            _allowMultipleChoise = allowMultipleChoise;
            Title = title;
            if (list == null || list.Count == 0) { return; }
            //DgItems.SelectionMode = allowMultipleChoise ? SelectionMode.Multiple : SelectionMode.Single;
            DgItems.DisplayMemberPath = "DisplayName";
            DgItems.SelectedValuePath = "SelectedValue";
            DgItems.ItemsSource = _productItemsByCkeck = list;
        }
        public SelectItemsByCheck(List<ItemsToSelectByCheck> list, string title = "Ընտրել")
        {
            InitializeComponent();
            Title = title;
            if (list == null || list.Count == 0) { return; }
            DgItems.DisplayMemberPath = "DisplayName";
            DgItems.SelectedValuePath = "SelectedValue";
            DgItems.ItemsSource = _itemsByCkeck = list;
        }

        #region SelectItems Events

        private void WinSelectItems_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    //Close();
                    break;
            }
        }
        private void WinSelectItems_Loaded(object sender, EventArgs e)
        {
            TxtSearchText.Focus();
        }
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            if(_productItemsByCkeck!=null)
            {DgItems.ItemsSource = _productItemsByCkeck.Where(s => s.DisplayName.ToLower().Contains(TxtSearchText.Text.ToLower())).ToList(); }
            if (_itemsByCkeck != null)
            {
                try
                {
                DgItems.ItemsSource = _itemsByCkeck.Where(s => string.IsNullOrEmpty(TxtSearchText.Text) || string.IsNullOrEmpty(s.Description) || s.Description.ToLower().Contains(TxtSearchText.Text.ToLower())).ToList();

                }
                catch (Exception)
                {
                    
                  
                }
            }
        }
        private void BtnAccept_Click(object sender, EventArgs e)
        {
            if(_productItemsByCkeck!=null)
            {foreach (ProductToSelectByCheck item in DgItems.Items)
            {
                if (!item.IsChecked) continue;

                SelectedProductItems.Add(item);
            }}
            if (_itemsByCkeck != null)
            {
                foreach (ItemsToSelectByCheck item in DgItems.Items)
                {
                    if (!item.IsChecked) continue;

                    SelectedItems.Add(item);
                }
            }
            DialogResult = true;
            Close();
        }
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            SelectedProductItems = null;
            DialogResult = false;
            Close();
        }
        private void TxtSearchText_Changed(object sender, EventArgs e)
        {
            DisposeTimer(); 
            _timer = new Timer(TimerElapsed, null, 300, 300);
            TxtSearchText.Text = TxtSearchText.Text.ToLower();
        }
        private void TimerElapsed(Object obj)
        {
            BtnSearch_Click(null,null);
            DisposeTimer();
        }

        private void DisposeTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
       
        private void TxtSearchText_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                case Key.Down:
                    DgItems.Focus();
                    DgItems.SelectedIndex = 0;
                    break;
                case Key.Up:
                    DgItems.Focus();
                    DgItems.SelectedIndex = DgItems.Items.Count;
                    break;
            }
        }

        private void LvItems_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    //BtnAccept_Click(null, null);
                    break;
                case Key.Up:
                    if (DgItems.SelectedIndex == DgItems.Items.Count) DgItems.SelectedIndex = 0;
                    break;
                case Key.Down:
                    if (DgItems.SelectedIndex == 0) DgItems.SelectedIndex = DgItems.Items.Count;
                    break;
                default:
                    //TxtSearchText.Focus();
                    break;
            }
        }
        #endregion
    }

   public class ProductToSelectByCheck
    {
        public ProductToSelectByCheck() { }
        public ProductToSelectByCheck(Guid id, string code, string displayName, decimal? count)
        {
            GuidId = id;
            DisplayName = displayName;
            Code = code;
            Count = ExCount = count??0;
        }

        public Guid GuidId=Guid.NewGuid();
        public bool IsChecked { get; set; }
        public string DisplayName { get; private set; }
        public string Code { get; private set; }
        public decimal ExCount { get; private set; }
        public decimal? Count { get; set; }
    }
    public class ItemsToSelectByCheck
    {
        public string Description { get; set; }
        public object Value;
        public bool IsChecked { get; set; }
    }
}
