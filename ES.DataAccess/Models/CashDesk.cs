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
    
    public partial class CashDesk
    {
        public CashDesk()
        {
            this.SaleInCash = new HashSet<SaleInCash>();
        }
    
        public System.Guid Id { get; set; }
        public int MemberId { get; set; }
        public decimal Total { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public bool IsCash { get; set; }
        public bool IsActive { get; set; }
    
        public virtual EsMembers EsMembers { get; set; }
        public virtual ICollection<SaleInCash> SaleInCash { get; set; }
    }
}
