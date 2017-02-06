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
    
    public partial class Invoices
    {
        public Invoices()
        {
            this.InvoiceItems = new HashSet<InvoiceItems>();
        }
    
        public System.Guid Id { get; set; }
        public long MemberId { get; set; }
        public long InvoiceTypeId { get; set; }
        public long InvoiceIndex { get; set; }
        public string CashReceiptId { get; set; }
        public string InvoiceNumber { get; set; }
        public Nullable<long> FromStockId { get; set; }
        public Nullable<long> ToStockId { get; set; }
        public long CreatorId { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public Nullable<System.DateTime> ApproveDate { get; set; }
        public Nullable<long> ApproverId { get; set; }
        public string Approver { get; set; }
        public Nullable<System.DateTime> AcceptDate { get; set; }
        public Nullable<long> AccepterId { get; set; }
        public string Accepter { get; set; }
        public Nullable<System.Guid> PartnerId { get; set; }
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
        public decimal Summ { get; set; }
        public string Notes { get; set; }
    
        public virtual EsInvoiceTypes EsInvoiceTypes { get; set; }
        public virtual EsMembers EsMembers { get; set; }
        public virtual EsStock EsStock { get; set; }
        public virtual EsStock EsStock1 { get; set; }
        public virtual EsUsers EsUsers { get; set; }
        public virtual EsUsers EsUsers1 { get; set; }
        public virtual EsUsers EsUsers2 { get; set; }
        public virtual ICollection<InvoiceItems> InvoiceItems { get; set; }
    }
}
