using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CTDT.Models.Khoa
{
    public class FindChartsTyLeKhaoSat
    {
        public int id_nam_hoc { get; set; }
        public int? id_ctdt { get; set; }
        public int id_survey { get; set; }
        public int id_hoc_ky { get; set; }
    }
}