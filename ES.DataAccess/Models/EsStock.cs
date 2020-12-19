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
    
    public partial class EsStock
    {
        public EsStock()
        {
            this.EsStock1 = new HashSet<EsStock>();
            this.Invoices = new HashSet<Invoices>();
            this.Invoices1 = new HashSet<Invoices>();
            this.ProductItems = new HashSet<ProductItems>();
        }
    
        public short Id { get; set; }
        public Nullable<short> ParentStockId { get; set; }
        public Nullable<int> StorekeeperId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string SpecialCode { get; set; }
        public bool IsEnable { get; set; }
        public int EsMemberId { get; set; }
    
        public virtual EsMembers EsMembers { get; set; }
        public virtual ICollection<EsStock> EsStock1 { get; set; }
        public virtual EsStock EsStock2 { get; set; }
        public virtual EsUsers EsUsers { get; set; }
        public virtual ICollection<Invoices> Invoices { get; set; }
        public virtual ICollection<Invoices> Invoices1 { get; set; }
        public virtual ICollection<ProductItems> ProductItems { get; set; }
    }
}
