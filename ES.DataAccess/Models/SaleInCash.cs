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
    
    public partial class SaleInCash
    {
        public System.Guid Id { get; set; }
        public System.Guid CashDeskId { get; set; }
        public System.Guid InvoiceId { get; set; }
        public System.DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
        public int CashierId { get; set; }
        public Nullable<System.Guid> AccountingRecordsId { get; set; }
    
        public virtual CashDesk CashDesk { get; set; }
        public virtual EsUsers EsUsers { get; set; }
    }
}
