using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CTDT.Models
{
    public class GiamSatThongKeKetQua
    {
        public int? id_hdt { get; set; }
        public int? surveyID { get; set; }
        public int? id_namhoc {  get; set; }
        public int? id_ctdt { get; set; }
        public int? id_lop { get; set; }
        public int? id_mh { get; set; }
        public int? id_CBVC { get; set; }
        public long? from_date { get; set; }
        public long? to_date { get; set; }
    }
}