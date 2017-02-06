using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common.ViewModels.Base;
using ES.Data.Models;
using Xceed.Wpf.AvalonDock.Layout;

namespace UserControls.ViewModels.Tools
{
    public class ProductItemsToolsViewModel : PaneViewModel
    {
        #region Internal proeprties

        private List<ProductModel> _products;
        private List<ProductItemModel> _productItems; 
        #endregion Internal properties

        #region External properties
        public List<CustomItem> Items { get; private set; } 
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
        }

        private void UpdateProducts()
        {
             _products = ApplicationManager.CashManager.Products.OrderBy(s => s.Description).ToList();
             Items = _products.Select(p => new CustomItem (p.Id, string.Format("{0} ({1} {2})", p.Description, p.Code, p.Price))).ToList();
        }

        #endregion Internal methods

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
