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
    
    public partial class InvoiceItems
    {
        public System.Guid Id { get; set; }
        public System.Guid InvoiceId { get; set; }
        public System.Guid ProductId { get; set; }
        public Nullable<System.Guid> ProductItemId { get; set; }
        public Nullable<short> StockId { get; set; }
        public Nullable<short> DisplayOrder { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public Nullable<decimal> Quantity { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<decimal> CostPrice { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public string Note { get; set; }
    
        public virtual Invoices Invoices { get; set; }
        public virtual Products Products { get; set; }
    }
}
