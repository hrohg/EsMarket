﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class EsStockDBEntities : DbContext
    {
        public EsStockDBEntities()
            : base("name=EsStockDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AccountingRecords> AccountingRecords { get; set; }
        public virtual DbSet<AccountingPlan> AccountingPlan { get; set; }
        public virtual DbSet<AccountsReceivable> AccountsReceivable { get; set; }
        public virtual DbSet<Brands> Brands { get; set; }
        public virtual DbSet<CashDesk> CashDesk { get; set; }
        public virtual DbSet<EsCategories> EsCategories { get; set; }
        public virtual DbSet<EsDefaults> EsDefaults { get; set; }
        public virtual DbSet<EsInvoiceItemsArchive> EsInvoiceItemsArchive { get; set; }
        public virtual DbSet<ESInvoicesArchive> ESInvoicesArchive { get; set; }
        public virtual DbSet<EsInvoiceTypes> EsInvoiceTypes { get; set; }
        public virtual DbSet<EsMembers> EsMembers { get; set; }
        public virtual DbSet<EsMembersAccounts> EsMembersAccounts { get; set; }
        public virtual DbSet<EsPartnersTypes> EsPartnersTypes { get; set; }
        public virtual DbSet<ESSharedProducts> ESSharedProducts { get; set; }
        public virtual DbSet<EsStock> EsStock { get; set; }
        public virtual DbSet<ESUserRoles> ESUserRoles { get; set; }
        public virtual DbSet<EsUsers> EsUsers { get; set; }
        public virtual DbSet<EsUsersInRole> EsUsersInRole { get; set; }
        public virtual DbSet<InvoiceItems> InvoiceItems { get; set; }
        public virtual DbSet<Invoices> Invoices { get; set; }
        public virtual DbSet<MembersBrands> MembersBrands { get; set; }
        public virtual DbSet<MembersRoles> MembersRoles { get; set; }
        public virtual DbSet<MemberUsersRoles> MemberUsersRoles { get; set; }
        public virtual DbSet<Partners> Partners { get; set; }
        public virtual DbSet<ProductCategories> ProductCategories { get; set; }
        public virtual DbSet<ProductGroup> ProductGroup { get; set; }
        public virtual DbSet<ProductItems> ProductItems { get; set; }
        public virtual DbSet<ProductOrder> ProductOrder { get; set; }
        public virtual DbSet<ProductOrderItems> ProductOrderItems { get; set; }
        public virtual DbSet<Products> Products { get; set; }
        public virtual DbSet<SaleInCash> SaleInCash { get; set; }
        public virtual DbSet<StockTake> StockTake { get; set; }
        public virtual DbSet<StockTakeItems> StockTakeItems { get; set; }
        public virtual DbSet<SubAccountingPlan> SubAccountingPlan { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<ProductsAdditionalData> ProductsAdditionalData { get; set; }
    }
}
