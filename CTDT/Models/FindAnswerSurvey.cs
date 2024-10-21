using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CTDT.Models
{
    public class FindAnswerSurvey
    {
        public int id_survey { get; set; } 
        public string name_hoc_ky { get; set; }
        public int? id_ctdt { get; set; }
    }
}