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
    
    public partial class EsPartnersTypes
    {
        public EsPartnersTypes()
        {
            this.Partners = new HashSet<Partners>();
        }
    
        public short Id { get; set; }
        public string Description { get; set; }
    
        public virtual ICollection<Partners> Partners { get; set; }
    }
}
