using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Models.Products;
using System.Collections.Generic;
using System.Linq;

namespace UserControls.ViewModels.Reports
{
    public class ViewProductDetilesViewModel : DocumentViewModel
    {
        #region Internal properties
        #endregion Internal properties

        #region External properties
        
        public List<ProductModel> Products { get; set; }
        public ProductModel Product { get; set; }
        #endregion External properties

        #region Constructors
        public ViewProductDetilesViewModel()
        {
            Title = "Ապրանքների դիտում";
        }
        #endregion Constructors

        #region External methods
        public override void SetExternalText(ExternalTextImputEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Text)) return;
            Products = ProductsManager.GetProductsByCodeOrBarcode(e.Text);
            Product = Products.FirstOrDefault();
            RaisePropertyChanged("Products");
            RaisePropertyChanged("Product");
        }
        #endregion External methods
    }
}
