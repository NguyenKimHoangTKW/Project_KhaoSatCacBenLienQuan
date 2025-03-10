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
    
    public partial class CanBoVienChuc
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CanBoVienChuc()
        {
            this.answer_response = new HashSet<answer_response>();
            this.cbvc_khao_sat = new HashSet<cbvc_khao_sat>();
            this.nguoi_hoc_dang_co_hoc_phan = new HashSet<nguoi_hoc_dang_co_hoc_phan>();
        }
    
        public int id_CBVC { get; set; }
        public string MaCBVC { get; set; }
        public string TenCBVC { get; set; }
        public Nullable<System.DateTime> NgaySinh { get; set; }
        public string Email { get; set; }
        public Nullable<int> id_donvi { get; set; }
        public Nullable<int> id_chucvu { get; set; }
        public Nullable<int> id_chuongtrinhdaotao { get; set; }
        public Nullable<int> id_khoa_vien_truong { get; set; }
        public Nullable<int> id_bo_mon { get; set; }
        public Nullable<int> id_namhoc { get; set; }
        public Nullable<int> id_don_vi { get; set; }
        public Nullable<int> id_trinh_do { get; set; }
        public string nganh_dao_tao { get; set; }
        public Nullable<int> ngaytao { get; set; }
        public Nullable<int> ngaycapnhat { get; set; }
        public string description { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<answer_response> answer_response { get; set; }
        public virtual bo_mon bo_mon { get; set; }
        public virtual ChucVu ChucVu { get; set; }
        public virtual ctdt ctdt { get; set; }
        public virtual DonVi DonVi { get; set; }
        public virtual khoa_vien_truong khoa_vien_truong { get; set; }
        public virtual NamHoc NamHoc { get; set; }
        public virtual trinh_do trinh_do { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cbvc_khao_sat> cbvc_khao_sat { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<nguoi_hoc_dang_co_hoc_phan> nguoi_hoc_dang_co_hoc_phan { get; set; }
    }
}
