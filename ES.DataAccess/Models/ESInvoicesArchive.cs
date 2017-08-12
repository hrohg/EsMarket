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
    
    public partial class ESInvoicesArchive
    {
        public System.Guid ID { get; set; }
        public int InvoiceTypeID { get; set; }
        public string InvoiceNumber { get; set; }
        public System.Guid FromStockID { get; set; }
        public System.Guid ToStockID { get; set; }
        public System.DateTime CreateDate { get; set; }
        public Nullable<System.Guid> CreatorID { get; set; }
        public string Creator { get; set; }
        public Nullable<System.DateTime> ApproveDate { get; set; }
        public Nullable<System.Guid> ApproverID { get; set; }
        public string Approver { get; set; }
        public Nullable<System.DateTime> AcceptDate { get; set; }
        public Nullable<System.Guid> AccepterID { get; set; }
        public string Accepter { get; set; }
        public string ProviderName { get; set; }
        public string ProviderJuridicalAddress { get; set; }
        public string ProviderAddress { get; set; }
        public string ProviderBank { get; set; }
        public string ProviderBankAccount { get; set; }
        public string ProviderTaxRegistration { get; set; }
        public string RecipientName { get; set; }
        public string RecipientJuridicalAddress { get; set; }
        public string RecipientAddress { get; set; }
        public string RecipientBank { get; set; }
        public string RecipientBankAccount { get; set; }
        public string RecipientTaxRegistration { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public string Description { get; set; }
    }
}