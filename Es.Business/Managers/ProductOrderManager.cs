using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ES.Business.Helpers;
using ES.Data.Models;
using ProductModel = ES.Data.Models.Products.ProductModel;


namespace ES.Business.Managers
{
    public class ProductOrderManager : BaseManager
    {
        public static List<ProductOrderModelBase> GetSaleQuantity(List<ProductOrderModelBase> productOrders, DateTime startDate, DateTime endDate)
        {
            if (productOrders == null) return productOrders;
            using (var db = GetDataContext())
            {
                foreach (var productOrderModel in productOrders)
                {
                    var saleQuantity = (double)(db.InvoiceItems.Where(s =>
                                                    s.Invoices.MemberId == ApplicationManager.Instance.GetMember.Id && s.Invoices.InvoiceTypeId == (int)InvoiceType.SaleInvoice &&
                                                        //InvoicesManager.GetInvoiceTypes(InvoiceTypeEnum.SaleInvoice).Contains((int)s.Invoices.InvoiceTypeId) &&
                                                    s.Invoices.ApproveDate >= startDate && s.Invoices.ApproveDate <= endDate && s.ProductId == productOrderModel.ProductId).Sum(s => s.Quantity) ?? 0);
                    productOrderModel.SaleQuantity = saleQuantity;
                }
            }
            return productOrders;
        }
    }

    public class ProductOrderModelBase
    {
        public Guid ProductId { get; set; }
        public ProductModel Product { get; set; }
        public PartnerModel Provider { get; set; }
        public double SaleQuantity { get; set; }
        public double DemandQuantity
        {
            get
            {
                return SaleQuantity + (double)(Product.MinQuantity ?? 0 - Product.ExistingQuantity ?? 0);
            }
        }

        public ProductOrderModelBase()
        {

        }

    }
}
