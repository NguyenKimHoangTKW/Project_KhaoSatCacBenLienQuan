//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CTDT.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class dot_khao_sat
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public dot_khao_sat()
        {
            this.nguoi_hoc_dang_co_hoc_phan = new HashSet<nguoi_hoc_dang_co_hoc_phan>();
            this.survey = new HashSet<survey>();
        }
    
        public int id_dot_khao_sat { get; set; }
        public string ten_dot_khao_sat { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<nguoi_hoc_dang_co_hoc_phan> nguoi_hoc_dang_co_hoc_phan { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<survey> survey { get; set; }
    }
}