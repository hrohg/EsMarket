using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Data.Models;
using ES.Data.Models.Products;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ES.Business.Models.Views
{
    public class CommodityTurnover
    {
        public ProductModel Product { get; set; }
        public InvoiceModel CreateInvoice { get; set; }
        public ProductItemModel ProductItem { get; set; }
        public InvoiceType InvoiceType { get { return (InvoiceType)CreateInvoice.InvoiceTypeId; } }
        public string Type { get => EnumHelper.GetInvoiceTypeNames(InvoiceType); }
        public string Code { get { return Product != null ? Product.Code : ""; } }
        public string Description { get { return Product != null ? Product.Description : ""; } }
        public double Quantity { get; set; }
        public ObservableCollection<StockProducts> StockProducts { get; set; }
    }
}
