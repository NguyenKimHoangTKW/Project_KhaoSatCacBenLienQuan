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
    
    public partial class dang_cau_hoi
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public dang_cau_hoi()
        {
            this.chi_tiet_cau_hoi_tieu_de = new HashSet<chi_tiet_cau_hoi_tieu_de>();
        }
    
        public int id_dang_cau_hoi { get; set; }
        public string ten_dang_cau_hoi { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<chi_tiet_cau_hoi_tieu_de> chi_tiet_cau_hoi_tieu_de { get; set; }
    }
}