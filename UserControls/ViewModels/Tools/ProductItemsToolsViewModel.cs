using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Models;
using Xceed.Wpf.AvalonDock.Layout;

namespace UserControls.ViewModels.Tools
{
    public class ProductItemsToolsViewModel : ToolsViewModel
    {
        #region Events

        public delegate void OnSelectProductItemDelegate(ProductItemModel productItem);
        public event OnSelectProductItemDelegate OnProductItemSelected;
        #endregion Events

        #region Internal proeprties

        private List<ProductModel> _products;
        private List<ProductItemModel> _productItems;
        #endregion Internal properties

        #region External properties
        public List<CustomItem> Items { get; private set; }
        public CustomItem SelectedItem { get; set; }
        #endregion External proeprties

        #region Constructors

        public ProductItemsToolsViewModel()
        {
            Initialize();
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {
            Title = "Ապրանքներ";
            IsFloating = true;
            CanFloat = true;
            AnchorSide = AnchorSide.Left;
            UpdateProducts();
            SelectItemCommand = new RelayCommand<CustomItem>(OnSelectItem);
        }

        private void UpdateProducts()
        {
            _products = ApplicationManager.CashManager.Products.OrderBy(s => s.Description).ToList();
            Items = _products.Select(p => new CustomItem(p.Id, string.Format("{0} ({1} {2})", p.Description, p.Code, p.Price))).ToList();
        }

        private void OnSelectItem(CustomItem item)
        {
            var handle = OnProductItemSelected;
            if (handle != null)
            {
                handle(null);
            }
        }
        #endregion Internal methods

        #region Commands
        public ICommand SelectItemCommand { get; private set; }
        #endregion Commands
    }

    public class CustomItem
    {
        public Guid Value { get; set; }
        public string Description { get; set; }

        public CustomItem(Guid id, string description)
        {
            Value = id;
            Description = description;
        }
    }
}
