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
    
    public partial class cbvc_khao_sat
    {
        public int id_cbvc_khao_sat { get; set; }
        public int surveyID { get; set; }
        public int id_cbvc { get; set; }
        public int is_khao_sat { get; set; }
    
        public virtual CanBoVienChuc CanBoVienChuc { get; set; }
        public virtual survey survey { get; set; }
    }
}
