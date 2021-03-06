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
    
    public partial class Partners
    {
        public Partners()
        {
            this.AccountsReceivable = new HashSet<AccountsReceivable>();
        }
    
        public System.Guid Id { get; set; }
        public string CardNumber { get; set; }
        public int EsMemberId { get; set; }
        public Nullable<short> EsPartnersTypeId { get; set; }
        public Nullable<int> EsUserId { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public Nullable<decimal> Debit { get; set; }
        public Nullable<decimal> Credit { get; set; }
        public Nullable<decimal> MaxDebit { get; set; }
        public string TIN { get; set; }
        public string PasportData { get; set; }
        public string JuridicalAddress { get; set; }
        public string Bank { get; set; }
        public string BankAccount { get; set; }
        public string Notes { get; set; }
    
        public virtual ICollection<AccountsReceivable> AccountsReceivable { get; set; }
        public virtual EsMembers EsMembers { get; set; }
        public virtual EsPartnersTypes EsPartnersTypes { get; set; }
        public virtual EsUsers EsUsers { get; set; }
    }
}
