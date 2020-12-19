//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ES.DataAccess.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Products
    {
        public Products()
        {
            this.InvoiceItems = new HashSet<InvoiceItems>();
            this.ProductCategories = new HashSet<ProductCategories>();
            this.ProductItems = new HashSet<ProductItems>();
            this.ProductKeys = new HashSet<ProductKeys>();
            this.ProductOrderItems = new HashSet<ProductOrderItems>();
        }
    
        public System.Guid Id { get; set; }
        public string Code { get; set; }
        public string Barcode { get; set; }
        public string HCDCS { get; set; }
        public string Description { get; set; }
        public Nullable<short> MeasureOfUnitsId { get; set; }
        public string Note { get; set; }
        public Nullable<decimal> CostPrice { get; set; }
        public Nullable<decimal> OldPrice { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public Nullable<decimal> DealerPrice { get; set; }
        public Nullable<decimal> DealerDiscount { get; set; }
        public Nullable<decimal> MinQuantity { get; set; }
        public int EsMemberId { get; set; }
        public bool IsEnable { get; set; }
        public Nullable<int> BrandId { get; set; }
        public int LastModifierId { get; set; }
        public System.DateTime LastModifiedDate { get; set; }
        public Nullable<short> ExpiryDays { get; set; }
    
        public virtual Brands Brands { get; set; }
        public virtual EsMembers EsMembers { get; set; }
        public virtual EsUsers EsUsers { get; set; }
        public virtual ICollection<InvoiceItems> InvoiceItems { get; set; }
        public virtual ICollection<ProductCategories> ProductCategories { get; set; }
        public virtual ICollection<ProductItems> ProductItems { get; set; }
        public virtual ICollection<ProductKeys> ProductKeys { get; set; }
        public virtual ICollection<ProductOrderItems> ProductOrderItems { get; set; }
        public virtual ProductsAdditionalData ProductsAdditionalData { get; set; }
    }
}
