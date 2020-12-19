using ES.Data.Models;
using ProductModel = ES.Data.Models.Products.ProductModel;

namespace UserControls.Views.View.Models
{
    public class ProductItemsModel
    {
        public ProductItemModel ProductItem { get; set; }
        public ProductModel Product { get; set; }
        public StockModel Stock { get; set; }
    }
}
