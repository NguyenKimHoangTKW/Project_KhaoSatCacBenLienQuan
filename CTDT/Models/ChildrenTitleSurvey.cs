using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CTDT.Models
{
    public class ChildrenTitleSurvey
    {
        public int id_chi_tiet_cau_hoi_tieu_de { get; set; }
        public Nullable<int> thu_tu { get; set; }
        public Nullable<int> id_tieu_de_phieu { get; set; }
        public string ten_cau_hoi { get; set; }
        public Nullable<int> id_dang_cau_hoi { get; set; }
        public Nullable<int> bat_buoc { get; set; }
        public Nullable<int> is_ykienkhac { get; set; }
        public string dieu_kien_hien_thi { get; set; }
        public int id_rd_cau_hoi_khac { get; set; }
        public string ten_rd_cau_hoi_khac { get; set; }
    }
}