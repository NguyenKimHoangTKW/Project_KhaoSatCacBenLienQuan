using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CTDT.Models
{
    public class SaveXacThuc
    {
        public int? Id { get; set; }
        public int? nguoi_hoc { get; set; }
        public int? ctdt { get; set; }
        public int? donvi { get; set; }
        public int? id_nguoi_hoc_by_mon_hoc { get; set; }
    }
}