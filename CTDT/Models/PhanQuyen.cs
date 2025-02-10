using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CTDT.Models
{
    public class PhanQuyen
    {
        public int ma_user {  get; set; }
        public int ma_quyen {  get; set; }
        public List<int> ma_ctdt { get; set; }
        public int ma_khoa { get; set; }
        public int[] ma_chuc_nang { get; set; }
    }
}