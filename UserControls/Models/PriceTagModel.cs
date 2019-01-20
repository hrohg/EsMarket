using ES.Data.Models;

namespace UserControls.Models
{
    public class PriceTagModel
    {
        private readonly EsProductModel _product;
        public string Name { get { return _product.Description; } }
        public string Description { get { return _product.Note; } }
        public decimal Price { get { return _product.Price ?? 0; } }
        public decimal? OldPrice { get { return _product.OldPrice; } }

        public PriceTagModel(EsProductModel product)
        {
            _product = product;
        }
    }
}
