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
    
    public partial class thong_ke_theo_yeu_cau
    {
        public int id_thong_ke_theo_yeu_cau { get; set; }
        public Nullable<int> surveyID { get; set; }
        public Nullable<int> id_nguoi_hoc_khao_sat { get; set; }
        public Nullable<int> id_nguoi_hoc_co_hoc_phan_khao_sat { get; set; }
        public Nullable<int> id_cbvc_khao_sat { get; set; }
        public Nullable<int> id_nguoi_lap_thong_ke { get; set; }
        public Nullable<int> id_answer { get; set; }
        public Nullable<int> id_ctdt { get; set; }
        public Nullable<int> ngay_tao { get; set; }
        public Nullable<int> ngay_cap_nhat { get; set; }
    
        public virtual answer_response answer_response { get; set; }
        public virtual cbvc_khao_sat cbvc_khao_sat { get; set; }
        public virtual ctdt ctdt { get; set; }
        public virtual nguoi_hoc_dang_co_hoc_phan nguoi_hoc_dang_co_hoc_phan { get; set; }
        public virtual nguoi_hoc_khao_sat nguoi_hoc_khao_sat { get; set; }
        public virtual survey survey { get; set; }
        public virtual users users { get; set; }
    }
}
