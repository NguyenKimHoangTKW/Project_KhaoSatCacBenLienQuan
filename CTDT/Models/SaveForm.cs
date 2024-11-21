using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CTDT.Models
{
    public class SaveForm
    {
        public int? idsurvey { get; set; }
        public int? id_mon_hoc { get; set; }
        public int? id_giang_vien { get; set; }
        public string email { get; set; }
        public string ten_giang_vien { get; set; }
        public string ma_nguoi_hoc { get; set; }
        public string ten_nguoi_hoc { get; set; }
        public string mon_hoc { get; set; }
        public string hoc_phan { get; set; }
        public string ctdt { get; set; }
        public string ten_cbvc { get; set; }
        public string don_vi { get; set; }
        public string chuc_vu { get; set; }
        public string json_answer { get; set; }
    }
}