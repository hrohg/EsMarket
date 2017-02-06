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
    
    public partial class EsMembers
    {
        public EsMembers()
        {
            this.AccountingRecords = new HashSet<AccountingRecords>();
            this.AccountsReceivable = new HashSet<AccountsReceivable>();
            this.CashDesk = new HashSet<CashDesk>();
            this.EsStock = new HashSet<EsStock>();
            this.Invoices = new HashSet<Invoices>();
            this.MembersBrands = new HashSet<MembersBrands>();
            this.MemberUsersRoles = new HashSet<MemberUsersRoles>();
            this.Partners = new HashSet<Partners>();
            this.ProductItems = new HashSet<ProductItems>();
            this.ProductOrder = new HashSet<ProductOrder>();
            this.Products = new HashSet<Products>();
        }
    
        public long Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ClubSixteenId { get; set; }
    
        public virtual ICollection<AccountingRecords> AccountingRecords { get; set; }
        public virtual ICollection<AccountsReceivable> AccountsReceivable { get; set; }
        public virtual ICollection<CashDesk> CashDesk { get; set; }
        public virtual ICollection<EsStock> EsStock { get; set; }
        public virtual ICollection<Invoices> Invoices { get; set; }
        public virtual ICollection<MembersBrands> MembersBrands { get; set; }
        public virtual ICollection<MemberUsersRoles> MemberUsersRoles { get; set; }
        public virtual ICollection<Partners> Partners { get; set; }
        public virtual ICollection<ProductItems> ProductItems { get; set; }
        public virtual ICollection<ProductOrder> ProductOrder { get; set; }
        public virtual ICollection<Products> Products { get; set; }
    }
}
