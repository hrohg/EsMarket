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
    
    public partial class MembersBrands
    {
        public System.Guid Id { get; set; }
        public int MemberId { get; set; }
        public int BrandId { get; set; }
    
        public virtual Brands Brands { get; set; }
        public virtual EsMembers EsMembers { get; set; }
    }
}
